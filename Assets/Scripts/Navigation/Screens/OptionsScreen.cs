using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

public enum SettingType
{
    None, Dropdown, Boolean, Slider, Number, Color, Note, Button
}

public class OptionsScreen : Screen
{
    public override string GetID() => "OptionsScreen";

    public RectTransform GeneralContent, NotesContent, OthersContent;

    public List<SettingElement> Prefabs = new();
    public SettingNumberElement OffsetPrefab;

    [Tooltip("Click, Swipe Left, Swipe Right, Slide, Hold")]
    public List<SettingNoteElement> NoteSettingsPrefabs = new();

    public GameObject SeparatorPrefab;

    public override void OnScreenInitialized()
    {
        PopulateGeneral();
        PopulateNotes();
        PopulateOthers();
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
        lang.SetValues(Context.LocalizationManager.Localizations.Values.Select(localization => localization.Strings["LANGUAGE_NAME"].Capitalize()).ToArray(), Context.LocalizationManager.Localizations.Keys.ToArray(), (object)PlayerSettings.LanguageString.Value);
        lang.SetLocalizationKeys("OPTIONS_LANG_NAME", "OPTIONS_LANG_DESC");
        lang.OnValueChanged.AddListener((_, _, key) =>
        {
            if (!Context.LocalizationManager.Localizations.TryGetValue((string)key, out var localization))
                return;
            PlayerSettings.LanguageString.Value = localization.Identifier;
        });

        var graphics = Enum.GetValues(typeof(GraphicsQuality)).Cast<GraphicsQuality>().Reverse().ToArray();
        var quality = CreateSetting<SettingDropdownElement>(SettingType.Dropdown, GeneralContent);
        quality.SetValues(graphics.Select(quality => quality.GetLocalized()).ToArray(), graphics.Select(quality => (object)quality.ToString().ToLower()).ToArray(), (object)PlayerSettings.GraphicsQuality.Value);
        quality.SetLocalizationKeys("OPTIONS_GRAPHICS_NAME", "OPTIONS_GRAPHICS_DESC");
        quality.OnValueChanged.AddListener((_, _, value) =>
        {
            PlayerSettings.GraphicsQuality.Value = (string)value;
            Context.SetupResolution();
        });
        quality.OnLocalizationChanged.AddListener(() =>
        {
            quality.SetNames(graphics.Select(quality => quality.GetLocalized()).ToArray());
        });

        var renderscale = CreateSetting<SettingSliderElement>(SettingType.Slider, GeneralContent);
        renderscale.SetValues(PlayerSettings.RenderScale.Value * 100f, 0f, 100f, 5f, 0, true, false);
        renderscale.SetRealMinMax(25f, 100f);
        renderscale.SetLocalizationKeys("OPTIONS_RENDERSCALE_NAME", "OPTIONS_RENDERSCALE_DESC");
        renderscale.OnValueChanged.AddListener(value =>
        {
            PlayerSettings.RenderScale.Value = value / 100f;
            Context.SetupResolution();
        });

        var targetfps = CreateSetting<SettingDropdownElement>(SettingType.Dropdown, GeneralContent);
        targetfps.SetValues(new[] { "30 FPS", "60 FPS", "120 FPS" }, new object[] { 30, 60, 120 }, PlayerSettings.TargetFPS.Value);
        targetfps.SetLocalizationKeys("OPTIONS_TARGETFPS_NAME", "OPTIONS_TARGETFPS_DESC");
        targetfps.OnValueChanged.AddListener((_, _, value) => PlayerSettings.TargetFPS.Value = (int)value);

        var musicvolume = CreateSetting<SettingSliderElement>(SettingType.Slider, GeneralContent);
        musicvolume.SetValues(PlayerSettings.MusicVolume.Value * 100f, 0f, 100f, 5f, 0, true, true);
        musicvolume.SetLocalizationKeys("OPTIONS_MUSICVOLUME_NAME", "OPTIONS_MUSICVOLUME_DESC");
        musicvolume.OnValueChanged.AddListener(value =>
        {
            PlayerSettings.MusicVolume.Value = value / 100f;
            if (Context.AudioController == null) return;
            Context.AudioController.DOKill();
            Context.AudioController.Volume = value / 100f;
        });

        var dim = CreateSetting<SettingSliderElement>(SettingType.Slider, GeneralContent);
        dim.SetValues(PlayerSettings.BackgroundDim.Value * 100f, 0f, 100f, 5f, 0, true, true);
        dim.SetLocalizationKeys("OPTIONS_BACKGROUNDDIM_NAME", "OPTIONS_BACKGROUNDDIM_DESC");
        dim.OnValueChanged.AddListener(value => PlayerSettings.BackgroundDim.Value = value / 100f);

        var blur = CreateSetting<SettingSliderElement>(SettingType.Slider, GeneralContent);
        blur.SetValues(PlayerSettings.BackgroundBlur.Value, 0f, 24f, 6f, 0, true, false);
        blur.SetLocalizationKeys("OPTIONS_BACKGROUNDBLUR_NAME", "OPTIONS_BACKGROUNDBLUR_DESC");
        blur.OnValueChanged.AddListener(value =>
        {
            PlayerSettings.BackgroundBlur.Value = (int)value;
            Backdrop.Instance.SetBlur((int)value);
        });

        var offset = Instantiate(OffsetPrefab.gameObject, GeneralContent).GetComponent<SettingNumberElement>();
        offset.SetValues(PlayerSettings.AudioOffset.Value, 4);
        offset.SetLocalizationKeys("OPTIONS_AUDIOOFFSET_NAME", "OPTIONS_AUDIOOFFSET_DESC");
        offset.OnValueChanged.AddListener(value => PlayerSettings.AudioOffset.Value = value);
    }

    private void PopulateNotes()
    {
        foreach (Transform child in NotesContent)
            Destroy(child.gameObject);

        var speed = CreateSetting<SettingSliderElement>(SettingType.Slider, NotesContent);
        speed.SetValues(PlayerSettings.NoteSpeedIndex.Value + 1, 1, 10, 1, 1, true, false);
        speed.SetLocalizationKeys("OPTIONS_NOTESPEED_NAME", "OPTIONS_NOTESPEED_DESC");
        speed.OnValueChanged.AddListener(value => PlayerSettings.NoteSpeedIndex.Value = (int)value - 1);

        var shapes = Enum.GetValues(typeof(NoteShape)).Cast<object>().ToArray();
        var judgement = CreateSetting<SettingDropdownElement>(SettingType.Dropdown, NotesContent);
        judgement.SetValues(shapes.Cast<NoteShape>().Select(shape => shape.GetLocalized()).ToArray(), shapes, PlayerSettings.JudgementShape.Value);
        judgement.SetLocalizationKeys("OPTIONS_JUDGEMENTSHAPE_NAME", "OPTIONS_JUDGEMENTSHAPE_DESC");
        judgement.OnValueChanged.AddListener((_, _, value) => PlayerSettings.JudgementShape.Value = (NoteShape)value);
        judgement.OnLocalizationChanged.AddListener(() => judgement.SetNames(shapes.Cast<NoteShape>().Select(shape => shape.GetLocalized()).ToArray()));
        CreateSeparator(NotesContent);

        var click = Instantiate(NoteSettingsPrefabs[0].gameObject, NotesContent).GetComponent<SettingNoteElement>();
        click.SetLocalizationKeys("OPTIONS_NOTECLICK_NAME", "", "OPTIONS_NOTECLICK_BACK_MODAL", "OPTIONS_NOTECLICK_FORE_MODAL");
        click.SetValues(PlayerSettings.ClickShape.Value, PlayerSettings.ClickBackgroundColor.Value, PlayerSettings.ClickForegroundColor.Value, PlayerSettings.ClickBackgroundColor.Fallback, PlayerSettings.ClickForegroundColor.Fallback);
        click.OnShapeChanged.AddListener(value => PlayerSettings.ClickShape.Value = value);
        click.OnBackgroundChanged.AddListener(value => PlayerSettings.ClickBackgroundColor.Value = value);
        click.OnForegroundChanged.AddListener(value => PlayerSettings.ClickForegroundColor.Value = value);
        CreateSeparator(NotesContent);

        var left = Instantiate(NoteSettingsPrefabs[1].gameObject, NotesContent).GetComponent<SettingNoteElement>();
        left.SetLocalizationKeys("OPTIONS_NOTESWIPE_LEFT_NAME", "", "OPTIONS_NOTESWIPE_LEFT_BACK_MODAL", "OPTIONS_NOTESWIPE_LEFT_FORE_MODAL");
        left.SetValues(PlayerSettings.SwipeLeftShape.Value, PlayerSettings.SwipeLeftBackgroundColor.Value, PlayerSettings.SwipeLeftForegroundColor.Value, PlayerSettings.SwipeLeftBackgroundColor.Fallback, PlayerSettings.SwipeLeftForegroundColor.Fallback);
        left.OnShapeChanged.AddListener(value => PlayerSettings.SwipeLeftShape.Value = value);
        left.OnBackgroundChanged.AddListener(value => PlayerSettings.SwipeLeftBackgroundColor.Value = value);
        left.OnForegroundChanged.AddListener(value => PlayerSettings.SwipeLeftForegroundColor.Value = value);
        CreateSeparator(NotesContent);

        var right = Instantiate(NoteSettingsPrefabs[2].gameObject, NotesContent).GetComponent<SettingNoteElement>();
        right.SetLocalizationKeys("OPTIONS_NOTESWIPE_RIGHT_NAME", "", "OPTIONS_NOTESWIPE_RIGHT_BACK_MODAL", "OPTIONS_NOTESWIPE_RIGHT_FORE_MODAL");
        right.SetValues(PlayerSettings.SwipeRightShape.Value, PlayerSettings.SwipeRightBackgroundColor.Value, PlayerSettings.SwipeRightForegroundColor.Value, PlayerSettings.SwipeRightBackgroundColor.Fallback, PlayerSettings.SwipeRightForegroundColor.Fallback);
        right.OnShapeChanged.AddListener(value => PlayerSettings.SwipeRightShape.Value = value);
        right.OnBackgroundChanged.AddListener(value => PlayerSettings.SwipeRightBackgroundColor.Value = value);
        right.OnForegroundChanged.AddListener(value => PlayerSettings.SwipeRightForegroundColor.Value = value);
        CreateSeparator(NotesContent);

        var slide = Instantiate(NoteSettingsPrefabs[3].gameObject, NotesContent).GetComponent<SettingNoteElement>();
        slide.SetLocalizationKeys("OPTIONS_NOTESLIDE_NAME", "", "OPTIONS_NOTESLIDE_BACK_MODAL", "OPTIONS_NOTESLIDE_FORE_MODAL");
        slide.SetValues(PlayerSettings.SlideShape.Value, PlayerSettings.SlideBackgroundColor.Value, PlayerSettings.SlideForegroundColor.Value, PlayerSettings.SlideBackgroundColor.Fallback, PlayerSettings.SlideForegroundColor.Fallback);
        slide.OnShapeChanged.AddListener(value => PlayerSettings.SlideShape.Value = value);
        slide.OnBackgroundChanged.AddListener(value => PlayerSettings.SlideBackgroundColor.Value = value);
        slide.OnForegroundChanged.AddListener(value => PlayerSettings.SlideForegroundColor.Value = value);
        CreateSeparator(NotesContent);

        var hold = Instantiate(NoteSettingsPrefabs[4].gameObject, NotesContent).GetComponent<SettingNoteHoldElement>();
        hold.SetLocalizationKeys("OPTIONS_NOTEHOLD_NAME", "", "OPTIONS_NOTEHOLD_BACK_MODAL", "OPTIONS_NOTEHOLD_BOTTOM_FORE_MODAL", "OPTIONS_NOTEHOLD_TOP_FORE_MODAL");
        hold.SetValues(PlayerSettings.HoldShape.Value, PlayerSettings.HoldBackgroundColor.Value, PlayerSettings.HoldBottomForegroundColor.Value, PlayerSettings.HoldTopForegroundColor.Value, PlayerSettings.HoldBackgroundColor.Fallback, PlayerSettings.HoldBottomForegroundColor.Fallback, PlayerSettings.HoldTopForegroundColor.Fallback);
        hold.OnShapeChanged.AddListener(value => PlayerSettings.HoldShape.Value = value);
        hold.OnBackgroundChanged.AddListener(value => PlayerSettings.HoldBackgroundColor.Value = value);
        hold.OnBottomForegroundChanged.AddListener(value => PlayerSettings.HoldBottomForegroundColor.Value = value);
        hold.OnTopForegroundChanged.AddListener(value => PlayerSettings.HoldTopForegroundColor.Value = value);
        CreateSeparator(NotesContent);

        var tick = Instantiate(NoteSettingsPrefabs[5].gameObject, NotesContent).GetComponent<SettingNoteElement>();
        tick.SetLocalizationKeys("OPTIONS_HOLDTICK_NAME", "", "OPTIONS_HOLDTICK_BACK_MODAL", "OPTIONS_HOLDTICK_FORE_MODAL");
        tick.SetValues(NoteShape.Diamond, PlayerSettings.HoldTickBackgroundColor.Value, PlayerSettings.HoldTickForegroundColor.Value, PlayerSettings.HoldTickBackgroundColor.Fallback, PlayerSettings.HoldTickForegroundColor.Fallback, true);
        tick.OnBackgroundChanged.AddListener(value => PlayerSettings.HoldTickBackgroundColor.Value = value);
        tick.OnForegroundChanged.AddListener(value => PlayerSettings.HoldTickForegroundColor.Value = value);
    }

    private void PopulateOthers()
    {
        foreach (Transform child in OthersContent)
            Destroy(child.gameObject);

        var storage = CreateSetting<SettingButtonElement>(SettingType.Button, OthersContent);
        storage.SetLocalizationKeys("OPTIONS_STORAGE_NAME", "OPTIONS_STORAGE_DESC", "OPTIONS_STORAGE_BUTTON");
        storage.OnButtonClicked.AddListener(() => Context.ScreenManager.ChangeScreen("FolderAccessScreen"));

        var safearea = CreateSetting<SettingBooleanElement>(SettingType.Boolean, OthersContent);
        safearea.SetValue(PlayerSettings.SafeArea.Value);
        safearea.SetLocalizationKeys("OPTIONS_SAFEAREA_NAME", "OPTIONS_SAFEAREA_DESC");
        safearea.OnValueChanged.AddListener(value => PlayerSettings.SafeArea.Value = value);

        var nativeaudio = CreateSetting<SettingBooleanElement>(SettingType.Boolean, OthersContent);
        nativeaudio.SetValue(PlayerSettings.NativeAudio.Value);
        nativeaudio.SetLocalizationKeys("OPTIONS_NATIVEAUDIO_NAME", "OPTIONS_NATIVEAUDIO_DESC");
        nativeaudio.OnValueChanged.AddListener(value =>
        {
            PlayerSettings.NativeAudio.Value = value;
            if (Context.AudioController != null && Context.SelectedLevel != null)
            {
                Context.StopSongPreview();
                Context.PlaySongPreview(Context.SelectedLevel);
            }
            Context.SetupProfiler();
        });

        var profiler = CreateSetting<SettingBooleanElement>(SettingType.Boolean, OthersContent);
        profiler.SetValue(PlayerSettings.Profiler.Value);
        profiler.SetLocalizationKeys("OPTIONS_PROFILER_NAME", "OPTIONS_PROFILER_DESC");
        profiler.OnValueChanged.AddListener(value => PlayerSettings.Profiler.Value = value);

        var debugtracks = CreateSetting<SettingBooleanElement>(SettingType.Boolean, OthersContent);
        debugtracks.SetValue(PlayerSettings.DebugTracks.Value);
        debugtracks.SetLocalizationKeys("OPTIONS_DEBUGTRACKS_NAME", "OPTIONS_DEBUGTRACKS_DESC");
        debugtracks.OnValueChanged.AddListener(value => PlayerSettings.DebugTracks.Value = value);

        var debugjudgements = CreateSetting<SettingBooleanElement>(SettingType.Boolean, OthersContent);
        debugjudgements.SetValue(PlayerSettings.DebugJudgements.Value);
        debugjudgements.SetLocalizationKeys("OPTIONS_DEBUGJUDGEMENTS_NAME", "OPTIONS_DEBUGJUDGEMENTS_DESC");
        debugjudgements.OnValueChanged.AddListener(value => PlayerSettings.DebugJudgements.Value = value);
    }
    
    public void ReturnButton()
    {
        if (!Context.ScreenManager.TryReturnScreen(simultaneous: true))
        {
            Debug.Log("This shouldn't be called");
            Context.ScreenManager.ChangeScreen("LevelSelectionScreen", simultaneous: true);
        }
    }
}
