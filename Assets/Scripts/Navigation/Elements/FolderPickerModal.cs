using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DG.Tweening;
using SimpleFileBrowser;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class FolderPickerModal : MonoBehaviour
{
    [SerializeField] private FolderPickerDirectory directoryButtonPrefab;
    [SerializeField] private TMP_InputField pathField;
    [SerializeField] private RectTransform content;
    [SerializeField] private CanvasGroup canvasGroup;
    
    private GameObject blocker;
    private int currentPathIndex = -1;
    private readonly List<string> pathsFollowed = new List<string>();

    public string CurrentPath;

    public UnityEvent<string> OnPathSelected = new();
    private static string AndroidRootPath => Application.persistentDataPath[..Application.persistentDataPath.IndexOf("/Android", StringComparison.Ordinal)];

    private void Start()
    {
#if UNITY_EDITOR
            pathField.interactable = true;
            pathField.onEndEdit.AddListener(GoToPath);
            string initial = Path.GetFullPath(Application.persistentDataPath);
#else
        if (FileBrowser.CheckPermission() != FileBrowser.Permission.Granted)
            FileBrowser.RequestPermission();

        string initial = AndroidRootPath;
#endif
        GoToPath(initial);

        canvasGroup.alpha = 0f;
        canvasGroup.DOKill();
        canvasGroup.DOFade(1f, 0.25f);

        foreach (var layoutGroup in gameObject.GetComponentsInChildren<LayoutGroup>())
            layoutGroup.transform.RebuildLayout();
    }

    public static FolderPickerModal CreateModal(Canvas rootCanvas)
    {
        var blocker = CreateBlocker(rootCanvas);
        var modal = Instantiate(Context.Instance.FolderPickerModalPrefab, blocker.transform).GetComponent<FolderPickerModal>();
        modal.blocker = blocker;

        return modal;
    }

    private static GameObject CreateBlocker(Canvas rootCanvas)
    {
        var blocker = new GameObject("Blocker");
        var rect = blocker.AddComponent<RectTransform>();
        // Make rect fill screen
        rect.SetParent(rootCanvas.transform, false);
        rect.anchorMin = rect.sizeDelta = Vector2.zero;
        rect.anchorMax = Vector2.one;

        var canvas = blocker.AddComponent<Canvas>();
        canvas.sortingOrder = SortingLayer.GetLayerValueFromName("Modals");
        blocker.AddComponent<GraphicRaycaster>();

        var image = blocker.AddComponent<Image>();
        image.color = Color.clear;

        return blocker;
    }

    public void GoToPath(string path)
    {
        if (!Directory.Exists(path)) return;
#if !UNITY_EDITOR && UNITY_ANDROID
        if (!path.StartsWith(AndroidRootPath)) return;
#endif

        CurrentPath = path;
        pathField.SetTextWithoutNotify(path);

        foreach (Transform child in content)
            Destroy(child.gameObject);

        var dirs = Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly).OrderBy(x => x);
        foreach (var dir in dirs)
            Instantiate(directoryButtonPrefab.gameObject, content).GetComponent<FolderPickerDirectory>().SetData(this, dir);

        if (currentPathIndex == -1 || pathsFollowed[currentPathIndex] != path)
        {
            currentPathIndex++;
            if (currentPathIndex < pathsFollowed.Count)
            {
                pathsFollowed[currentPathIndex] = path;
                for (int i = pathsFollowed.Count - 1; i >= currentPathIndex + 1; i--)
                    pathsFollowed.RemoveAt(i);
            }
            else
                pathsFollowed.Add(path);
        }
    }

    public void OnBackButtonPressed()
    {
        if (currentPathIndex > 0)
            GoToPath(pathsFollowed[--currentPathIndex]);
    }

    public void OnForwardButtonPressed()
    {
        if (currentPathIndex < pathsFollowed.Count - 1)
            GoToPath(pathsFollowed[++currentPathIndex]);
    }

    public void OnUpButtonPressed()
    {
        try
        {
            var parentPath = Directory.GetParent(CurrentPath);
            if (parentPath != null)
                GoToPath(Path.GetFullPath(parentPath.FullName));
        }
        catch { }
    }

    public void OnCancelButtonPressed()
    {
        if (blocker != null)
            Destroy(blocker);
        else
            Destroy(gameObject);
    }

    public void OnAcceptButtonPressed()
    {
        var path = CurrentPath;
        OnCancelButtonPressed();
        OnPathSelected?.Invoke(path);
    }
}
