using Lean.Touch;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : SingletonMonoBehavior<InputManager>
{
    private HashSet<int> swiped = new();

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
        if (swiped.Contains(finger.Index))
            swiped.Remove(finger.Index);

        if (Game.Instance.IsPaused) return;

        Note closest = null;
        foreach (var track in Game.Instance.CreatedTracks)
        {
            if (track.IsAnimating || !IsTrackWithin(track, finger.ScreenPosition.x * 0.1f)) continue;
            track.Fingers.Add(finger.Index);
            
            foreach(var note in track.CreatedNotes)
            {
                if (!Game.Instance.State.NoteIsJudged(note.ID)
                    && (closest == null || closest.Model.time > note.Model.time)
                    && !(note.Type == NoteType.Hold && (note as HoldNote).IsBeingHeld))
                    closest = note;
            }
        }

        if (closest != null)
            closest.OnTrackDown(Conductor.Instance.Time);
    }

    private void OnFingerUp(LeanFinger finger)
    {
        if (swiped.Contains(finger.Index))
            swiped.Remove(finger.Index);

        foreach(var track in Game.Instance.CreatedTracks)
        {
            if (track.Fingers.Contains(finger.Index))
                track.Fingers.Remove(finger.Index);
        }
    }

    private void OnFingerUpdate(LeanFinger finger)
    {
        if (Game.Instance.IsPaused) return;

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

        // LeanTouch handles swiping once the finger is up
        // instead of instantly so it has to be called manually
        if (finger.IsActive && !finger.Old && !swiped.Contains(finger.Index) && Mathf.Abs(finger.SwipeScaledDelta.x) > 1f)
            OnFingerSwipe(finger);
    }

    // This one called from OnFingerUpdate
    private void OnFingerSwipe(LeanFinger finger)
    {
        swiped.Add(finger.Index);
        if (Game.Instance.IsPaused) return;

        var note = GetClosestNote(finger.StartScreenPosition.x * 0.1f);
        if (note != null)
            note.OnTrackSwiped(Conductor.Instance.Time, finger.SwipeScaledDelta.x);
    }

    private Note GetClosestNote(float x)
    {
        Note closest = null;

        foreach(var track in Game.Instance.CreatedTracks)
        {
            if (track.IsAnimating || track.CreatedNotes.Count == 0 || !IsTrackWithin(track, x)) continue;
            foreach(var note in track.CreatedNotes)
            {
                if (note.Type == NoteType.Hold && (note as HoldNote).IsBeingHeld) continue;

                if (!Game.Instance.State.NoteIsJudged(note.ID) && (closest == null || closest.Model.time > note.Model.time))
                    closest = note;
            }
        }

        return closest;
    }

    public static bool IsTrackWithin(Track track, float x)
    {
        var pos = track.CurrentMoveValue;
        var width = track.CurrentScaleValue * 13.6f + 2f.ScreenScaledX();
        return x.IsWithin(pos - width * 0.5f, pos + width * 0.5f);
    }
}
