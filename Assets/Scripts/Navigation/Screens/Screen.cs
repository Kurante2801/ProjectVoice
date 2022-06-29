using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// based off https://github.com/Cytoid/Cytoid/blob/main/Assets/Scripts/Screen/Screen.cs

[RequireComponent(typeof(RectTransform), typeof(Canvas), typeof(CanvasGroup))]
public abstract class Screen : MonoBehaviour
{
    [HideInInspector] public Canvas Canvas;
    [HideInInspector] public CanvasGroup CanvasGroup;
    [HideInInspector] public RectTransform RectTransform;
    [HideInInspector] public GraphicRaycaster GraphicRaycaster;

    private ScreenState state = ScreenState.Destroyed;
    public ScreenState State
    {
        get => state;
        set
        {
            var original = state;
            state = value;

            switch (state)
            {
                case ScreenState.Destroyed:
                    if (original != ScreenState.Destroyed)
                        OnScreenDestroyed();
                    break;
                case ScreenState.Active:
                    if (original == ScreenState.Active) break;
                    else if (original == ScreenState.Destroyed) OnScreenInitialized();
                    OnScreenBecameActive();
                    OnScreenPostActive();
                    break;
                case ScreenState.Inactive:
                    if (original == ScreenState.Destroyed) OnScreenInitialized();
                    else if (original == ScreenState.Active) OnScreenBecameInactive();
                    break;
            }
        }
    }

    // We have to do virtual strings otherwise Unity throws exceptions when using prefabs
    public virtual string GetID() => "Screen";

    protected virtual void Awake()
    {
        Canvas = GetComponent<Canvas>();
        CanvasGroup = GetComponent<CanvasGroup>();
        RectTransform = GetComponent<RectTransform>();
        GraphicRaycaster = GetComponent<GraphicRaycaster>();
    }

    public virtual void OnScreenInitialized()
    {
        // According to https://github.com/Cytoid/Cytoid/blob/1ce07d83628aef0fd5afbc450ecd4fed0600e47b/Assets/Scripts/Screen/Screen.cs#L195
        // we have to rebuild all layout groups 2 times
        var layouts = gameObject.GetComponentsInChildren<LayoutGroup>();
        foreach (var layoutGroup in layouts)
            layoutGroup.transform.RebuildLayout();
        foreach (var layoutGroup in layouts)
            layoutGroup.transform.RebuildLayout();
    }

    public virtual void OnScreenBecameActive()
    {
        SetBlockRaycasts(true);
    }

    public virtual void OnScreenPostActive()
    {
    }

    public virtual void OnScreenDestroyed()
    {
        Context.ScreenManager.CreatedScreens.Remove(this);
    }

    public virtual void OnScreenBecameInactive()
    {
        SetBlockRaycasts(false);
    }

    public virtual void OnScreenTransitionInEnded()
    {
        SetBlockRaycasts(true);
    }

    public virtual void OnScreenTransitionOutBegan()
    {
        SetBlockRaycasts(false);
    }

    public virtual void SetBlockRaycasts(bool block)
    {
        CanvasGroup.blocksRaycasts = block;
        CanvasGroup.interactable = block;
    }
}

public enum ScreenState
{
    Destroyed,
    Active,
    Inactive
}

// In case I ever implement more
public enum ScreenTransition
{
    Fade, None
}