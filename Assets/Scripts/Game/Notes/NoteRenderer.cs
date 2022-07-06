using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class NoteRenderer : MonoBehaviour
{
    public static int SpeedIndex = 6;
    // https://github.com/AndrewFM/VoezEditor/blob/master/Assets/Scripts/Note.cs#L18
    public static readonly float[] ScrollDurations =
    {
        1.5f,   // 1x
        1.3f,   // 2x
        1.1f,   // 3x
        0.9f,   // 4x
        0.8f,   // 5x
        0.7f,   // 6x
        0.55f,  // 7x
        0.425f, // 8x
        0.3f,   // 9x
        0.2f,   // 10x
    };

    public static float Speed => ScrollDurations[SpeedIndex];

    public static Color DefaultForegroundColor = new Color32(220, 75, 75, 255);
    public static Color DefaultBackgroundColor = new Color(0, 0, 0, 255);

    public ChartModel.NoteModel Model;
    public int ID => Model.id;
    public NoteType Type => (NoteType)Model.type;

    public bool IsCollected = false;
    public SortingGroup SortingGroup;

    public SpriteRenderer Background, Foreground;

    private void OnEnable()
    {
        Game.Instance.OnScreenSizeChanged.AddListener(ScreenSizeChanged);
    }

    private void OnDisable()
    {
        if (Game.Instance != null)
            Game.Instance.OnScreenSizeChanged.RemoveListener(ScreenSizeChanged);
    }

    public virtual void Initialize()
    {
        SetAlpha(1f);
        IsCollected = false;
        SortingGroup.sortingOrder = -Model.time;
        ScreenSizeChanged(Context.ScreenWidth, Context.ScreenHeight);
    }

    public virtual void SetAlpha(float alpha)
    {
        Background.color = Background.color.WithAlpha(alpha);
        Foreground.color = Foreground.color.WithAlpha(alpha);
    }

    private void ScreenSizeChanged(int w, int h)
    {
        
    }

}
