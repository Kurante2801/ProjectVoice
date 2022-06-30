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

        //var path = Path.Combine(Application.streamingAssetsPath, "Localizations");

        /*foreach (string filepath in Directory.GetFiles(path, "*.json"))
         {
            using var request = UnityWebRequest.Get(filepath);
            await request.SendWebRequest();
        
            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                throw new Exception(request.error);
        
            var localization = new Localization
            {
                Identifier = Path.GetFileNameWithoutExtension(filepath),
                Strings = JsonConvert.DeserializeObject<Dictionary<string, string>>(DownloadHandlerBuffer.GetContent(request))
            };
        
            Localizations[localization.Identifier] = localization;
        }*/
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
