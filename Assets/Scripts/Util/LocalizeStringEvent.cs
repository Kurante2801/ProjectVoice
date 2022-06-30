using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class LocalizeStringEvent : MonoBehaviour
{
    public string Key, FallbackText;
    public UnityEvent<string> OnLocalizationChanged;

    private void OnEnable()
    {
        Context.OnLocalizationChanged.AddListener(LocalizationChanged);
        LocalizationChanged();
    }
    private void OnDisable()
    {
        Context.OnLocalizationChanged.RemoveListener(LocalizationChanged);
    }


    private void LocalizationChanged()
    {
        OnLocalizationChanged.Invoke(Key.Get(FallbackText));
    }
}
