using Cysharp.Threading.Tasks;
using DG.Tweening;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;

public class Game : SingletonMonoBehavior<Game>
{
    private new Camera camera;
    public ChartModel Chart;
    public GameState State;
    
    public string EditorLevelDirectory = "a13092021a";

    public UnityEvent<Game> OnGameLoaded = new();
    public UnityEvent<Game> OnGameStarted = new();
    public UnityEvent<Game> OnGameRestarted = new();
    public UnityEvent<Game> OnGameEnded = new();
    public UnityEvent<Game, int> OnNoteJudged = new();
    public UnityEvent<int, int> OnScreenSizeChanged = new();
    
    public List<Track> CreatedTracks = new();
    private ObjectPool<Track> tracksPool;

    // Custom ObjectPool for notes
    private List<Note> notesPool = new();

    [SerializeField] private Transform tracksContainer, poolContainer;
    [SerializeField] private Track trackPrefab;
    [SerializeField] private Note clickNotePrefab, swipeNotePrefab, slideNotePrefab, holdNotePrefab;

    public float TransitionTime = 0.5f;
    public int StartTime = 0, EndTime = 0;
    private int screenW, screenH;

    public bool IsPaused = false;

    public AnimationCurve TrackSpawnCurveWidth, TrackSpawnCurveHeight, TrackDespawnCurveWidth, TrackDespawnCurveHeight;
    public SpriteAtlas GameAtlas;
    public List<SpriteAtlas> ShapesAtlas;

    protected override void Awake()
    {
        base.Awake();
        camera = Camera.main;
        tracksPool = new(CreateTrack, OnGetTrack, OnReleaseTrack);
    }

    protected async void Start()
    {
        await UniTask.WaitUntil(() => Context.IsInitialized);
        await Initialize();
    }

    public async UniTask Initialize()
    {
        // Load test level
        if (Context.SelectedLevel == null)
        {
            var level = new Level();
            level.Path = Path.Join(Context.UserDataPath, EditorLevelDirectory) + Path.DirectorySeparatorChar;

            if (File.Exists(level.Path + "level.json"))
                level.Meta = JsonConvert.DeserializeObject<LevelMeta>(File.ReadAllText(level.Path + "level.json"));
            else
            {
                level.Meta = LegacyParser.ParseMeta(File.ReadAllText(level.Path + "songconfig.txt"));
                // Check what exists
                foreach (string extension in new[] { ".wav", ".ogg", ".mp3" })
                    if (File.Exists($"{level.Path}song_full{extension}"))
                    {
                        level.Meta.music_path = $"song_full{extension}";
                        break;
                    }
            }
            Context.SelectedLevel = level;
            Context.SelectedChart = level.Meta.charts.LastOrDefault() ?? level.Meta.charts[0];
            Backdrop.Instance.SetBackdrop(level.Path + level.Meta.background_path, level.Meta.background_aspect_ratio ?? 4f / 3f, true);

            Context.Modifiers.Add(Modifier.Auto);
        }

        // Make background stay behind sprites
        Backdrop.Instance.Canvas.renderMode = RenderMode.ScreenSpaceCamera;
        Backdrop.Instance.Canvas.worldCamera = camera;

        await Resources.UnloadUnusedAssets();

        // Load chart
        string path = Context.SelectedLevel.Path;
        var selected = Context.SelectedChart;

        
        if (selected.path.StartsWith("track_"))
            Chart = LegacyParser.ParseChart(path + selected.path, path + selected.path.Replace("track_", "note_"));
        else
            Chart = JsonConvert.DeserializeObject<ChartModel>(File.ReadAllText(path + selected.path));

        // Load audio
        await UniTask.WaitUntil(() => Conductor.Instance != null);
        await Conductor.Instance.Load(Context.SelectedLevel, Chart);

        // Ensure game doesn't end too soon (add 1 second in case I want to add an ending animation for full combo or something)
        EndTime = Mathf.CeilToInt((float)Conductor.Instance.MaxTime * 1000f);
        Chart.tracks.ForEach(track => EndTime = Mathf.Max(EndTime, track.despawn_time + track.despawn_duration + 1000));
        StartTime = Mathf.FloorToInt((float)Conductor.Instance.MinTime * 1000f);


        await Context.AudioSource.DOFade(1f, 0.25f).AsyncWaitForCompletion();
        Context.AudioSource.Stop();

        Note.SpeedIndex = Math.Clamp(PlayerSettings.NoteSpeedIndex, 0, 9);

        State = new(this);
        ScreenSizeChanged(Context.ScreenWidth, Context.ScreenHeight);
        OnGameLoaded?.Invoke(this);
        StartGame();
    }

    private void Update()
    {
        if (State == null || !State.HasStarted) return;
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Space))
        {
            RestartGame();
            return;
            //Conductor.Instance.Toggle();
            //IsPaused = !IsPaused;
        }
#endif
        // Handle screen size
        if (Context.ScreenWidth != screenW || Context.ScreenHeight != screenH)
        {
            screenW = Context.ScreenWidth;
            screenH = Context.ScreenHeight;
            ScreenSizeChanged(Context.ScreenWidth, Context.ScreenHeight);
        }
        // End game
        if(!State.IsCompleted && Conductor.Instance.Time > EndTime)
        {
            EndGame();
            return;
        }

        if (IsPaused) return;

        // Spawn notes
        foreach(var track in Chart.tracks)
        {
            if (!Conductor.Instance.Time.IsBetween(track.spawn_time, track.despawn_time)) continue;
            if (TrackExists(track)) continue;

            var spawned = tracksPool.Get();
            CreatedTracks.Add(spawned);
            spawned.Model = track;
            spawned.Initialize();
        }
    }

    public async void StartGame()
    {
        State.HasStarted = true;
        State.IsPlaying = true;

        TransitionTime = 0.5f;
        OnGameStarted?.Invoke(this);
        Backdrop.Instance.SetOverlay(PlayerSettings.BackgroundDim, TransitionTime);

        await UniTask.Delay(TimeSpan.FromSeconds(TransitionTime));
        Conductor.Instance.Initialize();
        IsPaused = false;
    }

    private void ClearGameplayElements()
    {
        ParticleManager.Instance.WipeEffects();

        for (int i = 0; i < CreatedTracks.Count; i++)
        {
            var track = CreatedTracks[i];
            for (int ii = 0; ii < track.CreatedNotes.Count; ii++)
            {
                DisposeNote(track.CreatedNotes[ii]);
            }
            track.CreatedNotes.Clear();
            tracksPool.Release(track);
        }
        CreatedTracks.Clear();
    }

    public async void RestartGame()
    {
        ClearGameplayElements();
        TransitionTime = 0.25f;
        State = null;
        OnGameRestarted?.Invoke(this);

        AudioListener.pause = true;
        IsPaused = true;
        
        await UniTask.Delay(TimeSpan.FromSeconds(TransitionTime));
        State = new(this);
        StartGame();
    }

    private async void EndGame()
    {
        ClearGameplayElements();
        State.IsCompleted = true;

        TransitionTime = 1f;
        Backdrop.Instance.BackgroundOverlay.DOFade(0f, TransitionTime);
        OnGameEnded?.Invoke(this);

        await UniTask.Delay(TimeSpan.FromSeconds(TransitionTime));
        SceneManager.LoadScene("Navigation");
    }

    public async void ExitGame()
    {
        ClearGameplayElements();
        State = null;

        TransitionTime = 1f;
        Backdrop.Instance.BackgroundOverlay.DOFade(0f, TransitionTime);
        OnGameEnded?.Invoke(this);

        await UniTask.Delay(TimeSpan.FromSeconds(TransitionTime));
        SceneManager.LoadScene("Navigation");
    }

    private Track CreateTrack()
    {
        return Instantiate(trackPrefab.gameObject, tracksContainer).GetComponent<Track>();
    }

    private void OnGetTrack(Track track)
    {
        track.transform.parent = tracksContainer;
        track.gameObject.SetActive(true);
    }

    private void OnReleaseTrack(Track track)
    {
        track.gameObject.SetActive(false);
        track.transform.parent = poolContainer;
        track.Model = null;
    }

    public void DisposeTrack(Track track)
    {
        CreatedTracks.Remove(track);
        tracksPool.Release(track);
    }

    public void DisposeNote(Note note)
    {
        note.gameObject.SetActive(false);
        note.transform.parent = poolContainer;
        note.Model = null;
        notesPool.Add(note);
    }

    public Note GetPooledNote(NoteType type, Transform parent)
    {
        for (int i = 0; i < notesPool.Count; i++)
        {
            var note = notesPool[i];
            if(note.Type == type)
            {
                note.transform.parent = parent;
                note.gameObject.SetActive(true);
                notesPool.Remove(note);
                return note;
            }
        }

        var prefab = type switch
        {
            NoteType.Slide => slideNotePrefab,
            NoteType.Swipe => swipeNotePrefab,
            NoteType.Hold => holdNotePrefab,
            _ => clickNotePrefab
        };

        return Instantiate(prefab, parent);
    }

    private void ScreenSizeChanged(int w, int h)
    {
        OnScreenSizeChanged?.Invoke(w, h);
        // To make the track's size accurate, we resize the camera to be the size of the screen / 10f
        // I don't know how much performance it costs to make the camera large since there's no documentation about that...
        // so I'm making it screen / 10f just in case it does have a performance impact.
        camera.orthographicSize = h * 0.05f;
        camera.transform.position = new Vector3(w * 0.05f, h * 0.05f, -10f);
        
        Track.ScreenMargin = 12f.ScreenScaledX();
        Track.ScaleY = 1f.ScreenScaledY();
        Track.WorldY = 11.9f.ScreenScaledY();
        Track.MarginPosition = (Context.ScreenWidth * 0.1f - Track.ScreenMargin * 2);
        Track.BackgroundWorldWidth = (Context.ScreenWidth / 136f) * Track.ScreenWidth;
        Track.LineWorldWidth = (Context.ScreenWidth / 6f) * Track.ScreenWidth;
    }

    private bool TrackExists(ChartModel.TrackModel track)
    {
        foreach (var created in CreatedTracks)
            if (created.Model.id == track.id) return true;
        return false;
    }

    public void SetPaused(bool paused)
    {
        IsPaused = paused;
        AudioListener.pause = paused;
    }
}
