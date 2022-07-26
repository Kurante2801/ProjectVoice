using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
#if UNITY_EDITOR
[ExecuteAlways]
#endif
public class TextStyle : MonoBehaviour
{
    private TMP_Text tmp;
    [SerializeField] private FontWeight weight = FontWeight.Medium;

    private void Awake()
    {
        tmp = GetComponent<TMP_Text>();
        tmp.fontWeight = weight;
    }

#if UNITY_EDITOR
    private void Update()
    {
        tmp.fontWeight = weight;
    }
#endif
}
