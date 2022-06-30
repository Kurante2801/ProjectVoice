using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SettingElement : MonoBehaviour
{
    public SettingType SettingType = SettingType.None;
    public TMP_Text Name, Description;
    public string NameLocalizationKey, DescriptionLocalizationKey;

    protected virtual void OnEnable()
    {
        Context.OnLocalizationChanged.AddListener(LocalizationChanged);
        LocalizationChanged();
    }

    protected virtual void OnDisable()
    {
        Context.OnLocalizationChanged.RemoveListener(LocalizationChanged);
    }

    protected virtual void LocalizationChanged()
    {
        Name.text = NameLocalizationKey.Get();
        Description.text = DescriptionLocalizationKey.Get();
    }

    public virtual void SetLocalizationKeys(string name, string description)
    {
        NameLocalizationKey = name;
        DescriptionLocalizationKey = description;
        LocalizationChanged();
    }
}
