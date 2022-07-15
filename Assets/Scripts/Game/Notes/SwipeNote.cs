using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwipeNote : Note
{
    public int SwipeDelta => Model.data;
    public static new bool IsAuto => Context.Modifiers.Contains(Modifier.Auto) || Context.Modifiers.Contains(Modifier.AutoSwipe);
    public override NoteShape GetShape() => SwipeDelta < 0 ? PlayerSettings.SwipeLeftShape : PlayerSettings.SwipeRightShape;

    protected override void Start() { }

    public override void Initialize()
    {
        bool left = SwipeDelta < 0;
        Background.sprite = Game.Instance.ShapesAtlas[(int)GetShape()].GetSprite("swipe_back");
        Foreground.sprite = Game.Instance.ShapesAtlas[(int)GetShape()].GetSprite("swipe_fore");

        Background.color = left ? PlayerSettings.SwipeLeftBackgroundColor : PlayerSettings.SwipeRightBackgroundColor;
        Foreground.color = left ? PlayerSettings.SwipeLeftForegroundColor : PlayerSettings.SwipeRightForegroundColor;

        transform.localRotation = Quaternion.Euler(0f, 0f, 90f + 90f * SwipeDelta);

        base.Initialize();
    }

    public override void OnTrackSwiped(int time, float delta)
    {
        if ((delta < 0 && SwipeDelta < 0) || (delta > 0 && SwipeDelta > 0))
            JudgeNote(time);
    }
}
