using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;

public class LevelSummaryScreen : Screen
{
    public override string GetID() => "LevelSummaryScreen";

    public TMP_Text Title, TitleLocalized, Artist, ArtistLocalized;
    public Level Level;

    public override void OnScreenBecameActive()
    {
        base.OnScreenBecameActive();
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
            TitleLocalized.text = "<font-weight=500>" + level.Meta.title_localized.SanitizeTMP();
        }
        else
            TitleLocalized.gameObject.SetActive(false);

        if (!string.IsNullOrEmpty(level.Meta.artist_localized))
        {
            ArtistLocalized.gameObject.SetActive(true);
            ArtistLocalized.text = "<font-weight=500>" + level.Meta.artist_localized.SanitizeTMP();
        }
        else
            ArtistLocalized.gameObject.SetActive(false);

        Backdrop.Instance.SetBackdrop(level.Path + level.Meta.background_path, level.Meta.background_aspect_ratio);
        Context.PlaySongPreview(level);
    }

    public void ReturnButton()
    {
        Backdrop.Instance.SetBackdrop(null);
        Context.StopSongPreview();
        Context.ScreenManager.ReturnScreen();
    }
}
