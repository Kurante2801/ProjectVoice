using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class InitializationScreen : Screen
{
    public override string GetID() => "InitializationScreen";
    public bool IsInitialized { get; protected set; }
    
    public async override void OnScreenTransitionInEnded()
    {
        base.OnScreenTransitionInEnded();
        Context.SetupProfiler();

        if(!Context.LocalizationManager.Initialized)
            await Context.LocalizationManager.Initialize();

        // Android scoped storage stuff
        string path = PlayerSettings.UserDataPath.Value;
        if (string.IsNullOrEmpty(path))
        {
            FolderAccessScreen.CanLeave = false;
            Context.ScreenManager.ChangeScreen("FolderAccessScreen", addToHistory: false);
            return;
        }
        else
        {
            // Check access
            var paths = SimpleFileBrowser.FileBrowserHelpers.GetEntriesInDirectory(path, false);
            if (paths == null)
            {
                FolderAccessScreen.CanLeave = false;
                Context.ScreenManager.ChangeScreen("FolderAccessScreen", addToHistory: false);
                return;
            }
        }

        FolderAccessScreen.IsFirstTime = false;
        Context.UserDataPath = PlayerSettings.UserDataPath.Value;

        if (Context.SelectedLevel != null)
        {
            if(Context.State != null && Context.State.IsCompleted && Context.SelectedChart != null && !Context.Modifiers.Contains(Modifier.Auto))
                Context.ScreenManager.ChangeScreen("ResultScreen", addToHistory: false);
            else
                Context.ScreenManager.ChangeScreen("LevelSummaryScreen");
            return;
        }

        Context.LevelManager.LoadLevels();
        //Context.ScreenManager.ChangeScreen("LevelSelectionScreen");
        Context.ScreenManager.ChangeScreen("CalibrationScreen");
    }
}
