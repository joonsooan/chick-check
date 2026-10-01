using System.Collections.Generic;
using UnityEngine;

public class DebugAutoplay : MonoBehaviour
{
    [SerializeField, Min(0.1f)] private float playingTimeScale = 1f;
    [SerializeField] private bool autoGuideEnabled = true;
    [SerializeField, Min(0.1f)] private float guideInterval = 3f;
    [SerializeField, Min(1)] private int chicksPerGuide = 5;

    private readonly List<Chick> guideCandidates = new List<Chick>();
    private Mirror mirror;
    private float guideTimer;

    private void Start()
    {
        mirror = FindFirstObjectByType<Mirror>();
        GameManager.Instance?.SetDebugPlayingTimeScale(playingTimeScale);
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            GameManager.Instance?.SetDebugPlayingTimeScale(playingTimeScale);
        }
    }

    private void Update()
    {
        if (!autoGuideEnabled || GameManager.Instance.CurrentPhase != GamePhase.Playing)
        {
            return;
        }

        guideTimer -= Time.deltaTime;
        if (guideTimer > 0f)
        {
            return;
        }

        guideTimer = guideInterval;
        GuideChicksToMirror();
    }

    private void GuideChicksToMirror()
    {
        guideCandidates.Clear();
        IReadOnlyList<PlaceableEntity> entities = PlaceableEntity.ActiveEntities;
        for (int i = 0; i < entities.Count; i++)
        {
            if (entities[i] is Chick chick && !chick.IsCarried && !chick.IsMirrorExiting
                && !chick.IsGuided && !chick.IsMerging)
            {
                guideCandidates.Add(chick);
            }
        }

        int count = Mathf.Min(chicksPerGuide, guideCandidates.Count);
        for (int i = 0; i < count; i++)
        {
            int pick = Random.Range(i, guideCandidates.Count);
            (guideCandidates[i], guideCandidates[pick]) = (guideCandidates[pick], guideCandidates[i]);
            Vector3 destination = mirror.GuideTarget + (Vector3)(Random.insideUnitCircle * mirror.GuideTargetJitter);
            guideCandidates[i].GuideTo(destination, ignoreObstacles: true);
        }
    }
}
