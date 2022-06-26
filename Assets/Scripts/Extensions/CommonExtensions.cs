using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class CommonExtensions
{
    public static string Capitalize(this string str) => char.ToUpper(str[0]) + str[1..];
    
    public static void RebuildLayout(this Transform transform)
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
    }

}
