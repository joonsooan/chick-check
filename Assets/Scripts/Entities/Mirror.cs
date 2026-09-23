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
    [SerializeField] private Vector2 assistHalfExtents = new Vector2(1f, 0.75f);
    [SerializeField] private Vector2 assistOffset = Vector2.zero;
    [SerializeField] private Vector2 assistTargetOffset = new Vector2(0f, -0.3f);
    [SerializeField, Min(0)] private float assistTargetJitter = 0.15f;

    private readonly List<Chick> chicksInZone = new List<Chick>();
    private readonly Dictionary<Chick, float> dwell = new Dictionary<Chick, float>();
    private static readonly List<PlaceableEntity> candidateBuffer = new List<PlaceableEntity>();
    private readonly List<Chick> chicksNowInZone = new List<Chick>();
    private readonly List<Chick> chicksNowInAssistZone = new List<Chick>();
    private readonly List<Chick> assistReachedBuffer = new List<Chick>();
    private readonly ZoneDwellTracker<Chick> assistDwell = new ZoneDwellTracker<Chick>();
    private MirrorReflection reflection;
    private SpriteMask glassMask;

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
        glassMask = GetComponentInChildren<SpriteMask>();
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
        assistDwell.Clear();
        revealingSource = null;
    }

    private void Update()
    {
        if (!duplicationEnabled)
        {
            return;
        }

        UpdateZoneMembership();
        UpdateAssistZone();

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

    private Vector3 AssistZoneCenter => transform.position + (Vector3)assistOffset;

    private Vector3 AssistTarget => transform.position + (Vector3)assistTargetOffset;

    private void UpdateAssistZone()
    {
        chicksNowInAssistZone.Clear();
        CollectChicksInBox(AssistZoneCenter, assistHalfExtents, chicksNowInAssistZone);

        for (int i = chicksNowInAssistZone.Count - 1; i >= 0; i--)
        {
            Chick chick = chicksNowInAssistZone[i];
            if (chick == null || !chick.gameObject.activeInHierarchy ||
                chick.IsCarried || chick.IsExiting || chick.IsGuided || chicksInZone.Contains(chick))
            {
                chicksNowInAssistZone.RemoveAt(i);
            }
        }

        assistReachedBuffer.Clear();
        assistDwell.Update(chicksNowInAssistZone, Time.deltaTime, data.AssistDwellTime, assistReachedBuffer);

        for (int i = 0; i < assistReachedBuffer.Count; i++)
        {
            Chick chick = assistReachedBuffer[i];
            if (chick == null || !chick.gameObject.activeInHierarchy)
            {
                continue;
            }

            Vector3 destination = AssistTarget + (Vector3)(UnityEngine.Random.insideUnitCircle * assistTargetJitter);
            chick.GuideTo(destination);
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
        if (glassMask != null)
        {
            copy.ForceExit(exitDir, data, masked: true, glassMask.bounds);
        }
        else
        {
            copy.ForceExit(exitDir, data, masked: true);
        }

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

    private static void CollectChicksInBox(Vector3 center, Vector2 halfExtents, List<Chick> result)
    {
        candidateBuffer.Clear();
        PlaceableEntity.Spatial.QueryBox(center, halfExtents, candidateBuffer);

        for (int i = 0; i < candidateBuffer.Count; i++)
        {
            if (!(candidateBuffer[i] is Chick chick))
            {
                continue;
            }

            if (Mathf.Abs(chick.transform.position.x - center.x) <= halfExtents.x &&
                Mathf.Abs(chick.transform.position.y - center.y) <= halfExtents.y)
            {
                result.Add(chick);
            }
        }
    }

    private void UpdateZoneMembership()
    {
        chicksNowInZone.Clear();
        CollectChicksInBox(ZoneCenter, zoneHalfExtents, chicksNowInZone);

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

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(AssistZoneCenter, new Vector3(assistHalfExtents.x * 2f, assistHalfExtents.y * 2f, 0f));

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(AssistTarget, assistTargetJitter);
        Gizmos.DrawLine(AssistTarget + Vector3.left * 0.1f, AssistTarget + Vector3.right * 0.1f);
        Gizmos.DrawLine(AssistTarget + Vector3.down * 0.1f, AssistTarget + Vector3.up * 0.1f);
    }
}
