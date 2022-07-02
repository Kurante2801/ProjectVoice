using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SettingColorElement : SettingElement
{
    public ColorPicker Picker;
    public UnityEvent<Color> OnValueChanged => Picker.OnValueChanged;
    public void SetValues(Color color, bool allowAlpha, string modalText) => Picker.SetValues(color, allowAlpha, modalText);

    private void Awake()
    {
        SettingType = SettingType.Dropdown;
    }
}
