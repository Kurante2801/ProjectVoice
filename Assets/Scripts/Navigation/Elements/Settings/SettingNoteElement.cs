using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SettingNoteElement : SettingDropdownElement
{
    public RawImage Background, Foreground;
    [SerializeField] internal List<Texture2D> backgroundTextures = new(), foregroundTextures = new();
    [SerializeField] internal ColorPicker backgroundPicker, foregroundPicker;
    private string backgroundKey, foregroundKey;

    public UnityEvent<Color> OnBackgroundChanged = new(), OnForegroundChanged = new();
    public UnityEvent<NoteShape> OnShapeChanged = new();
    
    public void SetValues(NoteShape selected, Color back, Color fore, Color back_fallback, Color fore_fallback)
    {
        Background.texture = backgroundTextures[(int)selected];
        Background.color = back;
        backgroundPicker.SetValues(back, false, backgroundKey, back_fallback);
        backgroundPicker.OnValueChanged.AddListener(value =>
        {
            Background.color = value;
            OnBackgroundChanged?.Invoke(value);
        });

        Foreground.texture = foregroundTextures[(int)selected];
        Foreground.color = fore;
        foregroundPicker.SetValues(fore, false, foregroundKey, fore_fallback);
        foregroundPicker.OnValueChanged.AddListener(value =>
        {
            Foreground.color = value;
            OnForegroundChanged?.Invoke(value);
        });

        var shapes = Enum.GetValues(typeof(NoteShape)).Cast<object>().ToArray();
        var names = shapes.Cast<NoteShape>().Select(shape => shape.GetLocalized()).ToArray();

        SetValues(names, shapes, selected);
    }

    protected override void ItemSelected(int index)
    {
        Background.texture = backgroundTextures[index];
        Foreground.texture = foregroundTextures[index];
        OnShapeChanged?.Invoke((NoteShape)index);
    }

    public void SetLocalizationKeys(string name, string description, string backgroundModal, string foregroundModal)
    {
        SetLocalizationKeys(name, description);
        backgroundKey = backgroundModal;
        foregroundKey = foregroundModal;
    }

    protected override void LocalizationChanged()
    {
        base.LocalizationChanged();
        SetNames(Enum.GetValues(typeof(NoteShape)).Cast<NoteShape>().Select(shape => shape.GetLocalized()).ToArray());
    }
}
