using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// https://github.com/StompyRobot/SRF/blob/master/Scripts/UI/CopyLayoutElement.cs

namespace SRF.UI
{

	/// <summary>
	/// Copies the preferred size of another layout element (useful for a parent object basing its sizing from a child element).
	/// This does have very quirky behaviour, though.
	/// TODO: Write custom editor for this to match layout element editor
	/// </summary>
	[RequireComponent(typeof(RectTransform))]
	[ExecuteInEditMode]
	public class CopyLayoutElement : UIBehaviour, ILayoutElement
	{
		public RectTransform CopySource;

		public bool CopyPreferredWidth;
		public bool CopyPreferredHeight;

		public bool CopyMinWidth;
		public bool CopyMinHeight;

		public float PaddingPreferredWidth;
		public float PaddingPreferredHeight;

		public float PaddingMinWidth;
		public float PaddingMinHeight;

		public float preferredWidth
		{
			get
			{
				if (!CopyPreferredWidth || CopySource == null || !IsActive())
					return -1f;
				return LayoutUtility.GetPreferredWidth(CopySource) + PaddingPreferredWidth;
			}
		}


		public float preferredHeight
		{
			get
			{
				if (!CopyPreferredHeight || CopySource == null || !IsActive())
					return -1f;
				return LayoutUtility.GetPreferredHeight(CopySource) + PaddingPreferredHeight;
			}
		}

		public float minWidth
		{
			get
			{
				if (!CopyMinWidth || CopySource == null || !IsActive())
					return -1f;
				return LayoutUtility.GetMinWidth(CopySource) + PaddingMinWidth;
			}
		}

		public float minHeight
		{
			get
			{
				if (!CopyMinHeight || CopySource == null || !IsActive())
					return -1f;
				return LayoutUtility.GetMinHeight(CopySource) + PaddingMinHeight;
			}
		}

		public int layoutPriority
		{
			get { return 2; }
		}

		public float flexibleHeight { get { return -1; } }
		public float flexibleWidth { get { return -1; } }

		public void CalculateLayoutInputHorizontal() { }

		public void CalculateLayoutInputVertical() { }

#if UNITY_EDITOR
		protected override void OnValidate()
        {
			if (CopySource == transform)
			{
				Debug.LogError("Copy Source is the same as current transform!");
				CopySource = null;
			}
			base.OnValidate();
        }
#endif
    }
}