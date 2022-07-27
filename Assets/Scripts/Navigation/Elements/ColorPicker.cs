using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;

public class ColorPicker : MonoBehaviour
{
    public Image ColorImage;
    public Button ColorButton;
    public TMP_InputField InputField;
    public bool AllowAlpha = false;
    public string ModalText = "";

    public Color? Fallback = null;
    private Color _color = Color.white;
    public Color Color
    {
        get => _color;
        set
        {
            _color = value;
            UpdateValues();
        }
    }

    public UnityEvent<Color> OnValueChanged = new();

    private void Awake()
    {
        InputField.onEndEdit.AddListener(InputFieldChanged);
    }

    public void UpdateValues()
    {
        InputField.SetTextWithoutNotify(_color.ToHex(true, AllowAlpha));
        ColorImage.color = _color.WithAlpha(AllowAlpha ? _color.a : 1f);
    }

    private void InputFieldChanged(string value)
    {
        var desired = value.ToColor(AllowAlpha);
        InputField.SetTextWithoutNotify(desired.ToHex(true, AllowAlpha));
        
        if (desired == Color) return;
        Color = desired;

        ColorImage.color = Color.WithAlpha(AllowAlpha ? Color.a : 1f);
        OnValueChanged?.Invoke(Color);
    }

    public void SetValues(Color color, bool allowAlpha, string modalText, Color? fallback = null)
    {
        AllowAlpha = allowAlpha;
        Color = color;
        ModalText = modalText;
        Fallback = fallback;
    }

    public void OpenModal()
    {
        var rootCanvas = GetComponentInParent<Canvas>().rootCanvas;
        var modal = ColorPickerModal.CreateModal(rootCanvas);
        modal.SetValues(Color, ModalText, AllowAlpha, Fallback);
        modal.OnEditEnd.AddListener(value =>
        {
            Color = value;
            UpdateValues();
            OnValueChanged?.Invoke(value);
        });
    }
}
