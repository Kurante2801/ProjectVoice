using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectionScreen : Screen
{
    public override string GetID() => "LevelSelectionScreen";

    public Dictionary<string, LevelCard> Levels = new();

    public GameObject LevelCardPrefab;
    public RectTransform Content;

    public override void OnScreenInitialized()
    {
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
