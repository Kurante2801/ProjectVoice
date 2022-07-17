using DG.Tweening;
using System;
using TMPro;
using UnityEngine;

public class GameScoreIndicator : MonoBehaviour
{
    private TMP_Text tmp;
    private double score = 0D, value = 0D, _value = 0D;

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
    
    private void Update()
    {
        if (Game.Instance.IsPaused) return;

        value = MathUtil.Lerp(value, score, 0.6D);
        if (value == _value) return;

        string text = Mathf.FloorToInt((float)value).ToString("D6");

        if (value >= 100000)
            tmp.text = "<color=#FFF>" + text;
        else
        {
            // Insert white tag when leading zeros stop
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] != '0')
                {
                    tmp.text = text.Insert(i, "<color=#FFF>");
                    break;
                }
            }
        }

        _value = value;
    }

    private void NoteJudged(Game game, int _) => score = game.State.Score;

    private void FadeIn(Game game)
    {
        score = 0D;
        value = 0D;
        _value = 0D;
        tmp.text = "000000";

        tmp.DOKill();
        tmp.color = new Color32(150, 150, 150, 0);
        tmp.DOFade(1f, game.TransitionTime);
    }

    private void FadeOut(Game game)
    {
        tmp.DOKill();
        tmp.DOFade(0f, game.TransitionTime);
    }
}
