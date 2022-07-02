using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum SettingType
{
    None, Dropdown, Boolean, Slider, Number, Color
}

public class OptionsScreen : Screen
{
    public override string GetID() => "OptionsScreen";

    public RectTransform GeneralContent;

    public SettingDropdownElement SettingDropdownElementPrefab;
    public SettingBooleanElement SettingBooleanElementPrefab;
    public SettingSliderElement SettingSliderElementPrefab;
    public SettingNumberElement SettingNumberElementPrefab;
    public SettingColorElement SettingColorElementPrefab;

    public List<SettingElement> GeneralSettings = new();
    public List<SettingElement> NoteSettings = new();


    public override void OnScreenInitialized()
    {
        PopulateGeneral();
        PopulateNotes();
        base.OnScreenInitialized();
    }

    private void PopulateGeneral()
    {
        foreach (Transform child in GeneralContent)
            Destroy(child);

        GeneralSettings.Clear();

        var lang = Instantiate(SettingDropdownElementPrefab, GeneralContent).GetComponent<SettingDropdownElement>();
        lang.SetValues(Context.LocalizationManager.Localizations.Values.Select(localization => localization.Strings["LANGUAGE_NAME"].Capitalize()).ToArray(), Context.LocalizationManager.Localizations.Keys.ToArray(), (object)PlayerSettings.LanguageString);
        lang.SetLocalizationKeys("OPTIONS_LANG_NAME", "OPTIONS_LANG_DESC");
        lang.OnValueChanged.AddListener((_, _, key) =>
        {
            if (!Context.LocalizationManager.Localizations.TryGetValue((string)key, out var localization))
                return;
            PlayerSettings.LanguageString = localization.Identifier;
        });
        GeneralSettings.Add(lang);

        var safearea = Instantiate(SettingBooleanElementPrefab, GeneralContent).GetComponent<SettingBooleanElement>();
        safearea.SetValue(PlayerSettings.SafeArea);
        safearea.SetLocalizationKeys("OPTIONS_SAFEAREA_NAME", "OPTIONS_SAFEAREA_DESC");
        safearea.OnValueChanged.AddListener(value => PlayerSettings.SafeArea = value);
        GeneralSettings.Add(safearea);

        var targetfps = Instantiate(SettingDropdownElementPrefab, GeneralContent).GetComponent<SettingDropdownElement>();
        targetfps.SetValues(new[] { "30 FPS", "60 FPS", "120 FPS" }, new object[] { 30, 60, 120 }, PlayerSettings.TargetFPS);
        targetfps.SetLocalizationKeys("OPTIONS_TARGETFPS_NAME", "OPTIONS_TARGETFPS_DESC");
        targetfps.OnValueChanged.AddListener((_, _, value) => PlayerSettings.TargetFPS = (int)value);
        GeneralSettings.Add(targetfps);

        var music = Instantiate(SettingSliderElementPrefab, GeneralContent).GetComponent<SettingSliderElement>();
        music.SetValues(PlayerSettings.MusicVolume * 100f, 0f, 100f, 5f, 0, true, true);
        music.SetLocalizationKeys("OPTIONS_MUSICVOLUME_NAME", "OPTIONS_MUSICVOLUME_DESC");
        music.OnValueChanged.AddListener(value =>
        {
            Context.AudioSource.DOKill();
            Context.AudioSource.volume = value / 100f;
            PlayerSettings.MusicVolume = value / 100f;
        });
        GeneralSettings.Add(music);

        var dim = Instantiate(SettingSliderElementPrefab, GeneralContent).GetComponent<SettingSliderElement>();
        dim.SetValues(PlayerSettings.BackgroundDim * 100f, 0f, 100f, 5f, 0, true, true);
        dim.SetLocalizationKeys("OPTIONS_BACKGROUNDDIM_NAME", "OPTIONS_BACKGROUNDDIM_DESC");
        dim.OnValueChanged.AddListener(value => PlayerSettings.BackgroundDim = value / 100f);
        GeneralSettings.Add(dim);

        var offset = Instantiate(SettingNumberElementPrefab, GeneralContent).GetComponent<SettingNumberElement>();
        offset.SetValues(PlayerSettings.AudioOffset, 4);
        offset.SetLocalizationKeys("OPTIONS_AUDIOOFFSET_NAME", "OPTIONS_AUDIOOFFSET_DESC");
        offset.OnValueChanged.AddListener(value => PlayerSettings.AudioOffset = value);
        GeneralSettings.Add(offset);

        var test = Instantiate(SettingColorElementPrefab, GeneralContent).GetComponent<SettingColorElement>();
        test.SetValues(PlayerSettings.TestColor, false, "Test Color");
        test.SetLocalizationKeys("Test Color", "");
        test.OnValueChanged.AddListener(value => PlayerSettings.TestColor = value);
        GeneralSettings.Add(test);

        var testAlpha = Instantiate(SettingColorElementPrefab, GeneralContent).GetComponent<SettingColorElement>();
        testAlpha.SetValues(PlayerSettings.TestColorAlpha, true, "Test Color with Alpha");
        testAlpha.SetLocalizationKeys("Test Color with Alpha", "");
        testAlpha.OnValueChanged.AddListener(value => PlayerSettings.TestColorAlpha = value);
        GeneralSettings.Add(testAlpha);
    }

    private void PopulateNotes()
    {
        
    }

    public void ReturnButton()
    {
        Context.ScreenManager.ReturnScreen();
    }
}
