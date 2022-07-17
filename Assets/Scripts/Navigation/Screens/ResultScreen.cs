using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using System.Globalization;

public class ResultScreen : Screen
{
    public override string GetID() => "ResultScreen";

    [SerializeField] private TMP_Text songInfo, score, maxCombo, accuracy, charter;
    [SerializeField] private List<TMP_Text> noteGrades = new();
    [SerializeField] private Transform modifiersContainer;
    [SerializeField] private GameObject modifierPrefab;

    private string grayText = "<color=#CCC>", mediumText = "<color=#FFF><font-weight=500>";

    public override void OnScreenBecameActive()
    {
        var state = Context.State;
        var level = Context.SelectedLevel;
        var chart = Context.SelectedChart;

        if (state == null || !state.IsCompleted)
        {
            Debug.LogError("Entered Results Screen without a GameState!");
            return;
        }

        songInfo.text = $"{level.Meta.title.SanitizeTMP()} {grayText}- <color=#{chart.type.GetColor().ToHex()}>{chart.name.SanitizeTMP()} {chart.difficulty}";
        score.text = Mathf.FloorToInt((float)state.Score).ToString("D6");
        maxCombo.text = $"{grayText}Max Combo: {mediumText}{state.MaxCombo}";
        accuracy.text = $"{grayText}Accuracy: {mediumText}{(state.Accuracy * 100).ToString("F2", CultureInfo.InvariantCulture)}<size=26>%";

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
        {
            var grade = (NoteGrade)i;
            noteGrades[i].text = $"{grayText}{grade}: {mediumText}{state.NoteJudgements.Values.Where(judge => judge.Grade == grade).Count()}";
        }

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

}
