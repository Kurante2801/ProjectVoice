using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScreen2 : Screen
{
    public override string GetID() => "TestScreen2";

    public void Button()
    {
        Context.ScreenManager.ChangeScreen("TestScreen1");
    }
}
