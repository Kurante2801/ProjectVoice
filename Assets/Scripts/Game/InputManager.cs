using Lean.Touch;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : SingletonMonoBehavior<InputManager>
{
    private void OnEnable()
    {
        LeanTouch.OnFingerDown += OnFingerDown;
        LeanTouch.OnFingerUp += OnFingerUp;
        LeanTouch.OnFingerUpdate += OnFingerUpdate;
    }

    private void OnDisable()
    {
        LeanTouch.OnFingerDown -= OnFingerDown;
        LeanTouch.OnFingerUp -= OnFingerUp;
        LeanTouch.OnFingerUpdate -= OnFingerUpdate;
    }

    private void OnFingerDown(LeanFinger finger)
    {
        foreach (var track in Game.Instance.CreatedTracks)
        {
            if (IsTrackWithin(track, finger.StartScreenPosition.x * 0.1f))
                track.Fingers.Add(finger.Index);
        }
    }

    private void OnFingerUp(LeanFinger finger)
    {
        foreach(var track in Game.Instance.CreatedTracks)
        {
            if (track.Fingers.Contains(finger.Index))
                track.Fingers.Remove(finger.Index);
        }
    }

    private void OnFingerUpdate(LeanFinger finger)
    {
        foreach (var track in Game.Instance.CreatedTracks)
        {
            if (finger.IsActive && IsTrackWithin(track, finger.ScreenPosition.x * 0.1f))
            {
                if (!track.Fingers.Contains(finger.Index))
                    track.Fingers.Add(finger.Index);
            }
            else if (track.Fingers.Contains(finger.Index))
                track.Fingers.Remove(finger.Index);
        }
    }

    private Note GetClosestNote(float x)
    {
        Note closest = null;

        foreach(var track in Game.Instance.CreatedTracks)
        {
            if (track.IsAnimating || track.CreatedNotes.Count == 0 | IsTrackWithin(track, x)) continue;
            foreach(var note in track.CreatedNotes)
            {
                if (!Game.Instance.State.NoteIsJudged(note.ID) && (closest == null || closest.Model.time > note.Model.time))
                    closest = note;
            }
        }

        return closest;
    }

    private static bool IsTrackWithin(Track track, float x)
    {
        var pos = track.CurrentMoveValue;
        var width = track.CurrentScaleValue * 13.6f;
        return x.IsWithin(pos - width * 0.5f, pos + width * 0.5f);
        
    }
}
