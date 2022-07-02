using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class LocalizationManager
{
    public string ActiveLocalization => PlayerSettings.LanguageString;
    public Localization Fallback = new(); // Creating a new to avoid exceptions when the game starts (OnEnable is called on childrens)
    public Dictionary<string, Localization> Localizations = new();

    public async UniTask Initialize()
    {
        // Load all localizations 
        foreach (string path in BetterStreamingAssets.GetFiles("Localizations", "*.json"))
        {
            using var stream = BetterStreamingAssets.OpenText(path);
            string json = await stream.ReadToEndAsync();

            var localization = new Localization
            {
                Identifier = Path.GetFileNameWithoutExtension(path),
                Strings = JsonConvert.DeserializeObject<Dictionary<string, string>>(json)
            };

            Localizations[localization.Identifier] = localization;
        }

        Fallback = Localizations["en"];
    }

    public string GetLocalized(string key, string fallback)
    {
        if (Localizations.ContainsKey(ActiveLocalization) && Localizations[ActiveLocalization].Strings.TryGetValue(key, out string localized))
            return localized;
        else if (Fallback.Strings.TryGetValue(key, out string english))
            return english;
        else
            return fallback;
    }
}

public class Localization
{
    public string Identifier;
    public Dictionary<string, string> Strings = new();
}
