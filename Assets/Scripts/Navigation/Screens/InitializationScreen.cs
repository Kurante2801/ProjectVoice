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

        // TODO: This needs refactoring, it's not reliable (especially with Auto modifier)
        if (Context.SelectedLevel != null)
        {
            if(Context.State != null && Context.State.IsCompleted && Context.SelectedChart != null && !Context.Modifiers.Contains(Modifier.Auto))
                Context.ScreenManager.ChangeScreen("ResultScreen", addToHistory: false, destroyOld: true);
            else
                Context.ScreenManager.ChangeScreen("LevelSummaryScreen");
            return;
        }

        await Context.LevelManager.LoadLevels();
        Context.ScreenManager.ChangeScreen("LevelSelectionScreen", destroyOld: true);
    }
}
