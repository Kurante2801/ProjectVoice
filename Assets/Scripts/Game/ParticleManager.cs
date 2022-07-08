using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Pool;

public class ParticleManager : SingletonMonoBehavior<ParticleManager>
{
    public ObjectPool<CollectionFX> pool;
    [SerializeField] private Transform poolContainer;
    [SerializeField] private CollectionFX collectionPrefab;

    public AnimationCurve SizeCurve;
    public AnimationCurve AlphaCurve;

    protected override void Awake()
    {
        base.Awake();
        pool = new ObjectPool<CollectionFX>(CreateFX, OnGetFX, OnReleaseFX);
    }

    private CollectionFX CreateFX()
    {
        return Instantiate(collectionPrefab.gameObject, poolContainer).GetComponent<CollectionFX>();
    }

    private void OnGetFX(CollectionFX fx)
    {
        fx.transform.parent = transform;
        fx.gameObject.SetActive(true);
    }

    private void OnReleaseFX(CollectionFX fx)
    {
        fx.gameObject.SetActive(false);
        fx.gameObject.transform.parent = poolContainer;
    }


    public void DisposeEffect(CollectionFX effect)
    {
        pool.Release(effect);
    }

    public CollectionFX GetPooledEffect() => pool.Get();

    public void SpawnEffect(NoteShape shape, NoteGrade grade, Vector2 pos)
    {
        var fx = GetPooledEffect();
        fx.SpriteRenderer.sprite = Game.Instance.ShapesAtlas[(int)shape].GetSprite("grade_" + grade.ToString().ToLower());
        fx.transform.position = pos;
        fx.SpawnTime = Conductor.Instance.Time;
    }
}   

public enum NoteShape
{
    Diamond, Circle, Hexagon
}
