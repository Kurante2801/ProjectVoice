using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectionScreen : Screen
{
    public override string GetID() => "LevelSelectionScreen";

    public Dictionary<string, LevelCard> Levels = new();

    public GameObject LevelCardPrefab, ScrollView;
    public RectTransform Content;
    public TMPro.TMP_Text NoLevelsText;

    public override void OnScreenInitialized()
    {
        if(Context.LevelManager.LoadedLevels.Count == 0)
        {
            ScrollView.SetActive(false);
            NoLevelsText.gameObject.SetActive(true);

            if (!string.IsNullOrWhiteSpace(Context.FileErrorText))
            {
                NoLevelsText.fontSize = 18f;
                NoLevelsText.text = Context.FileErrorText;
            }
            else
                NoLevelsText.text = "LEVEL_SEL_NOLEVELS".Get().Replace("{PATH}", Context.UserDataPath);

            base.OnScreenInitialized();
            return;
        }

        foreach (Transform child in Content)
            Destroy(child.gameObject);
        
        var sorted = Context.LevelManager.LoadedLevels.Values.OrderBy(level => level.Meta.title).ToList();

        foreach (var level in sorted)
        {
            var card = Instantiate(LevelCardPrefab, Content).GetComponent<LevelCard>();
            card.SetLevel(level);

            Levels[level.ID] = card;
        }

        base.OnScreenInitialized();
    }

    public void OptionsButton()
    {
        Context.ScreenManager.ChangeScreen("OptionsScreen");
    }
}
