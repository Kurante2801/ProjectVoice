using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;

public class InitializationScreen : Screen
{
    public override string GetID() => "InitializationScreen";
    public bool IsInitialized { get; protected set; }

    [SerializeField] private GameObject loadingText;
    
    public async override void OnScreenTransitionInEnded()
    {
        base.OnScreenTransitionInEnded();
        Context.SetupProfiler();

        if(!Context.LocalizationManager.Initialized)
            await Context.LocalizationManager.Initialize();

        // Android scoped storage stuff
        string path = PlayerSettings.LevelsPath.Value;
        if (string.IsNullOrEmpty(path))
        {
            Context.ScreenManager.ChangeScreen("FolderAccessScreen", addToHistory: false, destroyOld: true);
            return;
        }
        else
        {
            // Check access
            var paths = SimpleFileBrowser.FileBrowserHelpers.GetEntriesInDirectory(path, false);
            if (paths == null)
            {
                Context.ScreenManager.ChangeScreen("FolderAccessScreen", addToHistory: false, destroyOld: true);
                return;
            }
        }

        if (!Context.LevelManager.Loaded)
        {
            loadingText.SetActive(true);
            await Context.LevelManager.LoadLevels();
        }

        var state = Context.State;
        if (state != null && state.IsCompleted && Context.SelectedChart != null && !Context.Modifiers.Contains(Modifier.Auto))
            Context.ScreenManager.ChangeScreen("ResultScreen", addToHistory: false, destroyOld: true);
        else if (Context.SelectedLevel != null)
            Context.ScreenManager.ChangeScreen("LevelSummaryScreen");
        else
            Context.ScreenManager.ChangeScreen("LevelSelectionScreen", destroyOld: true);
    }
}
