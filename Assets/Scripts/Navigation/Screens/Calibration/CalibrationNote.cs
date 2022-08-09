using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CalibrationNote : MonoBehaviour
{
    [SerializeField] private RawImage background, foreground;

    public ChartModel.NoteModel model;
    public CalibrationScreen.Conductor conductor;
    public CalibrationScreen screen;

    private void Awake()
    {
        background.color = PlayerSettings.ClickBackgroundColor.Value;
        foreground.color = PlayerSettings.ClickForegroundColor.Value;
    }

    public void SetData(int time, CalibrationScreen.Conductor conductor, CalibrationScreen screen)
    {
        model = new ChartModel.NoteModel();
        model.time = time;
        this.conductor = conductor;
        this.screen = screen;
        Update();
    }

    private void Update()
    {
        int diff = model.time - conductor.Time;
        var transform = this.transform as RectTransform;
        transform.anchoredPosition = transform.anchoredPosition.WithY(diff.MapRange(0f, Note.ScrollDurations[2], 0f, 800f.ScreenScaledY()));

        if (diff < 0)
            screen.DisposeNote(this);
    }
}
