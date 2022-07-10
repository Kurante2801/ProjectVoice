using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePauseMenu : MonoBehaviour
{
    [SerializeField] private CanvasGroup CanvasGroup;
    [SerializeField] private RectTransform MenuTransform;

    private void Awake()
    {
        CanvasGroup.gameObject.SetActive(false);
    }

    public void Show()
    {
        CanvasGroup.alpha = 0f;
        CanvasGroup.gameObject.SetActive(true);
        MenuTransform.RebuildLayout();

        CanvasGroup.DOKill();
        CanvasGroup.DOFade(1f, 0.25f);
        CanvasGroup.interactable = true;
    }

    public async UniTask Hide()
    {
        CanvasGroup.DOKill();
        CanvasGroup.interactable = false;
        await CanvasGroup.DOFade(0f, 0.25f).AsyncWaitForCompletion();
        CanvasGroup.gameObject.SetActive(false);
    }

    public async void Resume()
    {
        await Hide();
        await UniTask.Delay(500);
        Game.Instance.SetPaused(false);
    }

    public void Retry()
    {
        _ = Hide();
        Game.Instance.RestartGame();
    }

    public void Exit()
    {
        _ = Hide();
        Game.Instance.ExitGame();
    }

    public void Pause()
    {
        if (Game.Instance.IsPaused) return;

        Game.Instance.SetPaused(true);
        Show();
    }
}
