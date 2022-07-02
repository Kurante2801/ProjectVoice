using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class SettingNumberElement : SettingElement
{
    public TMP_InputField InputField;
    public UnityEvent<float> OnValueChanged = new();

    public int Decimals = 0;

    private void Awake()
    {
        SettingType = SettingType.Number;
        InputField.onEndEdit.AddListener(input =>
        {
            float value = float.TryParse(input, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out float result) ? result : 0;
            InputField.SetTextWithoutNotify(value.ToString("F" + Decimals));
            ValueChanged(value);
        });
    }

    public void SetValues(float value, int decimals = 2)
    {
        Decimals = decimals;
        InputField.SetTextWithoutNotify(value.ToString("F" + decimals, CultureInfo.InvariantCulture));
    }

    private void ValueChanged(float value)
    {
        float desired = (float)Math.Round(value, Decimals);
        InputField.SetTextWithoutNotify(desired.ToString("F" + Decimals, CultureInfo.InvariantCulture));
        OnValueChanged?.Invoke(desired);
    }
}
