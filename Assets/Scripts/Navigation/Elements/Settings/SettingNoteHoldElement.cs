using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SettingNoteHoldElement : SettingNoteElement
{
    public RawImage TopBackground, TopForeground, Sustain;
    [SerializeField] private List<Texture2D> sustainTextures = new();
    private string topForegroundKey;
    [SerializeField] private ColorPicker topForegroundPicker;

    public UnityEvent<Color> OnBottomForegroundChanged => OnForegroundChanged;
    public UnityEvent<Color> OnTopForegroundChanged = new();

    public void SetValues(NoteShape shape, Color back, Color bottomFore, Color topFore)
    {
        TopBackground.texture = backgroundTextures[(int)shape];
        TopForeground.texture = foregroundTextures[(int)shape];
        Sustain.texture = sustainTextures[(int)shape];

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
        TopBackground.texture = backgroundTextures[index];
        TopForeground.texture = foregroundTextures[index];
        Sustain.texture = sustainTextures[index];
        base.ItemSelected(index);
    }

    public void SetLocalizationKeys(string name, string description, string backgroundModal, string bottomForegroundModal, string topForegroundModal)
    {
        topForegroundKey = topForegroundModal;
        SetLocalizationKeys(name, description, backgroundModal, bottomForegroundModal);
    }
}
