using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelInfoLine : MonoBehaviour
{
    [SerializeField] private TMP_Text tmp;
    [SerializeField] private Button link;

    public void SetInfo(string name, string text, string url = default)
    {
        bool hasLink = !string.IsNullOrEmpty(url);
        link.gameObject.SetActive(hasLink);
        link.onClick.AddListener(() => Application.OpenURL(url));
        tmp.text = $"<font-weight=500>{name}: </font-weight>{text.SanitizeTMP()}";
    }
    
}
