using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldTick : MonoBehaviour
{
    [SerializeField] private SpriteRenderer background, foreground;
    private HoldNote note;
    private float x;
    private ChartModel.NoteModel model;

    private void OnEnable()
    {
        Game.Instance.OnScreenSizeChanged.AddListener(ScreenSizeChanged);
    }

    private void OnDisable()
    {
        if (Game.Instance != null)
            Game.Instance.OnScreenSizeChanged.RemoveListener(ScreenSizeChanged);
    }

    public void Initialize(HoldNote note, float x, int time)
    {
        this.note = note;
        this.x = x;

        model = new ChartModel.NoteModel();
        model.time = time;

        background.color = PlayerSettings.HoldTickBackgroundColor.Value;
        foreground.color = PlayerSettings.HoldTickForegroundColor.Value;

        ScreenSizeChanged(Context.ScreenWidth, Context.ScreenHeight);
        Update();
    }

    private void Update()
    {
        if (Game.Instance.IsPaused) return;
        int time = Conductor.Instance.Time;
        int difference = model.time - time;

        if (difference < 0)
        {
            note.DisposeTick(this);
            return;
        }

        transform.position = new Vector3(Track.ScreenMargin + Track.MarginPosition * x, Note.GetPosition(time, model) + 12f.ScreenScaledY(), 0f);
    }

    private void ScreenSizeChanged(int w, int h)
    {
        transform.localScale = Vector2.one * 1f.ScreenScaledX();
    }
}
