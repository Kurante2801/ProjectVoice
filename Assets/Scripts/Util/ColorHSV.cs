using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// https://github.com/judah4/HSV-Color-Picker-Unity/blob/master/Assets/HSVPicker/UtilityScripts/HSVUtil.cs

public struct ColorHSV
{
    /// <summary>
    /// The Hue, ranges between 0 and 360
    /// </summary>
    public double h;

    /// <summary>
    /// The saturation, ranges between 0 and 1
    /// </summary>
    public double s;

    /// <summary>
    /// The value (brightness), ranges between 0 and 1
    /// </summary>
    public double v;

    public float normalizedH
    {
        get => (float)h / 360f;
        set => h = (double)value * 360f;
    }

    public ColorHSV(double hue, double saturation, double value)
    {
        h = hue;
        s = saturation;
        v = value;
    }

    public override string ToString()
    {
        return $"{{{h:F2},{s:F2},{v:F2}}}";
    }
}
