using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;

public static class CommonExtensions
{
    public static string Capitalize(this string str) => char.ToUpper(str[0]) + str[1..];
    
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

    public static Color WithAlpha(this Color color, float alpha)
    {
        return new Color(color.r, color.g, color.b, alpha);
    }
    public static Color32 WithAlpha(this Color32 color, byte alpha)
    {
        return new Color32(color.r, color.g, color.b, alpha);
    }
}
