using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalibrationOpener : MonoBehaviour
{
    public void OnButtonClicked()
    {
        Context.ScreenManager.ChangeScreen("CalibrationScreen");
        Context.FadeOutSongPreview();
    }
}
