using System.Collections;
using System.Collections.Generic;
using TMPro;
using SimpleFileBrowser;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using System;
using System.IO;

public class FolderAccessScreen : Screen
{
    public override string GetID() => "FolderAccessScreen";

    [SerializeField] private TMP_Text accessState;
    [SerializeField] private TMP_InputField directory;
    [SerializeField] private Button returnButton;

    public static bool CanLeave = true;
    public static bool IsFirstTime = true;
    private static bool shouldRestart = false;

    public override void OnScreenInitialized()
    {
        base.OnScreenInitialized();
        shouldRestart = false;
    }

    public override void OnScreenBecameActive()
    {
        IsFirstTime = false;

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

    public void SelectButton() => StorageUtil.BrowseFolder(path =>
    {
        if (string.IsNullOrEmpty(path)) return;

        if (!StorageUtil.DirectoryExists(path))
        {
            Debug.LogError("Directory does not exist");
            return;
        }

        // Make sure path is not root of storage
        if (Context.AndroidVersionCode <= 29 && Regex.IsMatch(path, @"^\/storage\/emulated\/\d+$"))
        {
            path = Path.Combine(path, "Project Voice");
            // We can use IO on api level 29 and below
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
        shouldRestart = PlayerSettings.UserDataPath.Value != path;
        PlayerSettings.UserDataPath.Value = path;

        // Create .nomedia folder
        if (!StorageUtil.GetSubfilePath(path, ".nomedia", out _))
            StorageUtil.CreateFile(path, ".nomedia");

        OnScreenBecameActive();
    });

    public async void ReturnButton()
    {
        if (FileBrowser.IsOpen || !CanLeave) return;

        if (!IsFirstTime && shouldRestart)
        {
            Context.ScreenManager.ChangeScreen(null);
            Context.FadeOutSongPreview();
            Backdrop.Instance.SetBackdrop(null);
            Context.SelectedLevel = null;

            await UniTask.Delay(TimeSpan.FromSeconds(0.25f));
            UnityEngine.SceneManagement.SceneManager.LoadScene("Navigation");
            return;
        }

        if (!Context.ScreenManager.TryReturnScreen())
            Context.ScreenManager.ChangeScreen("InitializationScreen", addToHistory: false);
    }
}
