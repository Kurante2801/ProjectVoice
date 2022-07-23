using DG.Tweening;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameFramesIndicator : MonoBehaviour
{
    private TMP_Text tmp;
    private List<float> frames = new();

    private void Awake()
    {
        tmp = GetComponent<TMP_Text>();
        tmp.color = Color.white.WithAlpha(0f);
    }

    private void OnEnable()
    {
        if (!PlayerSettings.FPSCounter) return;

        Game.Instance.OnGameStarted.AddListener(FadeIn);
        Game.Instance.OnGameRestarted.AddListener(FadeOut);
        Game.Instance.OnGameEnded.AddListener(FadeOut);
    }

    private void OnDisable()
    {
        if (!PlayerSettings.FPSCounter || Game.Instance == null) return;

        Game.Instance.OnGameStarted.RemoveListener(FadeIn);
        Game.Instance.OnGameRestarted.RemoveListener(FadeOut);
        Game.Instance.OnGameEnded.RemoveListener(FadeOut);
    }

    private void Update()
    {
        frames.Insert(0, 1f / Time.unscaledDeltaTime);
        while (frames.Count > 120)
            frames.RemoveAt(frames.Count - 1);

        float total = 0;
        foreach (float fps in frames)
            total += fps;
        total /= frames.Count;

        tmp.text = "<font-weight=500>FPS: <mspace=0.7em>" + total.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
    }

    private void FadeIn(Game game)
    {
        tmp.DOKill();
        tmp.DOFade(1f, game.TransitionTime);
    }

    private void FadeOut(Game game)
    {
        tmp.DOKill();
        tmp.DOFade(0f, game.TransitionTime);
    }
}
