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
    private int activeTab = 0;

    private void Awake()
    {
        for(int i = 0; i < Buttons.Count; i++)
        {
            int index = i;
            Buttons[i].onClick.AddListener(() => ChangeTab(index));
        }
    }

    private void OnEnable()
    {
        Context.OnMainColorChanged.AddListener(MainColorChanged);
        MainColorChanged();

        for(int i = 0; i < Targets.Count; i++)
        {
            var target = Targets[i];
            target.DOKill();
            target.alpha = 1f;
            target.gameObject.SetActive(i == activeTab);
        }
    }

    private void OnDisable()
    {
        Context.OnMainColorChanged.RemoveListener(MainColorChanged);
    }

    public void ChangeTab(int index)
    {
        activeTab = index;

        for (int i = 0; i < Targets.Count; i++)
        {
            var target = Targets[i];
            target.DOKill();

            if (target.gameObject.activeInHierarchy)
            {
                if (i == index)
                    target.DOFade(1f, 0.25f);
                else
                    target.DOFade(0f, 0.25f).OnComplete(() => target.gameObject.SetActive(false));
            }
            else if (i == index)
            {
                target.DOKill();
                target.alpha = 0f;
                target.gameObject.SetActive(true);
                target.DOFade(1f, 0.25f);
            }
        }

        for (int i = 0; i < Buttons.Count; i++)
        {
            var button = Buttons[i];
            button.image.DOKill();
            button.image.DOColor(i == index ? Context.MainColor : Context.Foreground1Color, 0.25f);
        }
    }

    private void MainColorChanged()
    {
        for(int i = 0; i < Buttons.Count; i++)
        {
            var button = Buttons[i];
            button.image.DOKill();
            button.image.color = i == activeTab ? Context.MainColor : Context.Foreground1Color;
        }
    }
}
