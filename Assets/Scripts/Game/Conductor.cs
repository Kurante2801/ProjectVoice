using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Conductor : SingletonMonoBehavior<Conductor>
{
    public AudioSource Source { get; private set; }

    public double AudioPos { get; private set; }
    private double lastReport;
    private double dspTime;

    public double AudioOffset = 0.0;
    public double StartTime = 0.0;

    public double MinTime => StartTime;
    public double MaxTime = -1;

    public bool Initialized { get; private set; }

    public int Time => (int)(AudioPos * 1000);

    protected override void Awake()
    {
        base.Awake();
        Source = gameObject.GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (AudioListener.pause || !Initialized) return;
        if (lastReport != AudioSettings.dspTime)
        {
            AudioPos = AudioSettings.dspTime - dspTime - AudioOffset;
            lastReport = AudioSettings.dspTime;
        }
        else
            AudioPos += UnityEngine.Time.unscaledDeltaTime;
    }

    public void Initialize()
    {
        dspTime = AudioSettings.dspTime + StartTime;
        if (StartTime >= 0.0)
        {
            Source.time = (float)StartTime;
            Source.Play();
        }
        else
            Source.PlayScheduled(dspTime);


        AudioListener.pause = false;
        Initialized = true;
    }

    public async UniTask Load(Level level, ChartModel chart)
    {
        StartTime = chart.start_time / 1000.0;
        AudioOffset = chart.music_offset / 1000.0 + PlayerSettings.AudioOffset;
        Source.volume = PlayerSettings.MusicVolume;
        Source.clip = await AudioLoader.LoadClip(level.Path + (!string.IsNullOrWhiteSpace(chart.music_override) ? chart.music_override : level.Meta.music_path));

        // Fix wrong time
        if (AudioLoader.MPEGLength > 0)
            MaxTime = AudioLoader.MPEGLength;
        else
            MaxTime = Source.clip.length;
    }
        
    public void Toggle() => AudioListener.pause = !AudioListener.pause;
}
