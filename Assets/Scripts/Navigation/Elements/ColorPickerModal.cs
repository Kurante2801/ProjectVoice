using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Globalization;
using UnityEngine.Events;

// TODO: Change to use ColorHSV instead of RGB! ! !
public class ColorPickerModal : MonoBehaviour
{
    public RectTransform AlphaInputContainer;
    public RawImage ColorBox, HSVBar, AlphaBar;
    public TMP_InputField RedInput, GreenInput, BlueInput, AlphaInput, HEXInput;
    public Slider HueSlider, AlphaSlider;
    public Slider2D SaturationValueSlider;

    private Color32 previous = Color.white;

    private Color32 _value = Color.white;
    public Color32 Value = Color.white;
    public bool AlphaAllowed = true;

    private float _hue;

    public UnityEvent<Color> OnFinishedEditing = new();

    private void Awake()
    {
        RedInput.onEndEdit.AddListener(value => RGBAInputchanged(value, 0));
        GreenInput.onEndEdit.AddListener(value => RGBAInputchanged(value, 1));
        BlueInput.onEndEdit.AddListener(value => RGBAInputchanged(value, 2));
        AlphaInput.onEndEdit.AddListener(value => RGBAInputchanged(value, 3));

        HEXInput.onEndEdit.AddListener(value =>
        {
            Value = value.ToColor(AlphaAllowed);
            ValueChanged();
        });
        AlphaSlider.onValueChanged.AddListener(value =>
        {
            if (!AlphaAllowed) return;
            Value.a = (byte)value;
            ValueChanged();
        });
        HueSlider.onValueChanged.AddListener(value =>
        {
            Color.RGBToHSV(Value, out float h, out float s, out float v);
            Color32 color = Color.HSVToRGB(value, s, v);
            Value = color.WithAlpha(Value.a);
            _hue = value;
            ValueChanged(false, true);
            
        });
        SaturationValueSlider.OnValueChanged.AddListener(value =>
        {
            Color.RGBToHSV(Value, out float h, out float s, out float v);
            s = value.y;
            v = value.x;
            Color32 color = Color.HSVToRGB(h, s, v);
            Value = color.WithAlpha(Value.a);
            ValueChanged(false, false);
        });

        AlphaInputContainer.gameObject.SetActive(AlphaAllowed);
        AlphaSlider.gameObject.SetActive(AlphaAllowed);
    }

    private void OnEnable()
    {
        RegenerateSVTexture();
        RegenerateHTexture();
        RegenerateAlphaTexture();
    }

    private void OnDisable()
    {
        if (AlphaBar.texture != null)
            DestroyImmediate(AlphaBar.texture);
        if (HSVBar.texture != null)
            DestroyImmediate(AlphaBar.texture);
        if (ColorBox.texture != null)
            DestroyImmediate(AlphaBar.texture);
    }

    private void RegenerateAlphaTexture()
    {
        if (!AlphaAllowed) return;
        if (AlphaBar.texture != null)
            DestroyImmediate(AlphaBar.texture);

        var texture = new Texture2D(256, 1);
        texture.wrapMode = TextureWrapMode.Clamp;

        var pixels = new Color32[256];
        for (int i = 0; i < 256; i++)
            pixels[i] = new Color32(255, 255, 255, (byte)i);
        texture.SetPixels32(pixels);
        texture.Apply();

        AlphaBar.texture = texture;
    }

    private void RegenerateHTexture()
    {
        if (HSVBar.texture != null)
            DestroyImmediate(HSVBar.texture);

        var texture = new Texture2D(1, 360);
        texture.wrapMode = TextureWrapMode.Clamp;

        var pixels = new Color[360];
        for (int i = 0; i < 360; i++)
            pixels[i] = Color.HSVToRGB(i / 360f, 1, 1);
        texture.SetPixels(pixels);
        texture.Apply();

        HSVBar.texture = texture;
    }


    // Almost everything was written by GitHub Copilot, I literally only had to make it use Value's Hue with the Color.RGBToHSV function
    private void RegenerateSVTexture()
    {
        if (ColorBox.texture != null)
            DestroyImmediate(ColorBox.texture);

        //if(hue == default)
            //Color.RGBToHSV(Value, out hue, out float _, out float _);
        
        var texture = new Texture2D(256, 256);
        texture.wrapMode = TextureWrapMode.Clamp;

        var pixels = new Color32[256 * 256];
        for (int i = 0; i < 256; i++)
            for (int j = 0; j < 256; j++)
                pixels[i * 256 + j] = Color.HSVToRGB(_hue, i / 255f, j / 255f);
        texture.SetPixels32(pixels);
        texture.Apply();

        ColorBox.texture = texture;
    }

    public void ValueChanged(bool updateH = true, bool updateSV = true)
    {
        /*if (Value.r == _value.r && Value.g == _value.g && Value.b == _value.b && Value.a == _value.a)
            return;*/

        if (AlphaAllowed)
            AlphaSlider.SetValueWithoutNotify(Value.a);

        Color.RGBToHSV(Value, out float h, out float s, out float v);

        if (updateH)
        {
            HueSlider.SetValueWithoutNotify(h);
            _hue = h;
        }
        if(updateSV)
        {
            SaturationValueSlider.SetValueNormalized(new Vector2(v, s));
            RegenerateSVTexture();
        }

        RedInput.SetTextWithoutNotify(Value.r.ToString());
        GreenInput.SetTextWithoutNotify(Value.g.ToString());
        BlueInput.SetTextWithoutNotify(Value.b.ToString());
        if (AlphaAllowed)
            AlphaInput.SetTextWithoutNotify(Value.a.ToString());
        HEXInput.SetTextWithoutNotify(Value.ToHEX(true, AlphaAllowed));

        AlphaBar.color = Value.WithAlpha(255);
    }
    
    private void RGBAInputchanged(string value, int index)
    {
        if (index == 3 && !AlphaAllowed) return;

        Value[index] = (byte)(float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float result) ? result : 0f);
        ValueChanged();
    }

    public void SetValues(Color32 color, bool alphaAllowed = false)
    {
        Value = color;
        AlphaAllowed = alphaAllowed;
        ValueChanged();
    }
}
