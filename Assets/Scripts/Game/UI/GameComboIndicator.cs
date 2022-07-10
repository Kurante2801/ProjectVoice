using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using TMPro;
using UnityEngine;

public class GameComboIndicator : MonoBehaviour
{
    [SerializeField] private RectTransform container;
    [SerializeField] private TMP_Text combo_tmp;
    private Sequence sequence;

    private void Awake()
    {
        combo_tmp.color = Color.white.WithAlpha(0f);
    }

    private void OnEnable()
    {
        Game.Instance.OnGameStarted.AddListener(FadeIn);
        Game.Instance.OnGameRestarted.AddListener(FadeOut);
        Game.Instance.OnGameEnded.AddListener(FadeOutEnd);
        Game.Instance.OnNoteJudged.AddListener(NoteJudged);
    }

    private void OnDisable()
    {
        if (Game.Instance == null) return;

        Game.Instance.OnGameStarted.RemoveListener(FadeIn);
        Game.Instance.OnGameRestarted.RemoveListener(FadeOut);
        Game.Instance.OnGameEnded.RemoveListener(FadeOutEnd);
        Game.Instance.OnNoteJudged.RemoveListener(NoteJudged);
    }

    private void NoteJudged(Game game, int _)
    {
        int combo = game.State.Combo;

        combo_tmp.DOKill();
        if (combo == 0)
            combo_tmp.DOFade(0f, 0.25f);
        else
        {
            combo_tmp.text = combo.ToString();
            combo_tmp.DOFade(1f, 0.25f);
            sequence?.Kill();
            sequence = DOTween.Sequence()
                .Append(container.DOScale(Vector2.one * 1.25f, 0.0625f).SetEase(Ease.OutCubic))
                .Append(container.DOScale(Vector2.one, 0.0625f).SetEase(Ease.OutCubic));
        }
    }

    private void FadeIn(Game game)
    {
        container.DOKill();
        combo_tmp.color = Color.white.WithAlpha(0f);
    }

    private void FadeOut(Game game)
    {
        combo_tmp.DOKill();
        combo_tmp.DOFade(0f, game.TransitionTime);
    }

    private async void FadeOutEnd(Game game)
    {
        combo_tmp.DOKill();
        await UniTask.Delay(TimeSpan.FromSeconds(game.TransitionTime * 0.5f));
        combo_tmp.DOFade(0f, game.TransitionTime * 0.5f);
    }
}
