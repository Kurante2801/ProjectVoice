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

    public async UniTask Load(Level level, ChartModel chart, bool overrideNative = false)
    {
        Initialized = false;
        string path = level.Path + (!string.IsNullOrWhiteSpace(chart.music_override) ? chart.music_override : level.Meta.music_path);
        MinTime = chart.start_time;

        if (isNative && !overrideNative)
        {
            int fileID = -2;
            ANAMusic.load(path, true, true, id => fileID = id);
            await UniTask.WaitUntil(() => fileID != -2);
            // Failed to load
            if(fileID == -1)
            {
                Debug.LogWarning("Could not load music file with ANAMusic, attempting Unity Audio");
                await Load(level, chart, true);
                return;
            }

            Controller = new NativeAudioController(fileID);
            this.fileID = fileID;
        }
        else
        {
            var clip = await AudioLoader.LoadClip(path);
            int length = AudioLoader.MPEGLength > 0 ? Mathf.CeilToInt((float)AudioLoader.MPEGLength * 1000f) : -1;

            Controller = new UnityAudioController(GetComponent<AudioSource>(), clip, length);
            isNative = false;
        }

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
    private int positionReport, fileID;

    private void Update()
    {
        if (!Initialized || Controller.Paused) return;

        if (isNative)
        {
            if (positionReport != ANAMusic.getCurrentPosition(fileID))
            {
                positionReport = ANAMusic.getCurrentPosition(fileID);
                Time = positionReport + offsetMilliseconds;
            }
            else
                Time += Mathf.RoundToInt(UnityEngine.Time.unscaledDeltaTime * 1000f);
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