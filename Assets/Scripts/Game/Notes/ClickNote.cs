using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickNote : Note
{
    public static new bool IsAuto => Context.Modifiers.Contains(Modifer.Auto) || Context.Modifiers.Contains(Modifer.AutoClick);
    public static new NoteShape Shape => PlayerSettings.ClickShape;

    protected override void Start()
    {
        Background.sprite = Game.Instance.ShapesAtlas[(int)Shape].GetSprite("click_back");
        Foreground.sprite = Game.Instance.ShapesAtlas[(int)Shape].GetSprite("click_fore");

        Background.color = PlayerSettings.ClickBackgroundColor;
        Foreground.color = PlayerSettings.ClickForegroundColor;
    }

    public override void OnTrackDown(int time) => JudgeNote(time);

}
