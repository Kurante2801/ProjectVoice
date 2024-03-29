using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;
using SimpleFileBrowser;
using System.IO;

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

    public static Vector3 WithX(this Vector3 vector, float x) => new(x, vector.y, vector.z);
    public static Vector3 WithY(this Vector3 vector, float y) => new(vector.x, y, vector.z);
    public static Vector2 WithX(this Vector2 vector, float x) => new(x, vector.y);
    public static Vector2 WithY(this Vector2 vector, float y) => new(vector.x, y);

    /// <summary>
    /// Converts width and height from Screen size to World space
    /// </summary>
    /// <param name="renderer"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns>World size</returns>
    public static Vector2 ScreenSizeToWorld(this SpriteRenderer renderer, float width, float height)
    {
        float camH = Context.Camera.orthographicSize * 2;
        var cameraSize = new Vector2(Context.Camera.aspect * camH, camH);

        var size = renderer.sprite.bounds.size;
        return new Vector2(MathExtensions.MapRange(width / size.x, 0f, Context.ScreenWidth, 0f, cameraSize.x), MathExtensions.MapRange(height / size.y, 0f, Context.ScreenHeight, 0f, cameraSize.y));
    }
}
