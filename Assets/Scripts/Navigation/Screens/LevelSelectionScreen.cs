using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectionScreen : Screen
{
    public override string GetID() => "LevelSelectionScreen";
    
    private readonly Dictionary<string, LevelCard> levels = new();
    [SerializeField] private GameObject levelCardPrefab, scrollView;
    [SerializeField] private RectTransform content;
    [SerializeField] private TMPro.TMP_Text noLevelsText;

    public override void OnScreenBecameActive()
    {
        if(Context.LevelManager.LoadedLevels.Count == 0)
        {
            scrollView.SetActive(false);
            noLevelsText.gameObject.SetActive(true);

            if (!string.IsNullOrWhiteSpace(Context.FileErrorText))
            {
                noLevelsText.fontSize = 18f;
                noLevelsText.text = Context.FileErrorText;
            }
            else
                noLevelsText.text = "LEVEL_SEL_NOLEVELS".Get().Replace("{PATH}", StorageUtil.GetFileName(PlayerSettings.LevelsPath.Value));

            base.OnScreenInitialized();
            return;
        }

        var sorted = Context.LevelManager.LoadedLevels.Values.OrderBy(level => level.Meta.title).ToList();

        // Remove old levels
        foreach (var entry in levels.ToList())
        {
            if (sorted.FirstOrDefault(level => level.Meta.id == entry.Key) == null)
            {
                Destroy(entry.Value.gameObject);
                levels.Remove(entry.Key);
            }
        }

        // Add new levels
        foreach(var level in sorted)
        {
            if (!levels.ContainsKey(level.Meta.id))
            {
                var card = Instantiate(levelCardPrefab, content).GetComponent<LevelCard>();
                card.SetLevel(level);
                levels.Add(level.Meta.id, card);
            }
        }

        base.OnScreenBecameActive();
    }

    public void OptionsButton()
    {
        Context.ScreenManager.ChangeScreen("OptionsScreen");
    }
}
