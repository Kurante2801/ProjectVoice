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
using System.Threading.Tasks;

public class FolderAccessScreen : Screen
{
    public override string GetID() => "FolderAccessScreen";

    [SerializeField] private TMP_Text header;
    [SerializeField] private TMP_InputField directory;
    [SerializeField] private Button browseButton, nextButton;

    public override void OnScreenBecameActive()
    {
        nextButton.interactable = Context.LevelManager.Loaded;

        if (Context.LevelManager.Loaded)
            header.text = "FOLDERACCESS_SUCCESS".Get().Replace("{LEVELS}", Context.LevelManager.LoadedLevels.Count.ToString());
        else
            header.text = "FOLDERACCESS_REQUEST".Get();

        string path = PlayerSettings.LevelsPath.Value;
        directory.text = path != null ? StorageUtil.GetFileName(path) : "";

        base.OnScreenBecameActive();
    }

    private void DisableButtons()
    {
        nextButton.interactable = false;
        browseButton.interactable = false;
    }

    private void EnableButtons()
    {
        nextButton.interactable = Context.LevelManager.Loaded;
        browseButton.interactable = true;
    }

    public void BrowseButton()
    {
        DisableButtons();
        StorageUtil.BrowseFolder(async path =>
        {
            if(path == null)
            {
                EnableButtons();
                return;
            }

            if (!StorageUtil.DirectoryExists(path))
            {
                Debug.LogError("Directory does not exist\n" + path);
                EnableButtons();
                return;
            }

#if UNITY_ANDROID && !UNITY_EDITOR
            // Make sure path is not root of storage
            if(Context.AndroidVersionCode <= 29 && Regex.IsMatch(path, @"^\/storage\/emulated\/\d+$"))
            {
                path = Path.Combine(path, "Project Voice");
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
            }

            // Create .nomedia
            if (!StorageUtil.GetSubfilePath(path, ".nomedia", out _))
                StorageUtil.CreateFile(path, ".nomedia");
#endif

            directory.text = StorageUtil.GetFileName(path);

            header.text = "FOLDERACCESS_LOADING".Get();
            try
            {
                PlayerSettings.LevelsPath.Value = path;
                await Context.LevelManager.LoadLevels();
            }
            catch(Exception e)
            {
                header.text = "FOLDERACCESS_FAILURE".Get();
                Debug.LogError(e);
            }

            if (Context.LevelManager.Loaded)
                header.text = "FOLDERACCESS_SUCCESS".Get().Replace("{LEVELS}", Context.LevelManager.LoadedLevels.Count.ToString());
            EnableButtons();
        });
    }

    public void NextButton()
    {
        if (FileBrowser.IsOpen) return;

        if (!Context.ScreenManager.TryReturnScreen())
            Context.ScreenManager.ChangeScreen("LevelSelectionScreen", addToHistory: false);
    }
}
