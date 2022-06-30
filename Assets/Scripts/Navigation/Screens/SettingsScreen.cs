using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum SettingType
{
    None, Dropdown
}

public class SettingsScreen : Screen
{
    public override string GetID() => "SettingsScreen";

    public RectTransform GeneralContent;

    public SettingDropdownElement SettingDropdownElementPrefab;
    public List<SettingElement> GeneralSettings = new();


    public override void OnScreenInitialized()
    {
        PopulateGeneral();
        base.OnScreenInitialized();
    }

    private void PopulateGeneral()
    {
        foreach (Transform child in GeneralContent)
            Destroy(child);

        GeneralSettings.Clear();

        var lang = Instantiate(SettingDropdownElementPrefab, GeneralContent).GetComponent<SettingDropdownElement>();
        lang.SetValues(Context.LocalizationManager.Localizations.Values.Select(localization => localization.Strings["LANGUAGE_NAME"].Capitalize()).ToArray(), "LANGUAGE_NAME".Get().Capitalize());
        lang.SetLocalizationKeys("OPTIONS_LANG_NAME", "");
        lang.OnValueChanged.AddListener((_, lang) =>
        {
            lang = lang.ToLower();
            foreach (var localization in Context.LocalizationManager.Localizations.Values)
            {
                if (localization.Strings["LANGUAGE_NAME"].ToLower() == lang)
                {
                    PlayerSettings.LanguageString = localization.Identifier;
                    break;
                }
            }
        });

        GeneralSettings.Add(lang);
    }

    public void ReturnButton()
    {
        Context.ScreenManager.ReturnScreen();
    }
}
