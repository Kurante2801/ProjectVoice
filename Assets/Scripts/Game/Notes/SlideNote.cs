using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlideNote : Note
{
    public override NoteShape GetShape() => PlayerSettings.SlideShape;

    protected override void Start()
    {
        Background.sprite = Game.Instance.ShapesAtlas[(int)GetShape()].GetSprite("slide_back");
        Foreground.sprite = Game.Instance.ShapesAtlas[(int)GetShape()].GetSprite("slide_fore");

        Background.color = PlayerSettings.SlideBackgroundColor;
        Foreground.color = PlayerSettings.SlideForegroundColor;
    }

    protected override void Update()
    {
        int time = Conductor.Instance.Time;
        int difference = Model.time - time;
        int missThreshold = -NoteGradeExtensions.Timings[(int)NoteGrade.Good];

        // Miss animation
        if (IsCollected)
        {
            if (difference > -1000)
            {
                float sinceMiss = (difference - missThreshold) / 1000f;

                transform.localPosition = new Vector3(0f, sinceMiss * Context.ScreenHeight * 0.025f, 0f);
                SetAlpha(0.5f + sinceMiss * 0.5f);
            }
            else
                Track.DisposeNote(this);

            return;
        }

        float y = Mathf.Max(0f, GetPosition(time, Model));
        transform.localPosition = new Vector3(0f, y, 0f);

        if ((difference < NoteGradeExtensions.Timings[(int)NoteGrade.Good] && Track.Fingers.Count > 0) || (IsAuto && difference <= 0) || difference < -NoteGradeExtensions.Timings[(int)NoteGrade.Good])
            JudgeNote(time);
                
    }

    public override void JudgeNote(int time)
    {
        var grade = JudgeGrade(time, Model);
        if(grade != NoteGrade.None)
        {
            if (grade != NoteGrade.Miss)
                grade = NoteGrade.Perfect;

            // Activate tracks behind this note's track (including this note's track)
            if (IsAuto)
            {
                foreach (var track in Game.Instance.CreatedTracks)
                {
                    if (InputManager.IsTrackWithin(track, Track.transform.position.x))
                        track.ActiveTime = time;
                }
            }

            Game.Instance.State.Judge(this, grade, Model.time - time);
            Game.Instance.OnNoteJudged?.Invoke(Game.Instance, Model.id);
            Collect(grade);
        }
    }

}
