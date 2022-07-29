using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GridLayoutGroup))]
#if UNITY_EDITOR
[ExecuteAlways]
#endif
public class GridDynamicSize : MonoBehaviour
{
    public float Width = 360, Height = 214, Padding = 68;
    public RectTransform RectTransform;
    private float w;
    private GridLayoutGroup gridLayout;
    private void Awake()
    {
        gridLayout = GetComponent<GridLayoutGroup>();
    }

    private void Update()
    {
        if (RectTransform.sizeDelta.x != w)
        {
            w = RectTransform.sizeDelta.x;
            var ratio = Width / Height;

            var scaled = Width / Context.ReferenceWidth * (Context.ScreenWidth + RectTransform.sizeDelta.x);
            gridLayout.cellSize = new Vector2(scaled, scaled / ratio);
            gridLayout.padding.left = (int)(Padding / Context.ReferenceWidth * (Context.ScreenWidth + RectTransform.sizeDelta.x));
        }
    }

}
