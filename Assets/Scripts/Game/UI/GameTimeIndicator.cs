using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameTimeIndicator : MonoBehaviour
{
    private Slider slider;
    private Image image;

    private void Awake()
    {
        slider = GetComponent<Slider>();
        image = GetComponentInChildren<Image>();
        image.color = Color.white.WithAlpha(0f);
    }

    private void OnEnable()
    {
        Game.Instance.OnGameLoaded.AddListener(GameLoaded);
        Game.Instance.OnGameStarted.AddListener(FadeIn);
        Game.Instance.OnGameRestarted.AddListener(FadeOut);
        Game.Instance.OnGameEnded.AddListener(FadeOut);
    }

    private void OnDisable()
    {
        if (Game.Instance == null) return;
        Game.Instance.OnGameLoaded.RemoveListener(GameLoaded);
        Game.Instance.OnGameStarted.RemoveListener(FadeIn);
        Game.Instance.OnGameRestarted.RemoveListener(FadeOut);
        Game.Instance.OnGameEnded.RemoveListener(FadeOut);
    }

    private void Update()
    {
        if (Game.Instance.IsPaused) return;
        slider.value = Conductor.Instance.Time;
    }
    
    private void GameLoaded(Game game)
    {
        slider.wholeNumbers = true;
        slider.minValue = game.StartTime;
        slider.maxValue = game.EndTime;
    }

    private void FadeIn(Game game)
    {
        slider.value = 0f;
        image.DOKill();
        image.color = Color.white;
    }

    private void FadeOut(Game game)
    {
        image.DOKill();
        image.DOColor(Color.white.WithAlpha(0f), game.TransitionTime);
    }    
}
