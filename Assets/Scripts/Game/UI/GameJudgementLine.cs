using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameJudgementLine : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        spriteRenderer.color = Color.white.WithAlpha(0f);
        transform.localScale = new Vector3(0f, 0.2f, 1f);
    }

    private void OnEnable()
    {
        Game.Instance.OnScreenSizeChanged.AddListener(ScreenSizeChanged);
        Game.Instance.OnGameStarted.AddListener(TransitionIn);
        Game.Instance.OnGameRestarted.AddListener(TransitionOut);
        Game.Instance.OnGameEnded.AddListener(TransitionOut);
    }

    private void OnDisable()
    {
        if (Game.Instance == null) return;
        
        Game.Instance.OnScreenSizeChanged.RemoveListener(ScreenSizeChanged);
        Game.Instance.OnGameStarted.RemoveListener(TransitionIn);
        Game.Instance.OnGameRestarted.RemoveListener(TransitionOut);
        Game.Instance.OnGameEnded.RemoveListener(TransitionOut);
    }

    private void ScreenSizeChanged(int w, int h)
    {
        transform.position = new Vector3(w * 0.05f, 12f.ScreenScaledY(), 1f);
        transform.localScale = new Vector3(3200f.ScreenScaledX(), 5f.ScreenScaledY(), 1f);
    }

    private void TransitionIn(Game game)
    {
        transform.DOKill();
        transform.localScale = new Vector3(0f, 5f.ScreenScaledY(), 1f);
        transform.DOScaleX(3200f.ScreenScaledX(), game.TransitionTime);

        spriteRenderer.DOKill();
        spriteRenderer.color = Color.white;
    }

    private void TransitionOut(Game game)
    {
        spriteRenderer.DOKill();
        spriteRenderer.DOFade(0f, game.TransitionTime);
    }
}
