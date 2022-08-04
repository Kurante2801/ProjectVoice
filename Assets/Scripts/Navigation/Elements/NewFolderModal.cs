using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class NewFolderModal : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_InputField nameField;
    public UnityEvent<string> OnFolderNamed;

    private string folderName = "";

    private void Awake()
    {
        nameField.onEndEdit.AddListener(value => folderName = value);
    }

    public void OnCancelButtonPressed()
    {
        Destroy(gameObject);
    }
    public void OnAcceptButtonPressed()
    {
        OnFolderNamed?.Invoke(folderName);
        OnCancelButtonPressed();
    }
}
