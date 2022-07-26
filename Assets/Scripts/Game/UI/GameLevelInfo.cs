using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameLevelInfo : MonoBehaviour
{
    [SerializeField] private TMP_Text title, difficulty;

    private void Awake()
    {
        title.color = Color.white.WithAlpha(0f);
        difficulty.color = Color.white.WithAlpha(0f);
    }

    private void OnEnable()
    {
        Game.Instance.OnGameStarted.AddListener(FadeIn);
        Game.Instance.OnGameRestarted.AddListener(FadeOut);
        Game.Instance.OnGameEnded.AddListener(FadeOut);
    }

    private void OnDisable()
    {
        if (Game.Instance == null) return;

        Game.Instance.OnGameStarted.RemoveListener(FadeIn);
        Game.Instance.OnGameRestarted.RemoveListener(FadeOut);
        Game.Instance.OnGameEnded.RemoveListener(FadeOut);
    }

    private void FadeIn(Game game)
    {
        title.text = Context.SelectedLevel.Meta.title;
        var chart = Context.SelectedChart;
        difficulty.text = $"{chart.name.SanitizeTMP()} <font-weight=500>{chart.difficulty}";
        difficulty.color = chart.type.GetColor().WithAlpha(0f);

        title.DOKill();
        difficulty.DOKill();
        title.DOFade(1f, game.TransitionTime);
        difficulty.DOFade(1f, game.TransitionTime);
    }

    private void FadeOut(Game game)
    {
        title.DOKill();
        difficulty.DOKill();
        title.DOFade(0f, game.TransitionTime);
        difficulty.DOFade(0f, game.TransitionTime);
    }
}
