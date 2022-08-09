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

public class CalibrationScreen : Screen
{
    public override string GetID() => "CalibrationScreen";
    private AudioSource metronomeSource, tambourineSource;
    private Conductor conductor = new();

    [SerializeField] private Texture2D pauseTex, playTex;
    [SerializeField] private RawImage toggleImage, touchIndicatorPrefab, collectionFXprefab;
    [SerializeField] private TMPro.TMP_Text toggleText;
    [SerializeField] private Button toggleButton, metronomeButton, tambourineButton;
    [SerializeField] private CalibrationNote notePrefab;
    [SerializeField] private RectTransform notesContainer, poolContainer;
    [SerializeField] private SettingNumberElement offsetSetting;
    [SerializeField] private SettingBooleanElement nativeSetting;

    private ObjectPool<CalibrationNote> notesPool;
    private List<CalibrationNote> createdNotes = new();

    protected override void Awake()
    {
        base.Awake();

        metronomeSource = gameObject.AddComponent<AudioSource>();
        tambourineSource = gameObject.AddComponent<AudioSource>();

        metronomeSource.playOnAwake = false;
        tambourineSource.playOnAwake = false;
    }

    private void Update()
    {
        conductor.Update();
        if (conductor.Playing && conductor.MetronomeController != null && conductor.Time > conductor.MetronomeController.Length)
            Stop();
    }

    public override async void OnScreenBecameActive()
    {
        offsetSetting.SetValues(PlayerSettings.AudioOffset.Value, 4);
        offsetSetting.SetLocalizationKeys("OPTIONS_AUDIOOFFSET_NAME", "OPTIONS_AUDIOOFFSET_DESC");
        offsetSetting.OnValueChanged.AddListener(value => PlayerSettings.AudioOffset.Value = value);

        nativeSetting.SetValue(PlayerSettings.NativeAudio.Value);
        nativeSetting.SetLocalizationKeys("OPTIONS_NATIVEAUDIO_NAME", "OPTIONS_NATIVEAUDIO_DESC");
        nativeSetting.OnValueChanged.AddListener(value =>
        {
            PlayerSettings.NativeAudio.Value = value;
            Stop();
            conductor.Unload();
            _ = conductor.Load(metronomeSource, tambourineSource);
        });

        toggleButton.image.color = DifficultyType.Easy.GetColor();
        toggleImage.texture = playTex;

        toggleText.text = "CALIBRATION_START".Get();
        metronomeButton.image.color = conductor.MetronomeAudible ? Context.MainColor : Context.Foreground1Color;
        tambourineButton.image.color = conductor.TambourineAudible ? Context.MainColor : Context.Foreground1Color;

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

        base.OnScreenBecameActive();
        await conductor.Load(metronomeSource, tambourineSource);
    }

    public override void OnScreenBecameInactive()
    {
        conductor.Unload();
        base.OnScreenBecameInactive();
    }

    public void Play()
    {
        conductor.Play();
        
        for(int i = 1; i <= 29; i++)
        {
            var note = notesPool.Get();
            note.SetData(i * 2000, conductor, this);
            createdNotes.Add(note);
        }

        toggleImage.texture = pauseTex;
        toggleButton.image.DOKill();
        toggleButton.image.DOColor(DifficultyType.Hard.GetColor(), 0.25f);
        toggleText.text = "CALIBRATION_STOP".Get();
    }

    public void Stop()
    {
        conductor.Stop();
        for (int i = createdNotes.Count - 1; i > -1; i--)
        {
            var note = createdNotes[i];
            notesPool.Release(note);
            createdNotes.RemoveAt(i);
        }
        createdNotes.Clear();

        toggleImage.texture = playTex;
        toggleButton.image.DOKill();
        toggleButton.image.DOColor(DifficultyType.Easy.GetColor(), 0.25f);
        toggleText.text = "CALIBRATION_START".Get();
    }

    public void Toggle()
    {
        if (conductor.Playing)
            Stop();
        else
            Play();
    }

    public void ToggleMetronome()
    {
        conductor.MetronomeAudible = !conductor.MetronomeAudible;
        metronomeButton.image.DOKill();
        metronomeButton.image.DOColor(conductor.MetronomeAudible ? Context.MainColor : Context.Foreground1Color, 0.25f);
    }
    public void ToggleTambourine()
    {
        conductor.TambourineAudible = !conductor.TambourineAudible;
        tambourineButton.image.DOKill();
        tambourineButton.image.DOColor(conductor.TambourineAudible ? Context.MainColor : Context.Foreground1Color, 0.25f);
    }

    public void DisposeNote(CalibrationNote note)
    {
        createdNotes.Remove(note);
        notesPool.Release(note);

        var image = Instantiate(collectionFXprefab.gameObject, notesContainer).GetComponent<RawImage>();
        image.DOFade(0f, 1f).OnComplete(() => Destroy(image.gameObject));
    }

    public void OnTrackDown()
    {
        if (!conductor.Playing) return;

        var times = new List<int>();
        for (int i = 1; i <= 29; i++)
        {
            if (i * 2000 - conductor.Time > -300)
                times.Add(i * 2000);
        }
        if (times.Count < 1) return;

        int closest = times.OrderBy(x => Mathf.Abs(x - conductor.Time)).First();
        int diff = closest - conductor.Time;

        // Can't use particle systems on UI on Android
        var image = Instantiate(touchIndicatorPrefab.gameObject, notesContainer).GetComponent<RawImage>();
        image.DOFade(0f, 1f).OnComplete(() => Destroy(image.gameObject));

        var transform = image.transform as RectTransform;
        transform.anchoredPosition = transform.anchoredPosition.WithY(diff.MapRange(0f, Note.ScrollDurations[2], 0f, 800f.ScreenScaledY()));
    }

    public void ReturnButton()
    {
        Stop();
        if (!Context.ScreenManager.TryReturnScreen())
            Context.ScreenManager.ChangeScreen("OptionsScreen");
    }

    public class Conductor
    {
        public bool Playing = false;
        public AudioController MetronomeController, TambourineController;

        private bool metronomeAudible = true, tambourineAudible = true;
        public bool MetronomeAudible {
            get => metronomeAudible;
            set
            {
                metronomeAudible = value;
                if(MetronomeController != null)
                    MetronomeController.Volume = value ? PlayerSettings.MusicVolume.Value : 0f;
            }
        }
        public bool TambourineAudible
        {
            get => tambourineAudible;
            set
            {
                tambourineAudible = value;
                if (TambourineController != null)
                    TambourineController.Volume = value ? PlayerSettings.MusicVolume.Value : 0f;
            }
        }
        private CancellationTokenSource tokenSource;

        public async UniTask Load(AudioSource metronomeSource, AudioSource tambourineSource)
        {
            MetronomeController = await LoadCalibrationAudio("metronome.ogg", metronomeSource);
            MetronomeAudible = metronomeAudible;
            TambourineController = await LoadCalibrationAudio("tambourine.ogg", tambourineSource);
            TambourineAudible = tambourineAudible;
        }

        public void Unload()
        {
            MetronomeController?.Unload();
            MetronomeController = null;
            TambourineController?.Unload();
            TambourineController = null;
        }

        public void Play()
        {
            Time = 0;
            Playing = true;
            MetronomeController.Play(0);
            TambourineController.Play(0);
            dspTime = AudioSettings.dspTime;
        }

        public void Stop()
        {
            Time = 0;
            Playing = false;

            if(MetronomeController != null)
                MetronomeController.Paused = true;
            if(TambourineController != null)
                TambourineController.Paused = true;
        }

        public async UniTask<AudioController> LoadCalibrationAudio(string fileName, AudioSource source)
        {
            tokenSource?.Cancel();
            tokenSource?.Dispose();
            tokenSource = new CancellationTokenSource();

            var data = BetterStreamingAssets.ReadAllBytes("Calibration/" + fileName);
            string cache = Path.Join(StorageUtil.TemporaryCachePath, StorageUtil.RandomFilename(fileName));

            AudioController result;

            try
            {
                await File.WriteAllBytesAsync(cache, data, tokenSource.Token);
                result = await AudioManager.LoadAudio(cache, source, tokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                StorageUtil.DeleteFromCache(cache);
                return null;
            }

            StorageUtil.DeleteFromCache(cache);

            tokenSource.Dispose();
            tokenSource = null;
            return result;
        }

        private double dspTime, dspReport;
        private int positionReport;

        public int Time { get; private set; }
        public void Update()
        {
            if (!Playing || TambourineController == null) return;

            if(TambourineController is UnityAudioController)
            {
                if (dspReport != AudioSettings.dspTime)
                {
                    dspReport = AudioSettings.dspTime;
                    Time = (int)((AudioSettings.dspTime - dspTime - PlayerSettings.AudioOffset.Value) * 1000D);
                }
                else
                    Time += Mathf.RoundToInt(UnityEngine.Time.unscaledDeltaTime * 1000f);
            }
            else
            {
                var controller = TambourineController as NativeAudioController;
                if (positionReport != ANAMusic.getCurrentPosition(controller.FileID))
                {
                    positionReport = ANAMusic.getCurrentPosition(controller.FileID);
                    Time = positionReport + Mathf.RoundToInt(PlayerSettings.AudioOffset.Value * 1000f);
                }
                else
                    Time += Mathf.RoundToInt(UnityEngine.Time.unscaledDeltaTime * 1000f);
            }
        }
    }
}
