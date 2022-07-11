using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class SettingDropdownElement : SettingElement
{
    public TMP_Dropdown Dropdown;
    public UnityEvent<int, string, object> OnValueChanged = new();
    public UnityEvent OnLocaleChanged = new();

    private string[] _values;
    private object[] _data;

    private void Awake()
    {
        SettingType = SettingType.Dropdown;
        Dropdown.onValueChanged.AddListener(ItemSelected);
    }

    private void SetValues(string[] names, object[] data)
    {
        _values = names;
        _data = data;
        Dropdown.options.Clear();
        for (int i = 0; i < names.Length; i++)
        {
            string text = names[i];
            Dropdown.options.Add(new TMP_Dropdown.OptionData(text));
        }
    }
    
    public void SetValues(string[] names, object[] data, string selected)
    {
        SetValues(names, data);

        for(int i = 0; i < names.Length; i++)
        {
            if (names[i] == selected)
                Dropdown.value = i;
        }
    }
    public void SetValues(string[] names, object[] data, object selected)
    {
        SetValues(names, data);

        for (int i = 0; i < data.Length; i++)
        {
            if (selected.Equals(data[i]))
                Dropdown.value = i;
        }
    }

    protected virtual void ItemSelected(int index)
    {
        OnValueChanged?.Invoke(index, _values[index], _data[index]);
    }

    protected override void MainColorChanged()
    {
        var colors = Dropdown.colors;
        colors.pressedColor = Context.MainColor;
        Dropdown.colors = colors;
    }

    protected override void LocalizationChanged()
    {
        base.LocalizationChanged();
        OnLocaleChanged?.Invoke();
    }
}
