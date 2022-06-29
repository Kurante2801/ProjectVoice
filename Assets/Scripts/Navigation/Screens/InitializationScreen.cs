using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitializationScreen : Screen
{
    public override string GetID() => "InitializationScreen";
    public bool IsInitialized { get; protected set; }

    public async override void OnScreenBecameActive()
    {
        base.OnScreenBecameActive();

        await Context.LevelManager.LoadLevels();
        Context.ScreenManager.ChangeScreen("LevelSelectionScreen");
    }
}
