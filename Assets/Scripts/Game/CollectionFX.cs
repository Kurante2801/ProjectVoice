using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectionFX : MonoBehaviour
{
    public SpriteRenderer SpriteRenderer;
    public int SpawnTime = 0;

    private void Update()
    {
        int lifetime = ParticleManager.Instance.AnimationTime;
        int time = Conductor.Instance.Time - SpawnTime;
        float perc = time / (float)lifetime;
        float scale = 1f.ScreenScaledX() * ParticleManager.Instance.SizeCurve.Evaluate(perc);

        transform.localScale = new Vector3(scale, scale, 1f);
        SpriteRenderer.color = Color.white.WithAlpha(ParticleManager.Instance.AlphaCurve.Evaluate(perc));
        if (time > lifetime)
            ParticleManager.Instance.DisposeEffect(this);
    }
}
