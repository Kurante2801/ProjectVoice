using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Conductor : SingletonMonoBehavior<Conductor>
{
    public bool Initialized { get; private set; } = false;
    public int Time { get; private set; } = 0;
    public int MinTime { get; private set; } = 0;
    public int MaxTime { get; private set; } = 1;
    public AudioController Controller;

    private bool isNative = false;
    private double offsetSeconds = 0D;
    private int offsetMilliseconds = 0;

    protected override void Awake()
    {
        base.Awake();
        isNative = PlayerSettings.NativeAudio && Application.platform == RuntimePlatform.Android;
    }

    protected override void OnDestroy()
    {
        Controller.Unload();
        base.OnDestroy();
    }

    public async UniTask Load(Level level, ChartModel chart)
    {
        Initialized = false;
        string path = level.Path + (!string.IsNullOrWhiteSpace(chart.music_override) ? chart.music_override : level.Meta.music_path);
        MinTime = chart.start_time;
        Controller = await AudioManager.LoadAudio(path, GetComponent<AudioSource>());
        isNative = Controller is NativeAudioController;

        Controller.Looping = false;
        offsetSeconds = chart.music_offset / 1000D + PlayerSettings.AudioOffset;
        offsetMilliseconds = chart.music_offset + Mathf.RoundToInt(PlayerSettings.AudioOffset * 1000f);
    }
    
    public void Initialize()
    {
        Controller.Volume = PlayerSettings.MusicVolume;

        dspTime = AudioSettings.dspTime - MinTime / 1000D;
        if (MinTime >= 0) Controller.Play(MinTime);
        else Controller.PlayScheduled(-MinTime);

        Time = MinTime;
        Initialized = true;
    }

    public void Toggle() => SetPaused(!Controller.Paused);

    private double dspTime, dspReport;
    private int positionReport;

    private void Update()
    {
        if (!Initialized || Controller.Paused) return;

        if (isNative)
        {
            // Workaround for ANAMusic not having scheduled play
            var controller = Controller as NativeAudioController;
            if (!controller.BegunPlaying)
            {
                if (Time >= 0)
                    controller.Play(0);
                else
                    Time += Mathf.RoundToInt(UnityEngine.Time.unscaledDeltaTime * 1000f);
            }
            else
            {
                if (positionReport != ANAMusic.getCurrentPosition(controller.FileID))
                {
                    positionReport = ANAMusic.getCurrentPosition(controller.FileID);
                    Time = positionReport + offsetMilliseconds;
                }
                else
                    Time += Mathf.RoundToInt(UnityEngine.Time.unscaledDeltaTime * 1000f);
            }
        }
        else
        {
            if (dspReport != AudioSettings.dspTime)
            {
                dspReport = AudioSettings.dspTime;
                Time = (int)((AudioSettings.dspTime - dspTime - offsetSeconds) * 1000D);
            }
            else
                Time += Mathf.RoundToInt(UnityEngine.Time.unscaledDeltaTime * 1000f);
        }
    }

    public void SetPaused(bool paused) => Controller.Paused = paused;
}