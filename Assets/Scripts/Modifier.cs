using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Modifier
{
    Auto, AutoClick, AutoHold, AutoSwipe, AutoSlide
}

public static class ModifierExtensions
{
    public static string GetName(this Modifier modifier)
    {
        return modifier switch
        {
            Modifier.Auto => "Auto",
            Modifier.AutoClick => "Auto Click",
            Modifier.AutoHold => "Auto Hold",
            Modifier.AutoSwipe => "Auto Swipe",
            Modifier.AutoSlide => "Auto Slide",
            _ => ""
        };
    }
}
