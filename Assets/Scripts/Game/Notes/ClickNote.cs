using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickNote : Note
{
    public static new bool IsAuto => Context.Modifiers.Contains(Modifier.Auto) || Context.Modifiers.Contains(Modifier.AutoClick);
    public override NoteShape GetShape() => PlayerSettings.ClickShape.Value;

    protected override void Start()
    {
        Background.sprite = Game.Instance.ShapesAtlas[(int)GetShape()].GetSprite("click_back");
        Foreground.sprite = Game.Instance.ShapesAtlas[(int)GetShape()].GetSprite("click_fore");

        Background.color = PlayerSettings.ClickBackgroundColor.Value;
        Foreground.color = PlayerSettings.ClickForegroundColor.Value;
    }

    public override void OnTrackDown(int time) => JudgeNote(time);

}
