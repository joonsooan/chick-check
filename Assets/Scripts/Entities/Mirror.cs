using System;
using System.Collections.Generic;
using UnityEngine;

public class Mirror : MonoBehaviour
{
    public event Action OnDuplicated;

    [SerializeField] private MirrorData data;
    [SerializeField] private bool duplicationEnabled = true;
    [SerializeField] private Transform chickSpawnParent;
    [SerializeField] private Vector2 zoneHalfExtents = new Vector2(0.5f, 0.25f);
    [SerializeField] private Vector2 zoneOffset = Vector2.zero;

    private readonly List<Chick> chicksInZone = new List<Chick>();
    private readonly Dictionary<Chick, float> dwell = new Dictionary<Chick, float>();
    private static readonly List<PlaceableEntity> candidateBuffer = new List<PlaceableEntity>();
    private readonly List<Chick> chicksNowInZone = new List<Chick>();
    private MirrorReflection reflection;

    private Chick revealingSource;

    public IReadOnlyList<Chick> ChicksInZone => chicksInZone;

    private static Chick SourceMutantPrefab =>
        UpgradeManager.Instance != null && UpgradeManager.Instance.MutationData != null
            ? UpgradeManager.Instance.MutationData.MutatedChickPrefab
            : null;

    public Chick PeekSource()
    {
        if (revealingSource != null && revealingSource.gameObject.activeInHierarchy)
        {
            return revealingSource;
        }
        return GetSourceChick();
    }

    private void Awake()
    {
        reflection = GetComponent<MirrorReflection>();
    }

    private void Start()
    {
        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.OnMirrorUpgrade += HandleMirrorUpgrade;
        }
    }

    private void OnDestroy()
    {
        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.OnMirrorUpgrade -= HandleMirrorUpgrade;
        }
    }

    private void HandleMirrorUpgrade(GameObject replacementPrefab)
    {
        if (replacementPrefab == null)
        {
            return;
        }

        Instantiate(replacementPrefab, transform.position, transform.rotation, transform.parent);
        Destroy(gameObject);
    }

    private void OnDisable()
    {
        chicksInZone.Clear();
        dwell.Clear();
        revealingSource = null;
    }

    private void Update()
    {
        if (!duplicationEnabled)
        {
            return;
        }

        UpdateZoneMembership();

        for (int i = chicksInZone.Count - 1; i >= 0; i--)
        {
            Chick chick = chicksInZone[i];
            if (chick == null || !chick.gameObject.activeInHierarchy)
            {
                RemoveChick(chick, i);
                continue;
            }

            if (chick.IsExiting || chick.IsCarried)
            {
                continue;
            }

            dwell.TryGetValue(chick, out float t);
            t += Time.deltaTime;

            if (t >= data.DwellTime)
            {
                Duplicate(chick);
            }
            else
            {
                dwell[chick] = t;
            }
        }
    }

    private void Duplicate(Chick source)
    {
        revealingSource = source;

        Vector3 spawnPos = reflection.GetReflectionPosition(source);
        Chick copy = source.IsMutant && SourceMutantPrefab != null
            ? SpawnManager.Instance.SpawnChick(SourceMutantPrefab, spawnPos, chickSpawnParent)
            : SpawnManager.Instance.SpawnChick(spawnPos, chickSpawnParent);

        Vector2 exitDir = transform.right * (UnityEngine.Random.value < 0.5f ? 1f : -1f);
        source.ForceExit(exitDir, data, masked: false);
        copy.ForceExit(exitDir, data, masked: true);

        AudioManager.Instance?.PlayChickDuplicated();

        OnDuplicated?.Invoke();
    }

    private Chick GetSourceChick()
    {
        for (int i = chicksInZone.Count - 1; i >= 0; i--)
        {
            Chick chick = chicksInZone[i];
            if (chick == null || !chick.gameObject.activeInHierarchy)
            {
                RemoveChick(chick, i);
                continue;
            }
            return chick;
        }
        return null;
    }

    private void RemoveChick(Chick chick, int index)
    {
        chicksInZone.RemoveAt(index);
        if (chick != null)
        {
            dwell.Remove(chick);
        }
    }

    private Vector3 ZoneCenter => transform.position + (Vector3)zoneOffset;

    private void UpdateZoneMembership()
    {
        candidateBuffer.Clear();
        chicksNowInZone.Clear();

        Vector3 zoneCenter = ZoneCenter;
        PlaceableEntity.Spatial.QueryBox(zoneCenter, zoneHalfExtents, candidateBuffer);

        for (int i = 0; i < candidateBuffer.Count; i++)
        {
            if (!(candidateBuffer[i] is Chick chick))
            {
                continue;
            }

            if (Mathf.Abs(chick.transform.position.x - zoneCenter.x) <= zoneHalfExtents.x &&
                Mathf.Abs(chick.transform.position.y - zoneCenter.y) <= zoneHalfExtents.y)
            {
                chicksNowInZone.Add(chick);
            }
        }

        for (int i = 0; i < chicksNowInZone.Count; i++)
        {
            Chick chick = chicksNowInZone[i];
            if (!chicksInZone.Contains(chick))
            {
                chicksInZone.Add(chick);
                dwell[chick] = 0f;
            }
        }

        for (int i = chicksInZone.Count - 1; i >= 0; i--)
        {
            Chick chick = chicksInZone[i];
            if (!chicksNowInZone.Contains(chick))
            {
                chicksInZone.RemoveAt(i);
                dwell.Remove(chick);
                if (chick == revealingSource)
                {
                    revealingSource = null;
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(ZoneCenter, new Vector3(zoneHalfExtents.x * 2f, zoneHalfExtents.y * 2f, 0f));
    }
}
