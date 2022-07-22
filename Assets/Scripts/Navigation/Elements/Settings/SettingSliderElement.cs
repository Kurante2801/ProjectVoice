using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using UnityEngine.Events;
using System.Globalization;

public class SettingSliderElement : SettingElement
{
    public TMP_InputField InputField;
    public Slider Slider;

    public UnityEvent<float> OnValueChanged = new();

    public float Step = 0.25f, Min = 0, Max = 0;
    public int Decimals = 0;

    private float _value = 0;

    private void Awake()
    {
        SettingType = SettingType.Slider;
        Slider.onValueChanged.AddListener(ValueChanged);
        InputField.onEndEdit.AddListener(input =>
        {
            float value = float.TryParse(input, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out float result) ? result : Slider.minValue;
            InputField.SetTextWithoutNotify(value.ToString("F" + Decimals, CultureInfo.InvariantCulture));
            Slider.value = value;
        });
    }

    public void SetValues(float value, float min, float max, float step, int decimals = 2, bool wholeNumbers = false, bool textEntryEnable = true)
    {
        Slider.minValue = min;
        Slider.maxValue = max;
        Slider.SetValueWithoutNotify((float)Math.Round(value, decimals));
        Slider.wholeNumbers = wholeNumbers;

        InputField.interactable = textEntryEnable;
        InputField.SetTextWithoutNotify(value.ToString("F" + Decimals));

        Step = step;
        Min = min;
        Max = max;
    }

    /// <summary>
    /// These min and max values are enforced when the slider is changed, but visually the slider's min and max are not affected
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    public void SetRealMinMax(float min, float max)
    {
        Min = min;
        Max = max;
    }

    private void ValueChanged(float value)
    {
        float desired = (float)Math.Round(value, Decimals);
        desired = Mathf.Clamp(Mathf.Round(desired / Step) * Step, Min, Max);

        if(value != desired)
            Slider.value = desired;

        InputField.SetTextWithoutNotify(desired.ToString("F" + Decimals));

        // Avoid too many calls
        if (desired == _value) return;
        _value = desired;

        OnValueChanged?.Invoke(value);
    }

    protected override void MainColorChanged()
    {
        base.MainColorChanged();
        var colors = Slider.colors;
        colors.pressedColor = Context.MainColor;
        Slider.colors = colors;
    }
}
