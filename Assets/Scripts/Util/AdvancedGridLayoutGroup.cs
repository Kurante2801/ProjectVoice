using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// https://answers.unity.com/questions/1653199/how-to-make-gridlayout-dramatically-resize-content.html

public class AdvancedGridLayoutGroup : GridLayoutGroup
{
    [SerializeField] private int cellsPerLine = 1;
    [SerializeField] private float aspectRatio = 1;

    public override void SetLayoutVertical()
    {
        float width = (this.GetComponent<RectTransform>()).rect.width;
        float useableWidth = width - this.padding.horizontal - (this.cellsPerLine - 1) * this.spacing.x;
        float cellWidth = useableWidth / cellsPerLine;
        this.cellSize = new Vector2(cellWidth, cellWidth * this.aspectRatio);
        base.SetLayoutVertical();
    }
}