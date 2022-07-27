using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ModifierToggle : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text descripion, buttonText;
    [SerializeField] private string descriptionKey;
    public Modifier Modifier;
    private bool active = false;

    private void OnEnable()
    {
        Context.OnLocalizationChanged.AddListener(LocalizationChanged);
        Context.OnModifiersChanged.AddListener(ModifiersChanged);
        LocalizationChanged();

        bool active = Context.Modifiers.Contains(Modifier);
        button.image.color = button.image.color.WithAlpha(active ? 1f : 0.5f);
    }

    private void OnDisable()
    {
        Context.OnLocalizationChanged.RemoveListener(LocalizationChanged);
        Context.OnModifiersChanged.RemoveListener(ModifiersChanged);
    }

    private void LocalizationChanged()
    {
        descripion.text = descriptionKey.Get();
    }

    private void ModifiersChanged()
    {
        active = Context.Modifiers.Contains(Modifier); 
        button.image.DOFade(active ? 1f : 0.5f, 0.25f);
    }

    public void DoClick()
    {
        active = !active;
        if (active && !Context.Modifiers.Contains(Modifier))
        {
            Context.Modifiers.Add(Modifier);
            Context.OnModifiersChanged?.Invoke();
        }
        if (!active && Context.Modifiers.Contains(Modifier))
        {
            Context.Modifiers.Remove(Modifier);
            Context.OnModifiersChanged?.Invoke();
        }
    }
}
