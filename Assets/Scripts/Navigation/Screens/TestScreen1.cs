using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScreen1 : Screen
{
    public override string GetID() => "TestScreen1";

    public void Button()
    {
        Context.ScreenManager.ChangeScreen("TestScreen2");
    }
}
