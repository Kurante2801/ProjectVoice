using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Globalization;
using UnityEngine.Events;
using DG.Tweening;

public class ColorPickerModal : MonoBehaviour
{
    [Tooltip("Objects to hide when alpha is not allowed")]
    public List<GameObject> AlphaObjects = new();
    public Image CancelColorImage, AcceptColorImage;

    public RawImage SVGraphic, HueGraphic, AlphaGraphic;
    public TMP_InputField RedInput, GreenInput, BlueInput, AlphaInput, HexInput;
    public Slider HueSlider, AlphaSlider;
    public Slider2D SaturationValueSlider;
    public Image TextBar;
    public TMP_Text PropertyText;

    public CanvasGroup CanvasGroup;

    public bool AllowAlpha = true;

    private float _hue = 0f, _saturation = 0f, _value = 0f;
    private byte _alpha = 255;
    public byte Alpha { get => AllowAlpha ? _alpha : (byte)255; set => _alpha = value; }
    public float Hue { get => _hue; set => _hue = Mathf.Clamp01(value); }
    public float Saturation { get => _saturation; set => _saturation = Mathf.Clamp01(value); }

    public float Value { get => _value; set => _value = Mathf.Clamp01(value); }

    public Color32 RGBA => ColorExtensions.HSVToRGB(Hue, Saturation, Value).WithAlpha(Alpha);

    private Color32 previous;

    public UnityEvent<Color32> OnEditEnd = new();

    public GameObject Blocker;

    public static ColorPickerModal CreateModal(Canvas rootCanvas)
    {
        var prefab = Context.Instance.ColorPickerModalPrefab;
        var blocker = CreateBlocker(rootCanvas);
        var gameObject = Instantiate(prefab, blocker.transform);

        var modal = gameObject.GetComponent<ColorPickerModal>();
        modal.Blocker = blocker;
        modal.CanvasGroup.alpha = 0f;
        return modal;
    }

    // From TMP Dropdown
    private static GameObject CreateBlocker(Canvas rootCanvas)
    {
        var blocker = new GameObject("Blocker");
        var rect = blocker.AddComponent<RectTransform>();
        // Make rect fill screen
        rect.SetParent(rootCanvas.transform, false);
        rect.anchorMin = rect.sizeDelta = Vector2.zero;
        rect.anchorMax = Vector2.one;

        var canvas = blocker.AddComponent<Canvas>();
        canvas.sortingOrder = SortingLayer.GetLayerValueFromName("Modals");
        blocker.AddComponent<GraphicRaycaster>();

        var image = blocker.AddComponent<Image>();
        image.color = Color.black.WithAlpha(0.5f);

        return blocker;
    }

    private void Awake()
    {
        RedInput.onEndEdit.AddListener(value => RGBAInputChanged(value, 0));
        GreenInput.onEndEdit.AddListener(value => RGBAInputChanged(value, 1));
        BlueInput.onEndEdit.AddListener(value => RGBAInputChanged(value, 2));
        AlphaInput.onEndEdit.AddListener(value => RGBAInputChanged(value, 3));

        HexInput.onEndEdit.AddListener(value =>
        {
            var color = value.ToColor(AllowAlpha);
            Color.RGBToHSV(color, out _hue, out _saturation, out _value);
            Alpha = ((Color32)color).a;
            ValueChanged();
        });

        HueSlider.minValue = 0f;
        HueSlider.maxValue = 1f;
        HueSlider.wholeNumbers = false;
        HueSlider.onValueChanged.AddListener(value =>
        {
            Hue = value;
            ValueChanged(updateHue: false);
        });

        SaturationValueSlider.Min = new Vector2(1f, 0f);
        SaturationValueSlider.Max = new Vector2(0f, 1f);
        SaturationValueSlider.OnValueChanged.AddListener(value =>
        {
            Saturation = value.x;
            Value = value.y;
            ValueChanged(updateSV: false);
        });

        AlphaSlider.minValue = 0f;
        AlphaSlider.maxValue = 255f;
        AlphaSlider.wholeNumbers = true;
        AlphaSlider.onValueChanged.AddListener(value =>
        {
            Alpha = (byte)value;
            ValueChanged();
        });
    }

    private void RGBAInputChanged(string value, int index)
    {
        var parsed = (byte)(float.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var result) ? result : 0);

        if (index == 3)
            Alpha = parsed;
        else
        {
            var color = RGBA;
            color[index] = parsed;
            Color.RGBToHSV(color, out _hue, out _saturation, out _value);
        }

        ValueChanged();
    }

    public void ValueChanged(bool updateHue = true, bool updateSV = true)
    {
        if (updateHue)
            HueSlider.SetValueWithoutNotify(Hue);
        if (updateSV)
        {
            SaturationValueSlider.SetValueNormalized(new Vector2(1f - Saturation, Value));
            SVGraphic.material.SetFloat("_Hue", Hue);
        }

        var color = RGBA;
        if (AllowAlpha)
        {
            AlphaSlider.value = Alpha;
            AlphaInput.SetTextWithoutNotify(Alpha.ToString());
            AlphaGraphic.color = color.WithAlpha(255);
        }

        RedInput.SetTextWithoutNotify(color.r.ToString());
        GreenInput.SetTextWithoutNotify(color.g.ToString());
        BlueInput.SetTextWithoutNotify(color.b.ToString());
        HexInput.SetTextWithoutNotify(color.ToHex(true, AllowAlpha));

        AcceptColorImage.color = color;
    }

    public void SetValues(Color32 value, string text, bool allowAlpha)
    {
        if (string.IsNullOrWhiteSpace(text))
            TextBar.gameObject.SetActive(false);
        else
            PropertyText.text = text;

        Color.RGBToHSV(value, out _hue, out _saturation, out _value);
        Alpha = value.a;
        AllowAlpha = allowAlpha;
        ValueChanged();

        previous = value;
        CancelColorImage.color = value.WithAlpha(allowAlpha ? value.a : (byte)255);

        foreach (var gameObject in AlphaObjects)
            gameObject.SetActive(allowAlpha);
        CanvasGroup.DOKill();
        CanvasGroup.DOFade(1f, 0.25f);
    }

    public void CancelButton()
    {
        if (Blocker != null)
            Destroy(Blocker);
        else
            Destroy(gameObject);
    }

    public void ResetButton()
    {
        SetValues(previous, PropertyText.text, AllowAlpha);
    }

    public void AcceptButton()
    {
        var color = RGBA;
        CancelButton();
        OnEditEnd?.Invoke(color);
    }

}