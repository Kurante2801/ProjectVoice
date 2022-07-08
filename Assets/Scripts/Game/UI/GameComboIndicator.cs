using TMPro;
using UnityEngine;

public class GameComboIndicator : MonoBehaviour
{
    private TMP_Text tmp;

    private void Awake()
    {
        tmp = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        Game.Instance.OnNoteJudged.AddListener(NoteJudged);
    }

    private void OnDisable()
    {
        if (Game.Instance != null)
            Game.Instance.OnNoteJudged.RemoveListener(NoteJudged);
    }

    private void NoteJudged(Game game, int _)
    {
        tmp.text = game.State.Combo.ToString();
    }
}
