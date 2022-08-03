using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelCard : MonoBehaviour
{
    //public Canvas Canvas;
    public RawImage Background;
    public AspectRatioFitter BackgroundFitter;
    public TMP_Text Title, TitleLocalized, Artist, ArtistLocalized;
    public Level Level;
    public RectTransform DifficultyContainer;
    public GameObject DifficultyBoxPrefab;

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

        foreach (Transform child in DifficultyContainer)
            Destroy(child.gameObject);

        // Load difficulty boxes
        foreach (var chart in level.Meta.charts)
        {
            var gameObject = Instantiate(DifficultyBoxPrefab, DifficultyContainer);
            gameObject.GetComponent<Image>().color = chart.type.GetColor();

            var diff = chart.difficulty;
            if(diff > 17)
                gameObject.GetComponentInChildren<TMP_Text>().text = "17+";
            else
                gameObject.GetComponentInChildren<TMP_Text>().text = chart.difficulty.ToString();
        }

        LoadBackground();
    }

    private async void LoadBackground()
    {
        if (!StorageUtil.GetSubfilePath(Level.Path, Level.Meta.background_path, out string background)) return;
        Background.DOKill();
        Background.color = Color.white.WithAlpha(0f);

        var texture = await TextureExtensions.LoadTexture(background);
        if (texture == null) return;

        Background.texture = texture;
        BackgroundFitter.aspectRatio = Level.Meta.background_aspect_ratio ?? texture.width / (float)texture.height;
        Background.DOFade(1f, 0.25f);
    }

    public void DoClick()
    {
        Context.SelectedLevel = Level;
        Context.ScreenManager.ChangeScreen("LevelSummaryScreen");
    }
}
