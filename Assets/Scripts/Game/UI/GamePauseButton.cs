using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GamePauseButton : MonoBehaviour
{
    [SerializeField] private Image icon;

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
        icon.DOKill();
        icon.DOFade(1f, game.TransitionTime);
    }

    private void FadeOut(Game game)
    {
        icon.DOKill();
        icon.DOFade(0f, game.TransitionTime);
    }

    public void DoClick()
    {
        if (Game.Instance.IsPaused) return;
        print("PAUSE");
    }
}
