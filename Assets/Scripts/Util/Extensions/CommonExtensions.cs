using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;

public static class CommonExtensions
{
    public static string Capitalize(this string str) => char.ToUpper(str[0]) + str[1..];
    public static string Get(this string key, string fallback = default) => Context.LocalizationManager.GetLocalized(key, fallback != default ? fallback : key);

    public static void RebuildLayout(this Transform transform)
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
    }

    private static string[] RichTextTags { get; } = new string[]
    {
        "align", "allcaps", "alpha", "b", "color", "cspace", "font", "font-weight", "gradient", "i", "indent", "line-height", "line-indent", "link", "lowercase", "margin", "margin-left", "margin-right", "mark", "mspace", "nobr", "noparse", "page", "pos", "rotate", "s", "size", "smallcaps", "space", "sprite", "style", "sub", "sup", "u", "uppercase", "voffset", "width",
    };

    private static bool CaptureTag(string text, int startIndex, out int endIndex)
    {
        endIndex = -1;

        for(int i = startIndex + 1; i < text.Length; i++)
        {
            if (text[i] == '>')
            {
                endIndex = i;
                return true;
            }
        }

        return false;
    }

    private static HashSet<string> CaptureTags(string text)
    {
        var tags = new HashSet<string>();
        if (string.IsNullOrWhiteSpace(text)) return tags;

        for(int i = 0; i < text.Length; i++)
        {
            if (text[i] != '<') continue;

            if (CaptureTag(text, i, out int endIndex))
            {
                tags.Add(text.Substring(i, endIndex + 1));
                i += endIndex - i;
            }
        }

        return tags;
    }

    /// <summary>
    /// Returns the text with all RichText tags from TextMesh Pro removed
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static string SanitizeTMP(this string text)
    {
        foreach(string captured in CaptureTags(text))
            foreach (string tag in RichTextTags)
                if (captured.StartsWith("<" + tag) || captured.StartsWith("</" + tag))
                    text = text.Replace(captured, string.Empty);

        return text;
    }

    public static float MapRange(this float value, float from1, float from2, float to1, float to2)
    {
        return (value - from1) * (to2 - to1) / (from2 - from1) + to1;
    }

    public static float MapRange(this int value, float from1, float from2, float to1, float to2)
    {
        return (value - from1) * (to2 - to1) / (from2 - from1) + to1;
    }

    public static bool IsBetween(this int value, int min, int max)
    {
        return min <= value && value <= max;
    }
}