using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class Backdrop : SingletonMonoBehavior<Backdrop>
{
    public Canvas Canvas;
    public RawImage Background;
    public RawImage BackgroundBlurred;
    public AspectRatioFitter AspectRatioFitter;
    private string backdropPath;

    public async void SetBackdrop(string path, float? aspect = null)
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
            Background.DOFade(0f, 0.25f);
        else
        {
            Background.color = Color.white.WithAlpha(0f);
            Background.texture = await TextureExtensions.LoadTexture(path);
            Background.DOColor(Color.white, 0.25f);

            AspectRatioFitter.aspectRatio = aspect ?? Background.texture.width / (float)Background.texture.height;
        }
    }
}
