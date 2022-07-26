using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using DG.Tweening;

public class DifficultyButton : MonoBehaviour
{
    public Button Button;
    public TMP_Text TMP;
    public ChartSection Chart;

    [HideInInspector] public LevelSummaryScreen SummaryScreen;

    public void SetChart(ChartSection chart)
    {
        Chart = chart;
        TMP.text = $"{chart.name} {chart.difficulty}";
        UpdateColors(false);
    }

    public void UpdateColors(bool active)
    {
        var color = Chart.type.GetColor();
        Button.image.color = active ? color : Context.Foreground1Color;
        TMP.color = active ? Color.white : color;
    }

    public void DOColors(bool active)
    {
        var color = Chart.type.GetColor();
        Button.image.DOColor(active ? color : Context.Foreground1Color, 0.25f);
        TMP.DOColor(active ? Color.white : color, 0.25f);
    }

    public void DoClick()
    {
        SummaryScreen.DifficultyChosen(this);
    }
}
