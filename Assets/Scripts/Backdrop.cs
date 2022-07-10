using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class Backdrop : SingletonMonoBehavior<Backdrop>
{
    public Canvas Canvas;
    public RawImage Background, BackgroundBlurred;
    public Image BackgroundOverlay;
    public AspectRatioFitter AspectRatioFitter;
    private string backdropPath;

    public async void SetBackdrop(string path, float? aspect = null, bool blurred = false, float overlay = 0f)
    {
        if (path == backdropPath) return;
        backdropPath = path;

        var valid = !string.IsNullOrWhiteSpace(path);
        if (valid && !File.Exists(path))
        {
            Debug.LogWarning($"Could not find backdrop {path}");
            backdropPath = "";
            valid = false;
        }

        Background.DOKill();

        if (!valid)
        {
            Background.DOFade(0f, 0.25f);
            BackgroundBlurred.DOFade(0f, 0.25f);
        }
        else
        {
            Background.color = Color.white.WithAlpha(0f);

            var tex = await TextureExtensions.LoadTexture(path);
            Background.texture = tex;
            Background.DOFade(1f, 0.25f);

            BackgroundBlurred.texture = tex.Blurred(PlayerSettings.BackgroundBlur);
            BackgroundBlurred.DOFade(blurred ? 1f : 0f, 0.25f);
            BackgroundOverlay.DOFade(overlay, 0.25f);

            AspectRatioFitter.aspectRatio = aspect ?? Background.texture.width / (float)Background.texture.height;
        }
    }

    public void DisplayBlurImage(bool blurred)
    {
        BackgroundBlurred.DOFade(blurred ? 1f : 0f, 0.25f);
    }

    public void SetBlur(int blur)
    {
        if (Background.texture == null) return;
        BackgroundBlurred.texture = Background.texture.Blurred(blur);
    }

    public void SetOverlay(float opacity, float duration = 0.25f)
    {
        BackgroundOverlay.DOFade(opacity, duration);
    }
}
