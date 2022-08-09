using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Pool;
using System.Threading.Tasks;
using System.Linq;
using Coffee.UIExtensions;

public class CalibrationScreen : Screen
{
    public override string GetID() => "CalibrationScreen";

    [SerializeField] private Conductor conductor;
    [SerializeField] private Texture2D pauseTex, playTex;
    [SerializeField] private RawImage toggleImage;
    [SerializeField] private TMPro.TMP_Text toggleText;
    [SerializeField] private Button toggleButton, metronomeButton, tambourineButton;
    [SerializeField] private CalibrationNote notePrefab;
    [SerializeField] private RectTransform notesContainer, poolContainer;
    [SerializeField] private SettingNumberElement offsetSetting;
    [SerializeField] private SettingBooleanElement nativeSetting;
    [SerializeField] private ParticleSystem tapFX;
    [SerializeField] private UIParticle tapFXui;

    private AudioSource metronomeSource, tambourineSource;
    private AudioController metronomeController, tambourineController;
    private CancellationTokenSource tokenSource;

    private bool playing = false, metronomeVolume = true, tambourineVolume = true, loading = true;

    private ObjectPool<CalibrationNote> notesPool;
    private List<CalibrationNote> createdNotes = new();

    protected override void Awake()
    {
        base.Awake();
        metronomeSource = gameObject.AddComponent<AudioSource>();
        tambourineSource = gameObject.AddComponent<AudioSource>();

        offsetSetting.SetValues(PlayerSettings.AudioOffset.Value, 4);
        offsetSetting.SetLocalizationKeys("OPTIONS_AUDIOOFFSET_NAME", "OPTIONS_AUDIOOFFSET_DESC");
        offsetSetting.OnValueChanged.AddListener(value =>
        {
            PlayerSettings.AudioOffset.Value = value;
            conductor.SetAudioOffset(value);
        });

        nativeSetting.SetValue(PlayerSettings.NativeAudio.Value);
        nativeSetting.SetLocalizationKeys("OPTIONS_NATIVEAUDIO_NAME", "OPTIONS_NATIVEAUDIO_DESC");
        nativeSetting.OnValueChanged.AddListener(async value =>
        {
            if (loading)
            {
                nativeSetting.SetValue(PlayerSettings.NativeAudio.Value);
                return;
            }
                
            PlayerSettings.NativeAudio.Value = value;
            loading = true;

            tokenSource?.Cancel();
            tokenSource?.Dispose();
            tokenSource = new();

            bool playing = this.playing;
            SetPlaying(false);
            metronomeController?.Unload();
            metronomeController = await LoadController("ticks.ogg", metronomeSource);
            metronomeController.Volume = metronomeVolume ? PlayerSettings.MusicVolume.Value : 0f;
            tambourineController?.Unload();
            tambourineController = await LoadController("tambourine.ogg", tambourineSource);
            tambourineController.Volume = tambourineVolume ? PlayerSettings.MusicVolume.Value : 0f;
            conductor.Load(metronomeController);
            conductor.SetAudioOffset(PlayerSettings.AudioOffset.Value);
            SetPlaying(playing);

            loading = false;
        });

        metronomeSource.playOnAwake = false;
        tambourineSource.playOnAwake = false;
        tambourineSource.volume = PlayerSettings.MusicVolume.Value;

        toggleButton.image.color = DifficultyType.Easy.GetColor();
        toggleImage.texture = playTex;

        metronomeButton.image.color = Context.MainColor;
        tambourineButton.image.color = Context.MainColor;

        notesPool = new(
            () => Instantiate(notePrefab.gameObject, notesContainer).GetComponent<CalibrationNote>(),
            note =>
            {
                note.transform.SetParent(notesContainer, false);
                note.gameObject.SetActive(true);
            },
            note =>
            {
                note.gameObject.SetActive(false);
                note.transform.SetParent(poolContainer, false);
            });
    }

    private void Update()
    {
        if (playing && conductor.Time > conductor.Controller.Length)
            SetPlaying(false);
    }

    private async UniTask<AudioController> LoadController(string fileName, AudioSource source)
    {
        var token = tokenSource.Token;
        var data = BetterStreamingAssets.ReadAllBytes("Calibration/" + fileName);
        string cache = Path.Join(StorageUtil.TemporaryCachePath, StorageUtil.RandomFilename(fileName));

        AudioController result;

        try
        {
            await File.WriteAllBytesAsync(cache, data, token);
            result = await AudioManager.LoadAudio(cache, source, token);
        }
        catch (OperationCanceledException)
        {
            StorageUtil.DeleteFromCache(cache);
            return null;
        }

        StorageUtil.DeleteFromCache(cache);
        return result;
    }

    public override async void OnScreenPostActive()
    {
        loading = true;
        tokenSource = new CancellationTokenSource();
        toggleText.text = "CALIBRATION_START".Get();

        metronomeController?.Unload();
        metronomeController = await LoadController("ticks.ogg", metronomeSource);

        tambourineController?.Unload();
        tambourineController = await LoadController("tambourine.ogg", tambourineSource);

        conductor.Load(metronomeController);
        conductor.SetAudioOffset(PlayerSettings.AudioOffset.Value);
        loading = false;

        tokenSource.Dispose();
        tokenSource = null;
    }

    public override void OnScreenBecameInactive()
    {
        if (playing)
            SetPlaying(false);

        if(tokenSource != null)
        {
            tokenSource.Cancel();
            tokenSource.Dispose();
        }

        metronomeController?.Unload();
        tambourineController?.Unload();

        base.OnScreenBecameInactive();
    }

    public void Toggle()
    {
        SetPlaying(!playing);
    }

    public void SetPlaying(bool playing)
    {
        if (playing)
        {
            conductor.Initialize();
            tambourineController.Play(0);
            metronomeController.Volume = metronomeVolume ? PlayerSettings.MusicVolume.Value : 0f;

            for(int i = 1; i <= 29; i ++)
            {
                var note = notesPool.Get();
                note.SetData(i * 2000, conductor, this);
                createdNotes.Add(note);
            }
        }
        else
        {
            conductor.Stop();
            tambourineController.Paused = true;
            metronomeController.Paused = true;

            for(int i = createdNotes.Count - 1; i > -1; i--)
            {
                var note = createdNotes[i];
                notesPool.Release(note);
                createdNotes.RemoveAt(i);
            }
        }

        UpdateButton(playing);
        this.playing = playing;
    }

    private void UpdateButton(bool playing)
    {
        toggleImage.texture = playing ? pauseTex : playTex;
        toggleButton.image.DOKill();
        toggleButton.image.DOColor(playing ? DifficultyType.Hard.GetColor() : DifficultyType.Easy.GetColor(), 0.25f);
        toggleText.text = playing ? "CALIBRATION_STOP".Get() : "CALIBRATION_START".Get();
    }

    public void MetronomeButton()
    {
        metronomeVolume = !metronomeVolume;

        metronomeController.Volume = metronomeVolume ? PlayerSettings.MusicVolume.Value : 0f;
        metronomeButton.image.DOKill();
        metronomeButton.image.DOColor(metronomeVolume ? Context.MainColor : Context.Foreground1Color, 0.25f);
    }

    public void TambourineButton()
    {
        tambourineVolume = !tambourineVolume;

        tambourineController.Volume = tambourineVolume ? PlayerSettings.MusicVolume.Value : 0f;
        tambourineButton.image.DOKill();
        tambourineButton.image.DOColor(tambourineVolume ? Context.MainColor : Context.Foreground1Color, 0.25f);
    }

    public void DisposeNote(CalibrationNote note)
    {
        createdNotes.Remove(note);
        notesPool.Release(note);
    }

    public void ReturnButton()
    {
        if (!Context.ScreenManager.TryReturnScreen())
            Context.ScreenManager.ChangeScreen("SongSelectionScreen");
    }

    public void OnTrackDown()
    {
        if (!playing) return;

        var times = new List<int>();
        for (int i = 1; i <= 29; i ++)
        {
            if(i * 2000 - conductor.Time > -300)
                times.Add(i * 2000);
        }
        if (times.Count < 1) return;

        int closest = times.OrderBy(x => Mathf.Abs(x - conductor.Time)).First();
        int diff = closest - conductor.Time;

        var transform = tapFX.transform as RectTransform;
        transform.anchoredPosition = transform.anchoredPosition.WithY(diff.MapRange(0f, Note.ScrollDurations[2], 0f, 800f.ScreenScaledY()));
        tapFX.Emit(1);
        tapFXui.scale3D = new Vector3(70f.ScreenScaledX(), 4f.ScreenScaledY(), 1f);
    }
}
