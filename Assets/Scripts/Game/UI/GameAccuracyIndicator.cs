using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;

public class GameAccuracyIndicator : MonoBehaviour
{
    private TMP_Text tmp;

    private void Awake()
    {
        tmp = GetComponent<TMP_Text>();
        tmp.color = Color.clear;
    }

    private void OnEnable()
    {
        Game.Instance.OnGameStarted.AddListener(FadeIn);
        Game.Instance.OnGameRestarted.AddListener(FadeOut);
        Game.Instance.OnGameEnded.AddListener(FadeOut);
        Game.Instance.OnNoteJudged.AddListener(NoteJudged);
    }

    private void OnDisable()
    {
        if (Game.Instance == null) return;

        Game.Instance.OnGameStarted.RemoveListener(FadeIn);
        Game.Instance.OnGameRestarted.RemoveListener(FadeOut);
        Game.Instance.OnGameEnded.RemoveListener(FadeOut);
        Game.Instance.OnNoteJudged.RemoveListener(NoteJudged);
    }

    private void NoteJudged(Game game, int _)
    {
        double acc = Math.Round(game.State.Accuracy * 100D, 2);
        if (acc == 100D)
            tmp.text = "100<size=20>%";
        else
            tmp.text = acc.ToString("F2", CultureInfo.InvariantCulture) + "<size=20>%";
    }

    private void FadeIn(Game game)
    {
        tmp.text = "100<size=20>%";
        tmp.DOKill();
        tmp.color = new Color32(200, 200, 200, 0);
        tmp.DOFade(1f, game.TransitionTime);
    }

    private void FadeOut(Game game)
    {
        tmp.DOKill();
        tmp.DOFade(0f, game.TransitionTime);
    }
}
