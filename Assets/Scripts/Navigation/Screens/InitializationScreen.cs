using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitializationScreen : Screen
{
    public override string GetID() => "InitializationScreen";
    public bool IsInitialized { get; protected set; }

    public async override void OnScreenTransitionInEnded()
    {
        base.OnScreenTransitionInEnded();

        if (Context.SelectedLevel != null)
        {
            if(Context.State != null && Context.State.IsCompleted && Context.SelectedChart != null)
                Context.ScreenManager.ChangeScreen("ResultScreen", addToHistory: false);
            else
                Context.ScreenManager.ChangeScreen("LevelSummaryScreen");
            return;
        }

        await Context.LevelManager.LoadLevels();
        await Context.LocalizationManager.Initialize();
        Context.ScreenManager.ChangeScreen("LevelSelectionScreen");
    }
}
