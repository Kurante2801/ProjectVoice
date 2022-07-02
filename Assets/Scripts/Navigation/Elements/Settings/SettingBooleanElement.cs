using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SettingBooleanElement : SettingElement
{
    public bool Value = true;
    public bool IgnoreIfNoChange = false;
    public Button TrueButton, FalseButton;
    public TMP_Text TrueText, FalseText;
    public string TrueLocalizationKey = "OPTIONS_TRUE_BOOLEAN", FalseLocalizationKey = "OPTIONS_FALSE_BOOLEAN";

    public UnityEvent<bool> OnValueChanged = new();

    private void Awake()
    {
        SettingType = SettingType.Boolean;
        TrueButton.onClick.AddListener(() => ValueChanged(true));
        FalseButton.onClick.AddListener(() => ValueChanged(false));
    }
    
    public void SetValue(bool value)
    {
        Value = value;
        MainColorChanged();
    }

    private void ValueChanged(bool value)
    {
        if (value == Value && IgnoreIfNoChange) return;

        Value = value;
        DOColors();
        OnValueChanged.Invoke(value);
    }

    protected override void MainColorChanged()
    {
        TrueButton.image.color = Value ? Context.MainColor : Context.Foreground1Color;
        FalseButton.image.color = Value ? Context.Foreground1Color : Context.MainColor;
    }

    public void DOColors()
    {
        TrueButton.image.DOColor(Value ? Context.MainColor : Context.Foreground1Color, 0.25f);
        FalseButton.image.DOColor(Value ? Context.Foreground1Color : Context.MainColor, 0.25f);
    }

    protected override void LocalizationChanged()
    {
        base.LocalizationChanged();
        TrueText.text = TrueLocalizationKey.Get();
        FalseText.text = FalseLocalizationKey.Get();
    }

    public void SetLocalizationKeys(string name, string description, string trueKey, string falseKey)
    {
        SetLocalizationKeys(name, description);
        TrueLocalizationKey = trueKey;
        FalseLocalizationKey = falseKey;
    }
}
