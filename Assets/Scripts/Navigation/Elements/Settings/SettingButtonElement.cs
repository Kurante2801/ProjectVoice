using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SettingButtonElement : SettingElement
{
    public Button Button;
    public TMPro.TMP_Text Text;
    public string ButtonKey;

    public UnityEvent OnButtonClicked = new();

    private void Awake()
    {
        SettingType = SettingType.Button;
        Button.onClick.AddListener(() => OnButtonClicked?.Invoke());
    }

    public void SetLocalizationKeys(string name, string description, string buttonKey)
    {
        SetLocalizationKeys(name, description);
        ButtonKey = buttonKey;
    }

    protected override void LocalizationChanged()
    {
        base.LocalizationChanged();
        Text.text = ButtonKey.Get();
    }
}
