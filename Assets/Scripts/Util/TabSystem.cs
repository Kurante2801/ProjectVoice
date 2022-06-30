using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabSystem : MonoBehaviour
{
    public List<Button> Buttons = new();
    public List<CanvasGroup> Targets = new();
    public int InitialTab = 0;
    private int active = 0;

    private void Awake()
    {
        active = InitialTab;
        for(int i = 0; i < Buttons.Count; i++)
        {

            var button = Buttons[i];
            button.image.color = i == active ? Context.MainColor : Context.Foreground2Color;
            int lambda = i; // Can't use i in the AddListener
            button.onClick.AddListener(() => OnButtonPressed(lambda));

            var target = Targets[i];
            target.gameObject.SetActive(i == active);
            target.alpha = i == active ? 1f : 0f;
        }
    }

    public void OnButtonPressed(int index)
    {
        for(int i = 0; i < Buttons.Count; i++)
        {
            var button = Buttons[i];
            button.image.DOKill();
            button.image.DOColor(index == i ? Context.MainColor : Context.Foreground2Color, 0.25f);
            
            var target = Targets[i];
            target.DOKill();
            
            if(i == index)
            {
                if (!target.gameObject.activeInHierarchy)
                {
                    target.gameObject.SetActive(true);
                    target.alpha = 0f;
                }

                target.DOFade(1f, 0.25f);
            }
            else if (target.gameObject.activeInHierarchy)
                target.DOFade(0f, 0.25f).OnComplete(() => target.gameObject.SetActive(false));
        }
    }
}
