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
        InputField.SetTextWithoutNotify(_color.ToHEX(true, AllowAlpha));
        ColorImage.color = _color.WithAlpha(AllowAlpha ? _color.a : 1f);
    }

    private void InputFieldChanged(string value)
    {
        var desired = value.ToColor(AllowAlpha);
        InputField.SetTextWithoutNotify(desired.ToHEX(true, AllowAlpha));

        if (desired == Color) return;
        Color = desired;

        ColorImage.color = Color.WithAlpha(AllowAlpha ? Color.a : 1f);
        OnValueChanged?.Invoke(Color);
    }

    public void SetValues(Color color, bool allowAlpha)
    {
        AllowAlpha = allowAlpha;
        Color = color;
    }

    // From TextMesh Pro's Dropdown code
    /*private GameObject CreateBlocker(Canvas rootCanvas)
    {
        var blocker = new GameObject("Blocker");
        var blockerRect = blocker.AddComponent<RectTransform>();
        blockerRect.SetParent(rootCanvas.transform, false);
        blockerRect.anchorMin = Vector2.zero;
        blockerRect.anchorMax = Vector2.one;
        blockerRect.sizeDelta = Vector2.zero;

        var blockerCanvas = blocker.AddComponent<Canvas>();
        blockerCanvas.overrideSorting = true;
        

    }*/
}
