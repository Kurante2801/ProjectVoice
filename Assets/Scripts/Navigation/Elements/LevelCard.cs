using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelCard : MonoBehaviour
{
    public Canvas Canvas;
    public RawImage Background;
    public AspectRatioFitter BackgroundFitter;
    public TMP_Text Title, TitleLocalized, Artist, ArtistLocalized;
    public Level Level;

    public void SetLevel(Level level)
    {
        Level = level;

        Title.text = level.Meta.title;
        Artist.text = level.Meta.artist;

        TitleLocalized.gameObject.SetActive(!string.IsNullOrWhiteSpace(level.Meta.title_localized));
        TitleLocalized.text = level.Meta.title_localized ?? "";

        ArtistLocalized.gameObject.SetActive(!string.IsNullOrWhiteSpace(level.Meta.title_localized));
        ArtistLocalized.text = level.Meta.artist_localized ?? "";

        LoadBackground();
    }

    private async void LoadBackground()
    {
        string path = Level.Path + Level.Meta.background_path;
        if (!File.Exists(path)) return;

        var texture = await TextureExtensions.LoadTexture(path);
        if (texture == null) return;

        Background.texture = texture;
        BackgroundFitter.aspectRatio = Level.Meta.background_aspect_ratio ?? texture.width / (float)texture.height;
    }

    public void DoClick()
    {
        Context.SelectedLevel = Level;
        Context.ScreenManager.ChangeScreen("LevelSummaryScreen");
    }
}
