using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SettingNoteElement : SettingDropdownElement
{
    public Image Background, Foreground;
    [SerializeField] internal List<Sprite> backgroundSprites = new(), foregroundSprites = new();
    [SerializeField] internal ColorPicker backgroundPicker, foregroundPicker;
    private string backgroundKey, foregroundKey;

    public UnityEvent<Color> OnBackgroundChanged = new(), OnForegroundChanged = new();
    public UnityEvent<NoteShape> OnShapeChanged = new();

    public void SetValues(NoteShape shape, Color back, Color fore)
    {
        Background.sprite = backgroundSprites[(int)shape];
        Background.color = back;
        backgroundPicker.SetValues(back, false, backgroundKey);
        backgroundPicker.OnValueChanged.AddListener(value =>
        {
            Background.color = value;
            OnBackgroundChanged?.Invoke(value);
        });

        Foreground.sprite = foregroundSprites[(int)shape];
        Foreground.color = fore;
        foregroundPicker.SetValues(fore, false, foregroundKey);
        foregroundPicker.OnValueChanged.AddListener(value =>
        {
            Foreground.color = value;
            OnForegroundChanged?.Invoke(value);
        });
        
        SetValues(Enum.GetNames(typeof(NoteShape)), Enum.GetValues(typeof(NoteShape)).Cast<object>().ToArray(), shape);
    }

    protected override void ItemSelected(int index)
    {
        Background.sprite = backgroundSprites[index];
        Foreground.sprite = foregroundSprites[index];
        OnShapeChanged?.Invoke((NoteShape)index);
    }

    public void SetLocalizationKeys(string name, string description, string backgroundModal, string foregroundModal)
    {
        SetLocalizationKeys(name, description);
        backgroundKey = backgroundModal;
        foregroundKey = foregroundModal;
    }
}
