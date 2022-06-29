using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Content Size Fitter doesn't update when
// changing text through code smh...

[ExecuteAlways, RequireComponent(typeof(TMP_Text))]
public class AutoSizeTMP : MonoBehaviour
{
    [SerializeField] private bool adjustWidth, adjustHeight;
    [SerializeField] private float minWidth, minHeight;
    private float w, h;

    private RectTransform rectTransform;
    private TMP_Text text;

    private void Awake()
    {
        text = GetComponent<TMP_Text>();
        rectTransform = transform as RectTransform;
    }

    private void Update()
    {
        bool shouldW = false, shouldH = false;

        if (adjustWidth && w != text.preferredWidth)
        {
            w = text.preferredWidth;
            shouldW = true;
        }

        if (adjustHeight && h != text.preferredHeight)
        {
            h = text.preferredHeight;
            shouldH = true;
        }

        if (shouldW || shouldH)
            rectTransform.sizeDelta = new Vector2(
                shouldW ? Mathf.Max(w, minWidth) : rectTransform.sizeDelta.x,
                shouldH ? Mathf.Max(h, minHeight) : rectTransform.sizeDelta.y
               );
    }
}
