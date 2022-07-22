using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

public enum SettingType
{
    None, Dropdown, Boolean, Slider, Number, Color, Note
}

public class OptionsScreen : Screen
{
    public override string GetID() => "OptionsScreen";

    public RectTransform GeneralContent, NoteContent;

    public List<SettingElement> Prefabs = new();

    [Tooltip("Click, Swipe Left, Swipe Right, Slide, Hold")]
    public List<SettingNoteElement> NoteSettingsPrefabs = new();

    public GameObject SeparatorPrefab;

    public override void OnScreenInitialized()
    {
        PopulateGeneral();
        PopulateNotes();
        base.OnScreenInitialized();
    }

    private T CreateSetting<T>(SettingType type, Transform parent)
    {
        foreach(var prefab in Prefabs)
        {
            if (prefab.SettingType == type)
                return Instantiate(prefab, parent).GetComponent<T>();
        }

        return default;
    }

    private void CreateSeparator(Transform parent)
    {
        Instantiate(SeparatorPrefab, parent);
    }

    private void PopulateGeneral()
    {
        foreach (Transform child in GeneralContent)
            Destroy(child.gameObject);

        var lang = CreateSetting<SettingDropdownElement>(SettingType.Dropdown, GeneralContent);
        lang.SetValues(Context.LocalizationManager.Localizations.Values.Select(localization => localization.Strings["LANGUAGE_NAME"].Capitalize()).ToArray(), Context.LocalizationManager.Localizations.Keys.ToArray(), (object)PlayerSettings.LanguageString);
        lang.SetLocalizationKeys("OPTIONS_LANG_NAME", "OPTIONS_LANG_DESC");
        lang.OnValueChanged.AddListener((_, _, key) =>
        {
            if (!Context.LocalizationManager.Localizations.TryGetValue((string)key, out var localization))
                return;
            PlayerSettings.LanguageString = localization.Identifier;
        });

        var graphics = Enum.GetValues(typeof(GraphicsQuality)).Cast<GraphicsQuality>().ToArray();
        var quality = CreateSetting<SettingDropdownElement>(SettingType.Dropdown, GeneralContent);
        quality.SetValues(graphics.Select(quality => quality.GetLocalized()).ToArray(), graphics.Select(quality => (object)quality.ToString().ToLower()).ToArray(), (object)PlayerSettings.GraphicsQuality);
        quality.SetLocalizationKeys("OPTIONS_GRAPHICS_NAME", "OPTIONS_GRAPHICS_DESC");
        quality.OnValueChanged.AddListener((_, _, value) =>
        {
            PlayerSettings.GraphicsQuality = (string)value;
            Context.SetupResolution();
        });
        quality.OnLocalizationChanged.AddListener(() =>
        {
            quality.SetNames(graphics.Select(quality => quality.GetLocalized()).ToArray());
        });

        var safearea = CreateSetting<SettingBooleanElement>(SettingType.Boolean, GeneralContent);
        safearea.SetValue(PlayerSettings.SafeArea);
        safearea.SetLocalizationKeys("OPTIONS_SAFEAREA_NAME", "OPTIONS_SAFEAREA_DESC");
        safearea.OnValueChanged.AddListener(value => PlayerSettings.SafeArea = value);

        var nativeaudio = CreateSetting<SettingBooleanElement>(SettingType.Boolean, GeneralContent);
        nativeaudio.SetValue(PlayerSettings.NativeAudio);
        nativeaudio.SetLocalizationKeys("OPTIONS_NATIVEAUDIO_NAME", "OPTIONS_NATIVEAUDIO_DESC");
        nativeaudio.OnValueChanged.AddListener(value =>
        {
            PlayerSettings.NativeAudio = value;
            if(Context.AudioController != null && Context.SelectedLevel != null)
            {
                Context.StopSongPreview();
                Context.PlaySongPreview(Context.SelectedLevel);
            }
        });

        var targetfps = CreateSetting<SettingDropdownElement>(SettingType.Dropdown, GeneralContent);
        targetfps.SetValues(new[] { "30 FPS", "60 FPS", "120 FPS" }, new object[] { 30, 60, 120 }, PlayerSettings.TargetFPS);
        targetfps.SetLocalizationKeys("OPTIONS_TARGETFPS_NAME", "OPTIONS_TARGETFPS_DESC");
        targetfps.OnValueChanged.AddListener((_, _, value) => PlayerSettings.TargetFPS = (int)value);

        var music = CreateSetting<SettingSliderElement>(SettingType.Slider, GeneralContent);
        music.SetValues(PlayerSettings.MusicVolume * 100f, 0f, 100f, 5f, 0, true, true);
        music.SetLocalizationKeys("OPTIONS_MUSICVOLUME_NAME", "OPTIONS_MUSICVOLUME_DESC");
        music.OnValueChanged.AddListener(value =>
        {
            PlayerSettings.MusicVolume = value / 100f;
            if (Context.AudioController == null) return;
            Context.AudioController.DOKill();
            Context.AudioController.Volume = value / 100f;
        });

        var dim = CreateSetting<SettingSliderElement>(SettingType.Slider, GeneralContent);
        dim.SetValues(PlayerSettings.BackgroundDim * 100f, 0f, 100f, 5f, 0, true, true);
        dim.SetLocalizationKeys("OPTIONS_BACKGROUNDDIM_NAME", "OPTIONS_BACKGROUNDDIM_DESC");
        dim.OnValueChanged.AddListener(value => PlayerSettings.BackgroundDim = value / 100f);

        var blur = CreateSetting<SettingSliderElement>(SettingType.Slider, GeneralContent);
        blur.SetValues(PlayerSettings.BackgroundBlur, 0f, 24f, 6f, 0, true, false);
        blur.SetLocalizationKeys("OPTIONS_BACKGROUNDBLUR_NAME", "OPTIONS_BACKGROUNDBLUR_DESC");
        blur.OnValueChanged.AddListener(value =>
        {
            PlayerSettings.BackgroundBlur = (int)value;
            Backdrop.Instance.SetBlur((int)value);
        });

        var offset = CreateSetting<SettingNumberElement>(SettingType.Number, GeneralContent);
        offset.SetValues(PlayerSettings.AudioOffset, 4);
        offset.SetLocalizationKeys("OPTIONS_AUDIOOFFSET_NAME", "OPTIONS_AUDIOOFFSET_DESC");
        offset.OnValueChanged.AddListener(value => PlayerSettings.AudioOffset = value);
    }

    private void PopulateNotes()
    {
        foreach (Transform child in NoteContent)
            Destroy(child.gameObject);

        var speed = CreateSetting<SettingSliderElement>(SettingType.Slider, NoteContent);
        speed.SetValues(PlayerSettings.NoteSpeedIndex + 1, 1, 10, 1, 1, true, false);
        speed.SetLocalizationKeys("OPTIONS_NOTESPEED_NAME", "OPTIONS_NOTESPEED_DESC");
        speed.OnValueChanged.AddListener(value => PlayerSettings.NoteSpeedIndex = (int)value - 1);
        CreateSeparator(NoteContent);

        var click = Instantiate(NoteSettingsPrefabs[0].gameObject, NoteContent).GetComponent<SettingNoteElement>();
        click.SetLocalizationKeys("OPTIONS_NOTECLICK_NAME", "", "OPTIONS_NOTECLICK_BACK_MODAL", "OPTIONS_NOTECLICK_FORE_MODAL");
        click.SetValues(PlayerSettings.ClickShape, PlayerSettings.ClickBackgroundColor, PlayerSettings.ClickForegroundColor);
        click.OnShapeChanged.AddListener(value => PlayerSettings.ClickShape = value);
        click.OnBackgroundChanged.AddListener(value => PlayerSettings.ClickBackgroundColor = value);
        click.OnForegroundChanged.AddListener(value => PlayerSettings.ClickForegroundColor = value);
        CreateSeparator(NoteContent);

        var left = Instantiate(NoteSettingsPrefabs[1].gameObject, NoteContent).GetComponent<SettingNoteElement>();
        left.SetLocalizationKeys("OPTIONS_NOTESWIPE_LEFT_NAME", "", "OPTIONS_NOTESWIPE_LEFT_BACK_MODAL", "OPTIONS_NOTESWIPE_LEFT_FORE_MODAL");
        left.SetValues(PlayerSettings.SwipeLeftShape, PlayerSettings.SwipeLeftBackgroundColor, PlayerSettings.SwipeLeftForegroundColor);
        left.OnShapeChanged.AddListener(value => PlayerSettings.SwipeLeftShape = value);
        left.OnBackgroundChanged.AddListener(value => PlayerSettings.SwipeLeftBackgroundColor = value);
        left.OnForegroundChanged.AddListener(value => PlayerSettings.SwipeLeftForegroundColor = value);
        CreateSeparator(NoteContent);

        var right = Instantiate(NoteSettingsPrefabs[2].gameObject, NoteContent).GetComponent<SettingNoteElement>();
        right.SetLocalizationKeys("OPTIONS_NOTESWIPE_RIGHT_NAME", "", "OPTIONS_NOTESWIPE_RIGHT_BACK_MODAL", "OPTIONS_NOTESWIPE_RIGHT_FORE_MODAL");
        right.SetValues(PlayerSettings.SwipeRightShape, PlayerSettings.SwipeRightBackgroundColor, PlayerSettings.SwipeRightForegroundColor);
        right.OnShapeChanged.AddListener(value => PlayerSettings.SwipeRightShape = value);
        right.OnBackgroundChanged.AddListener(value => PlayerSettings.SwipeRightBackgroundColor = value);
        right.OnForegroundChanged.AddListener(value => PlayerSettings.SwipeRightForegroundColor = value);
        CreateSeparator(NoteContent);

        var slide = Instantiate(NoteSettingsPrefabs[3].gameObject, NoteContent).GetComponent<SettingNoteElement>();
        slide.SetLocalizationKeys("OPTIONS_NOTESLIDE_NAME", "", "OPTIONS_NOTESLIDE_BACK_MODAL", "OPTIONS_NOTESLIDE_FORE_MODAL");
        slide.SetValues(PlayerSettings.SlideShape, PlayerSettings.SlideBackgroundColor, PlayerSettings.SlideForegroundColor);
        slide.OnShapeChanged.AddListener(value => PlayerSettings.SlideShape = value);
        slide.OnBackgroundChanged.AddListener(value => PlayerSettings.SlideBackgroundColor = value);
        slide.OnForegroundChanged.AddListener(value => PlayerSettings.SlideForegroundColor = value);
        CreateSeparator(NoteContent);

        var hold = Instantiate(NoteSettingsPrefabs[4].gameObject, NoteContent).GetComponent<SettingNoteHoldElement>();
        hold.SetLocalizationKeys("OPTIONS_NOTEHOLD_NAME", "", "OPTIONS_NOTEHOLD_BACK_MODAL", "OPTIONS_NOTEHOLD_BOTTOM_FORE_MODAL", "OPTIONS_NOTEHOLD_TOP_FORE_MODAL");
        hold.SetValues(PlayerSettings.HoldShape, PlayerSettings.HoldBackgroundColor, PlayerSettings.HoldBottomForegroundColor, PlayerSettings.HoldTopForegroundColor);
        hold.OnShapeChanged.AddListener(value => PlayerSettings.HoldShape = value);
        hold.OnBackgroundChanged.AddListener(value => PlayerSettings.HoldBackgroundColor = value);
        hold.OnBottomForegroundChanged.AddListener(value => PlayerSettings.HoldBottomForegroundColor = value);
        hold.OnTopForegroundChanged.AddListener(value => PlayerSettings.HoldTopForegroundColor = value);
    }

    public void ReturnButton()
    {
        if (!Context.ScreenManager.TryReturnScreen(simultaneous: true))
            Context.ScreenManager.ChangeScreen("SongSelectionScreen", simultaneous: true);
    }
}
