using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Pool;
using System;

public class ParticleManager : SingletonMonoBehavior<ParticleManager>
{
    private ObjectPool<CollectionFX> collectionPool;
    private ObjectPool<ParticleSystem> holdPool;
    [SerializeField] private Transform poolContainer;
    [SerializeField] private CollectionFX collectionPrefab;
    [SerializeField] private ParticleSystem holdPrefab;
    [SerializeField] private ParticleSystem.MinMaxCurve holdEndCurve;

    private List<CollectionFX> createdFX = new();
    private List<ParticleSystem> createdHolds = new();
    private List<ParticleSystemDiscard> discardedHolds = new();

    [SerializeField] private Material HoldMaterial;
    private Dictionary<NoteGrade, Material> holdMaterials = new();
    [SerializeField] private List<Texture2D> holdGood, holdGreat, holdPerfect;

    public int AnimationTime = 600;
    public AnimationCurve SizeCurve;
    public AnimationCurve AlphaCurve;
    public float HoldEndTime = 1.5f;

    private void Start()
    {
        collectionPool = new ObjectPool<CollectionFX>(CreateFX, OnGetFX, OnReleaseFX);
        holdPool = new ObjectPool<ParticleSystem>(CreateHold, OnGetHold, OnReleaseHold);

        // Create materials according to hold shape
        foreach(NoteGrade grade in Enum.GetValues(typeof(NoteGrade)))
        {
            if (grade == NoteGrade.Miss || grade == NoteGrade.None) continue;

            var mat = new Material(HoldMaterial);

            var tex = grade switch
            {
                NoteGrade.Good => holdGood[(int)PlayerSettings.HoldShape.Value],
                NoteGrade.Great => holdGreat[(int)PlayerSettings.HoldShape.Value],
                NoteGrade.Perfect => holdPerfect[(int)PlayerSettings.HoldShape.Value],
                _ => null
            };

            mat.mainTexture = tex;
            holdMaterials[grade] = mat;
        }
    }

    private void Update()
    {
        for (int i = discardedHolds.Count - 1; i > -1; i--)
        {
            var discarded = discardedHolds[i];
            if (Time.time - discarded.Time > HoldEndTime)
            {
                var sol = discarded.ParticleSystem.sizeOverLifetime;
                sol.size = discarded.Curve;

                discardedHolds.RemoveAt(i);
                holdPool.Release(discarded.ParticleSystem);
            }
        }
    }

    private CollectionFX CreateFX()
    {
        return Instantiate(collectionPrefab.gameObject, poolContainer).GetComponent<CollectionFX>();
    }

    private ParticleSystem CreateHold()
    {
        return Instantiate(holdPrefab.gameObject, poolContainer).GetComponent<ParticleSystem>();
    }

    private void OnGetFX(CollectionFX fx)
    {
        fx.transform.parent = transform;
        fx.gameObject.SetActive(true);
    }

    private void OnGetHold(ParticleSystem system)
    {
        system.transform.parent = transform;
        system.gameObject.SetActive(true);
    }

    private void OnReleaseFX(CollectionFX fx)
    {
        fx.gameObject.SetActive(false);
        fx.gameObject.transform.parent = poolContainer;
    }

    private void OnReleaseHold(ParticleSystem hold)
    {
        hold.gameObject.SetActive(false);
        hold.gameObject.transform.parent = poolContainer;
        hold.Stop();
    }

    public void DisposeEffect(CollectionFX effect)
    {
        createdFX.Remove(effect);
        collectionPool.Release(effect);
    }

    public void DisposeHold(ParticleSystem hold)
    {
        createdHolds.Remove(hold);
        holdPool.Release(hold);

        for (int i = discardedHolds.Count - 1; i > -1; i--)
        {
            if (discardedHolds[i].ParticleSystem == hold)
            {
                discardedHolds.RemoveAt(i);
                break;
            }
        }
    }

    public void SpawnEffect(NoteShape shape, NoteGrade grade, Vector2 pos)
    {
        var fx = collectionPool.Get();
        fx.SpriteRenderer.sprite = Game.Instance.ShapesAtlas[(int)shape].GetSprite("grade_" + grade.ToString().ToLower());
        fx.transform.position = pos;
        fx.SpawnTime = Conductor.Instance.Time;

        createdFX.Add(fx);
    }

    public ParticleSystem SpawnHold(NoteGrade grade, Transform parent)
    {
        var hold = holdPool.Get();
        hold.transform.parent = parent;
        hold.transform.localPosition = Vector3.zero;
#pragma warning disable CS0618
        hold.startSize = 55f.ScreenScaledX();
        hold.gameObject.GetComponent<ParticleSystemRenderer>().material = holdMaterials[grade];
        hold.time = 0f;
        hold.Play();

        return hold;
    }

    public void EndHold(ParticleSystem hold)
    {
        hold.transform.parent = transform;
        hold.Stop();

        discardedHolds.Add(new ParticleSystemDiscard()
        {
            ParticleSystem = hold,
            Curve = hold.sizeOverLifetime.size,
            Time = Time.time
        });

        var sol = hold.sizeOverLifetime;
        sol.size = holdEndCurve;
    }

    public void WipeEffects()
    {
        foreach(var fx in createdFX)
            collectionPool.Release(fx);
        createdFX.Clear();

        foreach(var hold in createdHolds)
            holdPool.Release(hold);
        createdHolds.Clear();
    }

    private class ParticleSystemDiscard
    {
        public ParticleSystem ParticleSystem;
        public ParticleSystem.MinMaxCurve Curve;
        public float Time;
    }
}
