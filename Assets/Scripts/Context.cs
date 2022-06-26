using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Context : SingletonMonoBehavior<Context>
{
    public static ScreenManager ScreenManager;

    protected override void Awake()
    {
        if (GameObject.FindGameObjectsWithTag("Context").Length > 1) // This is 1 instead of 0 because 'this' has the tag too
        {
            Destroy(gameObject);
            return;
        }

        base.Awake();
        DontDestroyOnLoad(gameObject);
        Application.targetFrameRate = 120;

#if UNITY_EDITOR
        Application.runInBackground = true;
#endif
    }
}
