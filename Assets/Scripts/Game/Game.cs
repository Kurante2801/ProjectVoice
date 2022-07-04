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

public class Game : SingletonMonoBehavior<Game>
{
    public Camera Camera;
    public ChartModel Chart;
    public GameState State;
    
    public string EditorLevelDirectory = "a13092021a";

    public UnityEvent<Game> OnGameLoaded = new();
    public UnityEvent<Game> OnGameStarted = new();
    public UnityEvent<Game> OnGameRestarted = new();
    public UnityEvent<Game> OnGameEnded = new();
    public UnityEvent<int, int> OnScreenSizeChanged = new();

    private List<Track> createdTracks = new();
    private ObjectPool<Track> tracksPool;
    [SerializeField] private Transform tracksContainer, poolContainer;
    [SerializeField] private Track trackPrefab;

    public float TransitionTime = 0.5f;
    public int StartTime = 0, EndTime = 0;
    private int screenW, screenH;

    protected override void Awake()
    {
        base.Awake();
        Camera = Camera.main;
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
            Backdrop.Instance.SetBackdrop(level.Path + level.Meta.background_path, level.Meta.background_aspect_ratio ?? 4f / 3f);
        }

        // Make background stay behind sprites
        Backdrop.Instance.Canvas.renderMode = RenderMode.ScreenSpaceCamera;
        Backdrop.Instance.Canvas.worldCamera = Camera;

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
        
        State = new(this);
        OnGameLoaded?.Invoke(this);
        ScreenSizeChanged(Context.ScreenWidth, Context.ScreenHeight);
        StartGame();
    }

    private void Update()
    {
        if (State == null || !State.Started) return;
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Space))
            Conductor.Instance.Toggle();
#endif
        // Handle screen size
        if (Context.ScreenWidth != screenW || Context.ScreenHeight != screenH)
        {
            screenW = Context.ScreenWidth;
            screenH = Context.ScreenHeight;
            ScreenSizeChanged(Context.ScreenWidth, Context.ScreenHeight);
        }
        // End game
        if(!State.Completed && Conductor.Instance.Time > EndTime)
        {
            EndGame();
            return;
        }

        // Spawn notes
        foreach(var track in Chart.tracks)
        {
            if (!Conductor.Instance.Time.IsBetween(track.spawn_time, track.despawn_time)) continue;
            if (TrackExists(track)) continue;

            var obj = tracksPool.Get();
            obj.Model = track;
            obj.Initialize();
            createdTracks.Add(obj);
        }
    }

    protected virtual void StartGame()
    {
        State.Started = true;
        State.Playing = true;

        Conductor.Instance.Initialize();
        OnGameStarted?.Invoke(this);
    }

    private async void EndGame()
    {
        State.Completed = true;

        TransitionTime = 1f;
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
        createdTracks.Remove(track);
        tracksPool.Release(track);
    }

    private void ScreenSizeChanged(int w, int h)
    {
        OnScreenSizeChanged?.Invoke(w, h);
        // To make the track's size accurate, we resize the camera to be the size of the screen / 10f
        // I don't know how much performance it costs to make the camera large since there's no documentation about that...
        // so I'm making it screen / 10f just in case it does have a performance impact.
        Camera.orthographicSize = h * 0.05f;
        Camera.transform.position = new Vector3(w * 0.05f, h * 0.05f, -10f);
        Track.ScreenMargin = 12f / Context.ReferenceWidth * w;
        Track.ScaleY = 1f / Context.ReferenceHeight * h;
        Track.TrackWorldY = 11.9f / Context.ReferenceHeight * h;
    }

    private bool TrackExists(ChartModel.TrackModel track)
    {
        foreach (var created in createdTracks)
            if (created.Model.id == track.id) return true;
        return false;
    }
}
