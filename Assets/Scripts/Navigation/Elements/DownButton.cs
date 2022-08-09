using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Android;

public class DownButton : MonoBehaviour, IPointerDownHandler
{
    public UnityEvent OnDown = new();
    public void OnPointerDown(PointerEventData eventData) => OnDown?.Invoke();
}
