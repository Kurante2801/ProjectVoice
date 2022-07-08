using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlideNote : Note
{
    protected override void Start()
    {
        Background.sprite = Game.Instance.ShapesAtlas[(int)Shape].GetSprite("slide_back");
        Foreground.sprite = Game.Instance.ShapesAtlas[(int)Shape].GetSprite("slide_fore");

        Background.color = PlayerSettings.SlideBackgroundColor;
        Foreground.color = PlayerSettings.SlideForegroundColor;
    }

    protected override void Update()
    {
        int time = Conductor.Instance.Time;

        float y = Mathf.Max(0f, GetPosition(time, Model));
        transform.localPosition = new Vector3(0f, y, 0f);

        var difference = Model.time - time;
        if ((difference < NoteGradeExtensions.Timings[(int)NoteGrade.Good] && Track.Fingers.Count > 0) || (IsAuto && difference <= 0) || difference < -NoteGradeExtensions.Timings[(int)NoteGrade.Good])
            JudgeNote(time);
                
    }
}
