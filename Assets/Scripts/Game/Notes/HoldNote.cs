using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldNote : Note
{
    public int HoldTime => Model.data;
    public static new bool IsAuto => Context.Modifiers.Contains(Modifer.Auto) || Context.Modifiers.Contains(Modifer.AutoHold);
    public new NoteShape Shape => PlayerSettings.HoldShape;

    [SerializeField] private SpriteRenderer backgroundTop, foregroundTop, sustain;

    public override void SetAlpha(float alpha)
    {
        base.SetAlpha(alpha);
        backgroundTop.color = backgroundTop.color.WithAlpha(alpha);
        foregroundTop.color = foregroundTop.color.WithAlpha(alpha);
        sustain.color = sustain.color.WithAlpha(alpha);
    }

    public override void ScreenSizeChanged(int w, int h)
    {
        base.ScreenSizeChanged(w, h);
        foregroundTop.transform.localScale = backgroundTop.transform.localScale = Background.transform.localScale;
        sustain.transform.localScale = new Vector3(1f.ScreenScaledX(), (HoldTime / Speed).ScreenScaledY(), 1f);
        foregroundTop.transform.localPosition = backgroundTop.transform.localPosition = new Vector3(0f, GetEndPosition(Model), 0f);
    }

    public static float GetEndPosition(ChartModel.NoteModel model) => Context.ScreenHeight * 0.1f * model.data / Speed;

}
