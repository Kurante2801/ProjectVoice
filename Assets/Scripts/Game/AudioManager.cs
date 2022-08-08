using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using DG.Tweening.Plugins.Options;
using DG.Tweening.Core;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;

// Based off https://github.com/Cytoid/Cytoid/blob/main/Assets/Scripts/AudioManager.cs
// but using a free native audio because I'm poor
public static class AudioManager
{
    public static async UniTask<AudioController> LoadAudio(string path, AudioSource source, CancellationToken token = default, bool overrideNative = false)
    {
        // Android native
        if(PlayerSettings.NativeAudio.Value && !overrideNative && Application.platform == RuntimePlatform.Android)
        {
            int fileID = -2;
            bool cached = false;
            if (Context.AndroidVersionCode > 29)
            {
                cached = true;
                path = StorageUtil.CopyToCache(path);
            }

            ANAMusic.load(path, true, true, id =>
            {
                fileID = id;
                if (cached)
                    StorageUtil.DeleteFromCache(path);
            });
            try
            {
                await UniTask.WaitUntil(() => fileID != -2, cancellationToken: token);
            }
            catch(OperationCanceledException)
            {
                await UniTask.WaitUntil(() => fileID != -2);
                if(fileID >= 0) ANAMusic.release(fileID);
                throw new OperationCanceledException(token);
            }

            // Failed to load
            if(fileID == -1)
            {
                Debug.LogWarning($"Could not load {path} using ANAMusic. Loading with Unity instead.");
                return await LoadAudio(path, source, token, true);
            }

            return new NativeAudioController(fileID);
        }
        else
        {
            var clip = await AudioLoader.LoadClip(path, token);
            return new UnityAudioController(source, clip, AudioLoader.MPEGLength > 0 ? Mathf.CeilToInt((float)AudioLoader.MPEGLength * 1000f) : -1);
        }
    }
}

public abstract class AudioController
{
    public abstract float Volume { get; set; }
    public abstract int Length { get; }
    public abstract void Play(int milliseconds);
    public abstract void PlayScheduled(int delay);
    public abstract bool Paused { get; set; }
    public abstract bool Looping { get; set; }
    public abstract void Unload();
    public abstract void Seek(int milliseconds);

    public TweenerCore<float, float, FloatOptions> DOFade(float volume, float duration)
    {
        var t = DOTween.To(() => Volume, value => Volume = value, volume, duration);
        t.SetTarget(this);
        return t;
    }

    public int DOKill(bool complete = false)
    {
        return DOTween.Kill(this, complete);
    }
}

public class UnityAudioController : AudioController
{
    private AudioSource source;
    public override float Volume { get => source.volume; set => source.volume = value; }
    private int length = 0;
    public override int Length => length; // When using MPEGs on Windows, clip.Length may be wrong

    private bool paused = true;
    public override bool Paused
    {
        get => paused;
        set
        {
            paused = value;
            if (value) source.Pause();
            else source.UnPause();
        }
    }

    public override bool Looping { get => source.loop; set => source.loop = value; }

    public UnityAudioController(AudioSource source, AudioClip clip, int length = -1)
    {
        this.source = source;
        this.length = length > 0 ? length : Mathf.CeilToInt(clip.length * 1000f);
        source.clip = clip;
    }

    public override void Play(int milliseconds)
    {
        source.time = milliseconds / 1000f;
        source.Play();
        Paused = false;
    }
    public override void PlayScheduled(int delay)
    {
        source.time = 0f;
        source.PlayScheduled(AudioSettings.dspTime + delay / 1000D);
        Paused = false;
    }

    public override void Unload()
    {
        if (source == null || source.clip == null) return;
        source.Stop();
        source.clip.UnloadAudioData();
        UnityEngine.Object.Destroy(source.clip);
        source.clip = null;
    }

    public override void Seek(int milliseconds)
    {
        source.time = milliseconds / 1000f;
    }
}

public class NativeAudioController : AudioController
{
    public int FileID { get; private set; } = -2;
    private float volume = 1f;
    public override float Volume
    {
        get => volume;
        set
        {
            volume = value;
            ANAMusic.setVolume(FileID, volume);
        }
    }
    private int length = 0;
    public override int Length => length;

    private bool paused = true;
    public override bool Paused
    {
        get => paused;
        set
        {
            paused = value;
            if (!BegunPlaying) return;

            if (value) ANAMusic.pause(FileID);
            else ANAMusic.play(FileID);
        }
    }

    private bool looping = false;
    public override bool Looping
    {
        get => looping;
        set
        {
            looping = value;
            ANAMusic.setLooping(FileID, value);
        }
    }

    public NativeAudioController(int fileID)
    {
        FileID = fileID;
        length = ANAMusic.getDuration(fileID);
        ANAMusic.setPlayInBackground(fileID, false);
    }

    public override void Play(int milliseconds)
    {
        ANAMusic.seekTo(FileID, milliseconds);
        ANAMusic.play(FileID);
        BegunPlaying = true;
        paused = false;
    }

    public int ScheduleTime = 0;
    public bool BegunPlaying = false;
    // ANAMusic doesn't support scheduled play, so we'll start it manually on Game scene
    public override void PlayScheduled(int delay)
    {
        ScheduleTime = delay;
        BegunPlaying = false;
        paused = false;

        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Game")
            throw new System.NotImplementedException();
    }

    public override void Unload()
    {
        ANAMusic.release(FileID);
    }

    public override void Seek(int milliseconds)
    {
        ANAMusic.seekTo(FileID, milliseconds);
    }
}