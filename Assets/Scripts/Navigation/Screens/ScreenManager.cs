using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Heavily super basef off https://github.com/Cytoid/Cytoid/blob/main/Assets/Scripts/Screen/ScreenManager.cs
// List.Find may be faster than List.FirstOrDefault

public class ScreenManager : SingletonMonoBehavior<ScreenManager>
{
    public string InitialScreenId = "InitializationScreen";
    public Canvas RootCanvas;
    [Space]
    public List<Screen> ScreenPrefabs = new();

    public List<Screen> CreatedScreens = new();
    public string ActiveScreenId { get; protected set; }
    public Screen ActiveScreen => CreatedScreens.Find(screen => screen.GetID() == ActiveScreenId);
    public string ChangingToScreenId { get; protected set; }
    public Stack<string> History = new();

    protected override void Awake()
    {
        base.Awake();
        Context.ScreenManager = this;
    }

    private async void Start()
    {
        await UniTask.WaitUntil(() => Context.Instance != null);

        foreach (var screen in CreatedScreens)
            screen.gameObject.SetActive(false);

        if (!string.IsNullOrWhiteSpace(InitialScreenId))
            ChangeScreen(InitialScreenId, ScreenTransition.None);
    }

    public Screen CreateScreen(string screen_id)
    {
        var prefab = ScreenPrefabs.Find(screen => screen.GetID() == screen_id);
        if (prefab == null)
            throw new ArgumentException($"Screen prefab '{screen_id}' not found");

        var obj = Instantiate(prefab.gameObject, RootCanvas.transform);
        var screen = obj.GetComponent<Screen>();
        obj.SetActive(true);
        CreatedScreens.Add(screen);

        return screen;
    }

    public void DestroyScreen(string screen_id)
    {
        var screen = CreatedScreens.Find(screen => screen.GetID() == screen_id);
        if (screen == null) return;

        screen.State = ScreenState.Destroyed;
        Destroy(screen.gameObject);
    }

    public async void ChangeScreen(string screen_id, ScreenTransition transition = ScreenTransition.Fade, float duration = 0.25f, bool addToHistory = true, bool destroyOld = false, bool simultaneous = false)
    {
        if (!string.IsNullOrWhiteSpace(ChangingToScreenId))
        {
            Debug.LogWarning($"Atempted to change screen while transitioning to {ChangingToScreenId}");
            return;
        }

        if (screen_id == ActiveScreenId && ActiveScreen != null)
            transition = ScreenTransition.None;

        if (transition == ScreenTransition.None)
            duration = 0f;

        ChangingToScreenId = screen_id;

        var oldScreen = ActiveScreen;
        ActiveScreenId = screen_id;

        if (oldScreen != null)
        {
            if (oldScreen.GetID() == "FolderAccessScreen" && !FolderAccessScreen.CanLeave)
            {
                Debug.LogError("Tried to leave Folder Access Screen without selecting a folder!");
                ActiveScreenId = "FolderAccessScreen";
                ChangingToScreenId = null;
                return;
            }
            
            oldScreen.CanvasGroup.DOFade(0f, duration);
            oldScreen.OnScreenTransitionOutBegan();

            if (!simultaneous && duration > 0f)
                await UniTask.Delay(TimeSpan.FromSeconds(duration));

            oldScreen.State = ScreenState.Inactive;
            
            if (destroyOld)
                DestroyScreen(oldScreen.GetID());
            else
                oldScreen.gameObject.SetActive(false);
        }

        if (string.IsNullOrEmpty(screen_id)) return;

        var newScreen = CreatedScreens.Find(screen => screen.GetID() == screen_id) ?? CreateScreen(screen_id);
        newScreen.CanvasGroup.alpha = 0f;
        newScreen.gameObject.SetActive(true);
        newScreen.CanvasGroup.DOFade(1f, duration);
        newScreen.State = ScreenState.Active;

        if (addToHistory)
            History.Push(screen_id);

        if(duration > 0f)
            await UniTask.Delay(TimeSpan.FromSeconds(duration));

        ChangingToScreenId = null;
        newScreen.OnScreenTransitionInEnded();
    }

    public string PopAndPeekHistory()
    {
        if (History.Count > 1)
        {
            History.Pop();
            return History.Peek();
        }

        return null;
    }

    public void ReturnScreen(float duration = 0.25f, bool destroy = false, bool simultaneous = false)
    {
        ChangeScreen(PopAndPeekHistory(), default, duration, false, destroy, simultaneous);
    }

    public bool TryReturnScreen(float duration = 0.25f, bool destroy = false, bool simultaneous = false)
    {
        if (History.Count > 2)
        {
            ReturnScreen(duration, destroy, simultaneous);
            return true;
        }
        return false;
    }
}