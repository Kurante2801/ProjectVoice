using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class Slider2D : Selectable, IDragHandler, IInitializePotentialDragHandler
{
    public RectTransform HandleContainer, HandleRect;
    public bool WholeNumbers = false;

    private Vector2 _normalizedValue = Vector2.one;
    public Vector2 NormalizedValue
    {
        get => _normalizedValue;
        set
        {
            _normalizedValue.x = Mathf.Clamp01(value.x);
            _normalizedValue.y = Mathf.Clamp01(value.y);
        }
    }

    public Vector2 Min = Vector2.zero, Max = Vector2.one;

    public UnityEvent<Vector2> OnValueChanged = new();

    private Vector2 m_Offset = Vector2.zero;
    private DrivenRectTransformTracker m_Tracker;

    private bool CanDrag(PointerEventData eventData) => IsActive() && IsInteractable() && eventData.button == PointerEventData.InputButton.Left;

    private void UpdateDrag(PointerEventData eventData)
    {
        if (HandleContainer == null || HandleContainer.rect.size.x <= 0f || HandleContainer.rect.size.y <= 0f) return;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(HandleContainer, eventData.position, eventData.pressEventCamera, out Vector2 localCursor)) return;
        localCursor -= HandleContainer.rect.position;

        var val = localCursor - m_Offset;
        NormalizedValue = val / HandleContainer.rect.size;
        ValueChanged();
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (!CanDrag(eventData)) return;
        base.OnPointerDown(eventData);

        m_Offset = Vector2.zero;
        if (HandleContainer != null && RectTransformUtility.RectangleContainsScreenPoint(HandleRect, eventData.position, eventData.enterEventCamera))
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(HandleRect, eventData.position, eventData.pressEventCamera, out var localMousePos))
                m_Offset = localMousePos;
            m_Offset.y = -m_Offset.y;
        }
        else
            UpdateDrag(eventData);
    }

    public virtual void OnDrag(PointerEventData eventData)
    {
        if(CanDrag(eventData))
            UpdateDrag(eventData);
    }

    public virtual void OnInitializePotentialDrag(PointerEventData eventData)
    {
        eventData.useDragThreshold = false;
    }

    private void UpdateVisuals()
    {
        m_Tracker.Clear();
        if (HandleContainer == null) return;

        m_Tracker.Add(this, HandleRect, DrivenTransformProperties.Anchors);
        HandleRect.anchorMin = HandleRect.anchorMax = NormalizedValue;
    }

    private void ValueChanged()
    {
        UpdateVisuals();
        OnValueChanged?.Invoke(new Vector2(Mathf.Lerp(Min.x, Max.x, NormalizedValue.x), Mathf.Lerp(Min.y, Max.y, NormalizedValue.y)));
    }

    public void SetValueNormalized(Vector2 value)
    {
        NormalizedValue = value;
        UpdateVisuals();
    }
}
