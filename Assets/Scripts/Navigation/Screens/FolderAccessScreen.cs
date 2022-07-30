using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FolderAccessScreen : Screen
{
    [SerializeField] private TMP_Text state;
    [SerializeField] private TMP_InputField directory;

    public override void OnScreenInitialized()
    {
        state.text = "";
        directory.text = "";

        base.OnScreenInitialized();
    }

    public void SelectButton()
    {
        print("Select Button Pressed");
    }

    public void ReturnButton()
    {
        print("Return Button Pressed");
    }
}
