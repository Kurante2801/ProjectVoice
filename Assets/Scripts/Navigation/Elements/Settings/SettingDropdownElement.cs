using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class SettingDropdownElement : SettingElement
{
    public TMP_Dropdown Dropdown;
    public UnityEvent<int, string> OnValueChanged = new();
    public UnityEvent OnLocaleChanged = new();

    private string[] values;

    private void Awake()
    {
        SettingType = SettingType.Dropdown;
        Dropdown.onValueChanged.AddListener(ItemSelected);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        Context.OnMainColorChanged.AddListener(MainColorChanged);
        MainColorChanged();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        Context.OnMainColorChanged.RemoveListener(MainColorChanged);
    }

    public void SetValues(string[] options, string selected = null)
    {
        values = options;
        Dropdown.options.Clear();
        for(int i = 0; i < options.Length; i++)
        {
            string option = options[i];
            Dropdown.options.Add(new TMP_Dropdown.OptionData(option));

            if (option == selected)
                Dropdown.value = i;
        }
    }

    private void ItemSelected(int index)
    {
        OnValueChanged?.Invoke(index, values[index]);
    }

    private void MainColorChanged()
    {
        var colors = Dropdown.colors;
        colors.pressedColor = Context.MainColor;
        Dropdown.colors = colors;
    }
}
