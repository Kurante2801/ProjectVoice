using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

public class LevelSummaryScreen : Screen
{
    public override string GetID() => "LevelSummaryScreen";

    public TMP_Text Title, TitleLocalized, Artist, ArtistLocalized;
    public Level Level;

    public RectTransform ChartsContainer;
    public DifficultyButton DifficultyButtonPrefab;

    private List<DifficultyButton> createdButtons = new();

    [SerializeField] private CanvasGroup modifiers;
    [SerializeField] private Transform infoContent;
    [SerializeField] private LevelInfoLine infoLinePrefab;

    public override void OnScreenBecameActive()
    {
        base.OnScreenBecameActive();
        Backdrop.Instance.DisplayBlurImage(false);
        SetLevel(Context.SelectedLevel);
    }

    public void SetLevel(Level level)
    {
        Level = level;

        Title.text = level.Meta.title;
        Artist.text = level.Meta.artist;

        if (!string.IsNullOrWhiteSpace(level.Meta.title_localized))
        {
            TitleLocalized.gameObject.SetActive(true);
            TitleLocalized.text = level.Meta.title_localized;
        }
        else
            TitleLocalized.gameObject.SetActive(false);

        if (!string.IsNullOrEmpty(level.Meta.artist_localized))
        {
            ArtistLocalized.gameObject.SetActive(true);
            ArtistLocalized.text = level.Meta.artist_localized;
        }
        else
            ArtistLocalized.gameObject.SetActive(false);

        // Info tab
        foreach (Transform child in infoContent)
            Destroy(child.gameObject);

        AddInfoLine("Artist", level.Meta.artist, level.Meta.artist_source);
        AddInfoLine("Illustrator", level.Meta.illustrator, level.Meta.illustrator_source);
        AddInfoLine("Charter", level.Meta.charter);
        infoContent.transform.RebuildLayout();
        infoContent.transform.RebuildLayout();

        createdButtons.Clear();
        foreach (Transform child in ChartsContainer)
            Destroy(child.gameObject);

        foreach (var chart in level.Meta.charts)
        {
            var button = Instantiate(DifficultyButtonPrefab.gameObject, ChartsContainer).GetComponent<DifficultyButton>();
            button.SetChart(chart);
            button.SummaryScreen = this;
            createdButtons.Add(button);
        }

        // If we have multiple charts of the same difficulty type, we attempt to remember which chart was chosen
        var typeButtons = createdButtons.FindAll(button => button.Chart.type == Context.SelectedDifficultyType);
        var selected = typeButtons.ElementAtOrDefault(Context.SelectedDifficultyIndex);

        if (selected == null)
            selected = typeButtons.FirstOrDefault();
        if (selected == null)
            selected = createdButtons.First();
            
        selected.UpdateColors(true);
        Context.SelectedChart = selected.Chart;
        Context.MainColor = selected.Chart.type.GetColor();
        Context.OnMainColorChanged?.Invoke();

        if (!string.IsNullOrWhiteSpace(level.Meta.background_path))
        {
            if (StorageUtil.GetSubfilePath(level.Path, level.Meta.background_path, out string path))
                Backdrop.Instance.SetBackdrop(path, level.Meta.background_aspect_ratio ?? 4f / 3f);
            else
                Debug.LogError($"Could not find {level.Meta.background_path} inside {level.Path}");
        }
        else
            Backdrop.Instance.SetBackdrop(null);

        Context.PlaySongPreview(level);
    }

    public void ReturnButton()
    {
        Backdrop.Instance.SetBackdrop(null);
        Context.FadeOutSongPreview();

        if(!Context.ScreenManager.TryReturnScreen())
            Context.ScreenManager.ChangeScreen("LevelSelectionScreen");
    }

    public void OptionsButton()
    {
        Context.ScreenManager.ChangeScreen("OptionsScreen", simultaneous: true);
        Backdrop.Instance.DisplayBlurImage(true);
    }

    public void DifficultyChosen(DifficultyButton button)
    {
        if (Context.SelectedChart == button.Chart) return;

        var buttons = Level.Meta.charts.FindAll(chart => chart.type == button.Chart.type);
        for(int i = 0; i < buttons.Count; i++)
        {
            if (button.Chart == buttons[i])
            {
                Context.SelectedDifficultyIndex = i;
                break;
            }
        }

        foreach (var other in createdButtons)
            other.DOColors(other == button);

        Context.SelectedChart = button.Chart;
        Context.SelectedDifficultyType = button.Chart.type;
        Context.MainColor = button.Chart.type.GetColor();
        Context.OnMainColorChanged?.Invoke();
    }

    public async void GoButton()
    {
        Context.ScreenManager.ChangeScreen(null);
        Backdrop.Instance.DisplayBlurImage(true);
        await UniTask.Delay(TimeSpan.FromSeconds(0.3f));
        SceneManager.LoadScene("Game");
    }

    private void AddInfoLine(string name, string text, string url = default)
    {
        if (string.IsNullOrWhiteSpace(text)) return;
        Instantiate(infoLinePrefab.gameObject, infoContent).GetComponent<LevelInfoLine>().SetInfo(name, text.SanitizeTMP(), url);
    }
}
