using DG.Tweening;
using DG.Tweening.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MainColorEvent : MonoBehaviour
{
    public float TransitionDuration = 0f;
    public List<Graphic> Graphics = new();

    private void OnEnable()
    {
        Context.OnMainColorChanged.AddListener(MainColorChanged);
        MainColorChanged();
    }

    private void OnDisable()
    {
        Context.OnMainColorChanged.RemoveListener(MainColorChanged);
    }

    private void MainColorChanged()
    {
        foreach(var graphic in Graphics)
        {
            graphic.DOKill();
            graphic.DOColor(Context.MainColor, TransitionDuration);
        }
    }

}