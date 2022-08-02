using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FolderPickerDirectory : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_Text dirName;
    private string path;
    private FolderPickerModal modal;

    public void SetData(FolderPickerModal modal, string path)
    {
        dirName.text = Path.GetFileName(path);
        this.modal = modal;
        this.path = path;
    }

    public void DoClick() => modal.GoToPath(path);
}
