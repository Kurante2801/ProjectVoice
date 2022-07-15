using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectionFX : MonoBehaviour
{
    public SpriteRenderer SpriteRenderer;
    public int SpawnTime = 0;
    private readonly int lifetime = 600;

    private void Update()
    {
        int time = Conductor.Instance.Time - SpawnTime;
        float perc = time / (float)lifetime;
        float scale = 1f.ScreenScaledX() * ParticleManager.Instance.SizeCurve.Evaluate(perc);

        transform.localScale = new Vector3(scale, scale, 1f);
        SpriteRenderer.color = SpriteRenderer.color.WithAlpha(ParticleManager.Instance.AlphaCurve.Evaluate(perc));
        if (time > lifetime)
            ParticleManager.Instance.DisposeEffect(this);
    }

    /*[SerializeField] private AnimationCurve A, W;
    private void Update()
    {
        int time = Mathf.RoundToInt((Time.realtimeSinceStartup * 1000) % lifetime);
        float perc = time / (float)lifetime;
        float scale = 0.5f * W.Evaluate(perc);

        transform.localScale = new Vector3(scale, scale, 1);
        SpriteRenderer.color = SpriteRenderer.color.WithAlpha(A.Evaluate(perc));
    }*/
}
