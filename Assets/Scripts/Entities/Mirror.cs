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

    private static readonly List<PlaceableEntity> candidateBuffer = new List<PlaceableEntity>();
    private readonly List<Chick> chicksInDuplicationZone = new List<Chick>();
    private readonly HashSet<Chick> chicksInDuplicationZoneSet = new HashSet<Chick>();
    private readonly List<Chick> duplicationCandidates = new List<Chick>();
    private readonly List<Chick> chicksReadyToDuplicate = new List<Chick>();
    private readonly ZoneDwellTracker<Chick> duplicationDwell = new ZoneDwellTracker<Chick>();
    private readonly List<Chick> chicksInGuideZone = new List<Chick>();
    private readonly List<Chick> chicksReadyToGuide = new List<Chick>();
    private readonly ZoneDwellTracker<Chick> guideDwell = new ZoneDwellTracker<Chick>();
    private MirrorReflection reflection;
    private SpriteMask glassMask;

    public IReadOnlyList<Chick> ChicksInDuplicationZone => chicksInDuplicationZone;

    private static Chick SourceMutantPrefab =>
        UpgradeManager.Instance != null && UpgradeManager.Instance.MutationData != null
            ? UpgradeManager.Instance.MutationData.MutatedChickPrefab
            : null;

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
        chicksInDuplicationZone.Clear();
        chicksInDuplicationZoneSet.Clear();
        duplicationDwell.Clear();
        guideDwell.Clear();
    }

    private void Update()
    {
        if (!duplicationEnabled)
        {
            return;
        }

        UpdateDuplicationZoneMembership();
        UpdateGuideZone();
        UpdateDuplication();
    }

    private Vector3 DuplicationZoneCenter => transform.position + (Vector3)zoneOffset;

    private Vector3 GuideZoneCenter => transform.position + (Vector3)assistOffset;

    private Vector3 GuideTarget => transform.position + (Vector3)assistTargetOffset;

    private void UpdateDuplicationZoneMembership()
    {
        chicksInDuplicationZone.Clear();
        chicksInDuplicationZoneSet.Clear();
        CollectChicksInBox(DuplicationZoneCenter, zoneHalfExtents, chicksInDuplicationZone);

        for (int i = 0; i < chicksInDuplicationZone.Count; i++)
        {
            chicksInDuplicationZoneSet.Add(chicksInDuplicationZone[i]);
        }
    }

    private void UpdateDuplication()
    {
        duplicationCandidates.Clear();
        for (int i = 0; i < chicksInDuplicationZone.Count; i++)
        {
            Chick chick = chicksInDuplicationZone[i];
            if (!chick.IsMirrorExiting && !chick.IsCarried && !chick.IsMerging)
            {
                duplicationCandidates.Add(chick);
            }
        }

        chicksReadyToDuplicate.Clear();
        duplicationDwell.Tick(duplicationCandidates, Time.deltaTime, data.DwellTime, chicksReadyToDuplicate);

        for (int i = 0; i < chicksReadyToDuplicate.Count; i++)
        {
            Chick chick = chicksReadyToDuplicate[i];
            if (chick != null && chick.gameObject.activeInHierarchy)
            {
                Duplicate(chick);
            }
        }
    }

    private void UpdateGuideZone()
    {
        chicksInGuideZone.Clear();
        CollectChicksInBox(GuideZoneCenter, assistHalfExtents, chicksInGuideZone);

        for (int i = chicksInGuideZone.Count - 1; i >= 0; i--)
        {
            Chick chick = chicksInGuideZone[i];
            if (chick.IsCarried || chick.IsMirrorExiting || chick.IsGuided || chick.IsMerging || chicksInDuplicationZoneSet.Contains(chick))
            {
                chicksInGuideZone.RemoveAt(i);
            }
        }

        chicksReadyToGuide.Clear();
        guideDwell.Tick(chicksInGuideZone, Time.deltaTime, data.GuideDwellTime, chicksReadyToGuide);

        for (int i = 0; i < chicksReadyToGuide.Count; i++)
        {
            Chick chick = chicksReadyToGuide[i];
            if (chick == null || !chick.gameObject.activeInHierarchy)
            {
                continue;
            }

            Vector3 destination = GuideTarget + (Vector3)(UnityEngine.Random.insideUnitCircle * assistTargetJitter);
            chick.GuideTo(destination);
        }
    }

    private void Duplicate(Chick source)
    {
        Vector3 spawnPos = reflection.GetReflectionPosition(source);
        Chick copy = SpawnManager.Instance.SpawnChick(ResolveCopyPrefab(source), spawnPos, chickSpawnParent);

        Vector2 exitDir = transform.right * (UnityEngine.Random.value < 0.5f ? 1f : -1f);
        source.ForceMirrorExit(exitDir, data, masked: false);
        copy.ForceMirrorExit(exitDir, data, masked: true, glassMask != null ? glassMask.bounds : (Bounds?)null);

        AudioManager.Instance?.PlayChickDuplicated();

        OnDuplicated?.Invoke();
    }

    private static Chick ResolveCopyPrefab(Chick source)
    {
        GameObject sourcePrefab = source.SourcePrefab;
        if (sourcePrefab != null && sourcePrefab.TryGetComponent(out Chick prefabChick))
        {
            return prefabChick;
        }

        return source.IsMutant && SourceMutantPrefab != null ? SourceMutantPrefab : SpawnManager.Instance.DefaultChickPrefab;
    }

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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(DuplicationZoneCenter, new Vector3(zoneHalfExtents.x * 2f, zoneHalfExtents.y * 2f, 0f));

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(GuideZoneCenter, new Vector3(assistHalfExtents.x * 2f, assistHalfExtents.y * 2f, 0f));

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(GuideTarget, assistTargetJitter);
        Gizmos.DrawLine(GuideTarget + Vector3.left * 0.1f, GuideTarget + Vector3.right * 0.1f);
        Gizmos.DrawLine(GuideTarget + Vector3.down * 0.1f, GuideTarget + Vector3.up * 0.1f);
    }
}
