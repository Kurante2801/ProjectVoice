using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SettingColorElement : SettingElement
{
    public ColorPicker Picker;
    public UnityEvent<Color> OnValueChanged => Picker.OnValueChanged;
    public void SetValues(Color color, bool allowAlpha) => Picker.SetValues(color, allowAlpha);

    private void Awake()
    {
        SettingType = SettingType.Dropdown;
    }
}
