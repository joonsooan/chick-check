using System.Collections.Generic;
using UnityEngine;

public class MirrorReflection : MonoBehaviour
{
    public enum ReflectionMode
    {
        NextChickOnly,
        AllChicks
    }

    [SerializeField] private SpriteRenderer reflectionPrefab;
    [SerializeField] private Transform reflectionContainer;

    [Header("Mode")]
    [SerializeField] private ReflectionMode mode = ReflectionMode.NextChickOnly;

    [Header("Appearance")]
    [SerializeField] private Color tint = new Color(1f, 1f, 1f, 0.5f);
    [SerializeField] private Vector2 positionOffset;
    [SerializeField] private float scale = 1f;

    [Header("Reflection Axis")]
    [SerializeField] private float axisOffset;

    [Header("Glass Area")]
    [SerializeField] private Vector2 glassCenter;
    [SerializeField] private Vector2 glassSize = Vector2.one;

    private Mirror mirror;
    private readonly Dictionary<Chick, SpriteRenderer> active = new Dictionary<Chick, SpriteRenderer>();
    private readonly List<Chick> targets = new List<Chick>();
    private readonly List<Chick> stale = new List<Chick>();

    private void Awake()
    {
        mirror = GetComponent<Mirror>();
    }

    private void OnDisable()
    {
        foreach (SpriteRenderer sr in active.Values)
        {
            if (sr != null)
            {
                PoolManager.Despawn(sr.gameObject);
            }
        }
        active.Clear();
    }

    private void LateUpdate()
    {
        BuildTargets();
        ReleaseStale();
        UpdateActive();
    }

    private void BuildTargets()
    {
        targets.Clear();

        if (mode == ReflectionMode.NextChickOnly)
        {
            Chick source = mirror.PeekSource();
            if (source != null)
            {
                targets.Add(source);
            }
            return;
        }

        IReadOnlyList<Chick> inZone = mirror.ChicksInZone;
        for (int i = 0; i < inZone.Count; i++)
        {
            Chick chick = inZone[i];
            if (chick != null && chick.gameObject.activeInHierarchy)
            {
                targets.Add(chick);
            }
        }
    }

    private void ReleaseStale()
    {
        stale.Clear();
        foreach (KeyValuePair<Chick, SpriteRenderer> pair in active)
        {
            if (!targets.Contains(pair.Key))
            {
                stale.Add(pair.Key);
            }
        }

        for (int i = 0; i < stale.Count; i++)
        {
            SpriteRenderer sr = active[stale[i]];
            if (sr != null)
            {
                PoolManager.Despawn(sr.gameObject);
            }
            active.Remove(stale[i]);
        }
    }

    private void UpdateActive()
    {
        for (int i = 0; i < targets.Count; i++)
        {
            Chick chick = targets[i];
            if (!active.TryGetValue(chick, out SpriteRenderer sr))
            {
                sr = PoolManager.Instance.Spawn(
                    reflectionPrefab, Vector3.zero, Quaternion.identity, reflectionContainer);
                active.Add(chick, sr);
            }

            DrawReflection(sr, chick);
        }
    }

    private void DrawReflection(SpriteRenderer sr, Chick chick)
    {
        SpriteRenderer source = chick.SpriteRenderer;
        sr.sprite = source.sprite;
        sr.flipX = source.flipX;
        sr.color = tint;
        sr.transform.localScale = Vector3.one * scale;
        sr.transform.position = Reflect(chick.transform.position);
    }

    private Vector3 Reflect(Vector3 chickWorldPos)
    {
        float axisY = transform.TransformPoint(new Vector2(0f, axisOffset)).y;
        return new Vector3(chickWorldPos.x, 2f * axisY - chickWorldPos.y, chickWorldPos.z);
    }

    public Vector3 GetReflectionPosition(Chick chick) => Reflect(chick.transform.position);

    public void ReleaseReflection(Chick chick)
    {
        if (active.TryGetValue(chick, out SpriteRenderer sr))
        {
            if (sr != null)
            {
                PoolManager.Despawn(sr.gameObject);
            }
            active.Remove(chick);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = new Color(0.4f, 0.7f, 1f, 1f);
        Gizmos.DrawWireCube(glassCenter + positionOffset, glassSize);

        Gizmos.color = new Color(1f, 0.6f, 0.2f, 1f);
        Gizmos.DrawLine(new Vector3(-2f, axisOffset), new Vector3(2f, axisOffset));
    }
}
