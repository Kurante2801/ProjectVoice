using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameModifiersList : MonoBehaviour
{
    [SerializeField] private GameObject modifierPrefab;
    private List<Graphic> graphics = new();

    private void Awake()
    {
        foreach (Transform child in transform)
            Destroy(child.gameObject);
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
        Awake();
        graphics.Clear();
        foreach (var modifier in Context.Modifiers)
        {
            var obj = Instantiate(modifierPrefab, transform);

            var image = obj.GetComponent<Image>();
            image.color = image.color.WithAlpha(0f);

            var tmp = obj.GetComponentInChildren<TMPro.TMP_Text>();
            tmp.text = modifier.ToString();
            tmp.color = tmp.color.WithAlpha(0f);

            image.DOFade(0.5f, game.TransitionTime);
            tmp.DOFade(1f, game.TransitionTime);

            graphics.Add(image);
            graphics.Add(tmp);
        }

        transform.RebuildLayout();
    }

    private void FadeOut(Game game)
    {
        foreach (var graphic in graphics)
            graphic.DOFade(0f, game.TransitionTime);
    }
}
