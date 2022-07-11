using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SettingNoteHoldElement : SettingNoteElement
{
    public Image TopBackground, TopForeground, Sustain;
    [SerializeField] private List<Sprite> sustainSprites = new();
    private string topForegroundKey;
    [SerializeField] private ColorPicker topForegroundPicker;

    public UnityEvent<Color> OnBottomForegroundChanged => OnForegroundChanged;
    public UnityEvent<Color> OnTopForegroundChanged = new();

    public void SetValues(NoteShape shape, Color back, Color bottomFore, Color topFore)
    {
        TopBackground.sprite = backgroundSprites[(int)shape];
        TopForeground.sprite = foregroundSprites[(int)shape];
        Sustain.sprite = sustainSprites[(int)shape];

        TopForeground.color = topFore;
        topForegroundPicker.SetValues(topFore, false, topForegroundKey);
        topForegroundPicker.OnValueChanged.AddListener(value =>
        {
            TopForeground.color = value;
            OnTopForegroundChanged?.Invoke(value);
        });

        SetValues(shape, back, bottomFore);
        backgroundPicker.OnValueChanged.AddListener(color =>
        {
            TopBackground.color = color;
            Sustain.color = color;
        });
    }

    protected override void ItemSelected(int index)
    {
        TopBackground.sprite = backgroundSprites[index];
        TopForeground.sprite = foregroundSprites[index];
        Sustain.sprite = sustainSprites[index];
        base.ItemSelected(index);
    }

    public void SetLocalizationKeys(string name, string description, string backgroundModal, string bottomForegroundModal, string topForegroundModal)
    {
        topForegroundKey = topForegroundModal;
        SetLocalizationKeys(name, description, backgroundModal, bottomForegroundModal);
    }
}
