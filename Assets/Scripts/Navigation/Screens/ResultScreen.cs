using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using System.Globalization;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using DG.Tweening;
using UnityEngine.UI;

public class ResultScreen : Screen
{
    public override string GetID() => "ResultScreen";

    [SerializeField] private TMP_Text songInfo, rank, score, maxCombo, accuracy, charter;
    [SerializeField] private Slider accuracySlider, scoreSlider;
    [SerializeField] private List<TMP_Text> noteGrades = new();
    [SerializeField] private Transform modifiersContainer;
    [SerializeField] private GameObject modifierPrefab;

    private string grayText = "<color=#CCC>", mediumText = "<color=#FFF><font-weight=500>";

    public override void OnScreenBecameActive()
    {
        float duration = 0.75f;

        var state = Context.State;
        var level = Context.SelectedLevel;
        var chart = Context.SelectedChart;

        if (state == null || !state.IsCompleted)
        {
            Debug.LogError("Entered Results Screen without a GameState!");
            return;
        }

        songInfo.text = $"{level.Meta.title.SanitizeTMP()} {grayText}- <color=#{chart.type.GetColor().ToHex()}>{chart.name.SanitizeTMP()} {chart.difficulty}";
        rank.text = ScoreRankExtensions.FromScore(state.Score).ToString();
        
        scoreSlider.DOValue((float)state.Score, duration).SetEase(Ease.OutQuart);
        accuracySlider.DOValue((float)state.Accuracy, duration).SetEase(Ease.OutQuart);

        DOScore(score, (int)state.Score, duration).SetEase(Ease.OutQuart);
        DOFloat(maxCombo, state.MaxCombo, duration, 0).SetEase(Ease.OutQuart);
        DOFloat(accuracy, 0f, (float)state.Accuracy * 100f, duration, value => accuracy.text = value.ToString("F2", CultureInfo.InvariantCulture) + "<size=20>%").SetEase(Ease.OutQuart);

        // Optional charter credit
        if (string.IsNullOrWhiteSpace(level.Meta.charter))
            charter.gameObject.SetActive(false);
        else
        {
            charter.gameObject.SetActive(true);
            charter.text = $"{grayText}{"RESULT_CHARTER".Get().Replace("{CHARTER}", mediumText + level.Meta.charter.SanitizeTMP())}";
        }

        // Note judgement count
        for (int i = 0; i < noteGrades.Count; i++)
            DOFloat(noteGrades[i], state.NoteJudgements.Values.Where(judgement => judgement.Grade == (NoteGrade)i).Count(), duration, 0).SetEase(Ease.OutQuart);

        // Modifiers list
        foreach (Transform child in modifiersContainer)
            Destroy(child.gameObject);

        foreach (var modifier in Context.Modifiers)
        {
            var obj = Instantiate(modifierPrefab, modifiersContainer);
            var tmp = obj.GetComponentInChildren<TMP_Text>();
            tmp.text = modifier.GetName();
        }

        transform.RebuildLayout();
        base.OnScreenBecameActive();
    }

    public override void OnScreenTransitionInEnded()
    {
        if (Context.State == null || !Context.State.IsCompleted)
            Context.ScreenManager.ChangeScreen("LevelSelectionScreen");
    }

    public override void OnScreenBecameInactive()
    {
        Context.State = null;
        base.OnScreenBecameInactive();
    }

    public async void RetryButton()
    {
        Context.ScreenManager.ChangeScreen(null);
        await UniTask.Delay(TimeSpan.FromSeconds(0.3f));
        SceneManager.LoadScene("Game");
    }

    public void NextButton()
    {
        Context.ScreenManager.ChangeScreen("LevelSummaryScreen");
    }
    
    private TweenerCore<float, float, FloatOptions> DOScore(TMP_Text tmp, int endValue, float duration)
    {
        float tweening = 0;
        var t = DOTween.To(() => tweening, value => {
            tweening = value;

            string text = Mathf.FloorToInt(tweening).ToString("D6");

            if (tweening >= 100000)
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
        }, endValue, duration);

        t.SetTarget(tmp);
        return t;
    }

    private TweenerCore<float, float, FloatOptions> DOFloat(TMP_Text tmp, float endValue, float duration, int decimals)
    {
        float tweening = 0f;
        var t = DOTween.To(() => tweening, value => {
            tweening = value;
            tmp.text = value.ToString($"F{decimals}", CultureInfo.InvariantCulture);
        }, endValue, duration);

        t.SetTarget(tmp);
        return t;
    }

    private TweenerCore<float, float, FloatOptions> DOFloat(object target, float startValue, float endValue, float duration, Action<float> callback)
    {
        float tweening = startValue;
        var t = DOTween.To(() => tweening, value =>
        {
            tweening = value;
            callback(value);
        }, endValue, duration);

        t.SetTarget(target);
        return t;
    }
}
