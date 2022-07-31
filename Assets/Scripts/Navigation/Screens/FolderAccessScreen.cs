using System.Collections;
using System.Collections.Generic;
using TMPro;
using SimpleFileBrowser;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class FolderAccessScreen : Screen
{
    public override string GetID() => "FolderAccessScreen";

    [SerializeField] private TMP_Text accessState;
    [SerializeField] private TMP_InputField directory;
    [SerializeField] private Button returnButton;

    public static bool CanLeave = true; // Require folder to continue

    public override void OnScreenBecameActive()
    {
        returnButton.interactable = CanLeave;
        accessState.text = "FOLDERACCESS_REQUEST".Get();
        directory.text = "";

        // Check for existing
        if (!string.IsNullOrEmpty(PlayerSettings.UserDataPath.Value))
        {
            directory.text = PlayerSettings.UserDataPath.Value;
            accessState.text = "FOLDERACCESS_LOADING".Get();

            var entries = FileBrowserHelpers.GetEntriesInDirectory(PlayerSettings.UserDataPath.Value, false);
            if (entries == null)
            {
                accessState.text = "FOLDERACCESS_FAILURE".Get();
                CanLeave = false;
                returnButton.interactable = false;
            }
            else
            {
                accessState.text = "FOLDERACCESS_SUCCESS".Get().Replace("{DIRCOUNT}", entries.Where(entry => entry.IsDirectory).ToArray().Length.ToString());
                CanLeave = true;
                returnButton.interactable = true;
            }
        }

        base.OnScreenBecameActive();
    }

    public void SelectButton()
    {
        FileBrowser.ShowLoadDialog(paths =>
        {
            string path = paths[0];
            // Make sure path is not root of storage
            if (Context.AndroidVersionCode <= 29 && Regex.IsMatch(path, @"^\/storage\/emulated\/\d+$"))
            {
                path = System.IO.Path.Combine(path, "Project Voice");
                System.IO.Directory.CreateDirectory(path); // We can use IO on api level 29 and below
            }

            PlayerSettings.UserDataPath.Value = path;
            OnScreenBecameActive();
        }, () => { }, FileBrowser.PickMode.Folders, false, title: "Select folder containing all levels");
    }

    public void ReturnButton()
    {
        if (FileBrowser.IsOpen || !CanLeave) return;

        if (!Context.ScreenManager.TryReturnScreen())
            Context.ScreenManager.ChangeScreen("InitializationScreen", addToHistory: false);
    }
}
