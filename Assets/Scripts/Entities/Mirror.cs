using System.Collections.Generic;
using UnityEngine;

public class Mirror : MonoBehaviour
{
    [SerializeField] private MirrorData data;
    [SerializeField] private bool duplicationEnabled = true;

    private readonly List<Chick> chicksInZone = new List<Chick>();
    private float timer;

    public IReadOnlyList<Chick> ChicksInZone => chicksInZone;
    public Chick PeekSource() => GetSourceChick();

    private void OnDisable()
    {
        chicksInZone.Clear();
        timer = 0f;
    }

    private void Update()
    {
        if (!duplicationEnabled || chicksInZone.Count == 0)
        {
            return;
        }

        timer += Time.deltaTime;
        if (timer < data.DuplicateInterval)
        {
            return;
        }
        timer -= data.DuplicateInterval;

        Duplicate();
    }

    private void Duplicate()
    {
        Chick source = GetSourceChick();
        if (source == null)
        {
            return;
        }

        Vector2 offset = Random.insideUnitCircle * data.SpawnScatterRadius;
        Vector3 spawnPos = source.transform.position + (Vector3)offset;
        SpawnManager.Instance.SpawnChick(spawnPos);
    }

    private Chick GetSourceChick()
    {
        for (int i = chicksInZone.Count - 1; i >= 0; i--)
        {
            Chick chick = chicksInZone[i];
            if (chick == null || !chick.gameObject.activeInHierarchy)
            {
                chicksInZone.RemoveAt(i);
                continue;
            }
            return chick;
        }
        return null;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Chick"))
        {
            return;
        }

        Chick chick = other.GetComponent<Chick>();
        if (chick != null && !chicksInZone.Contains(chick))
        {
            chicksInZone.Add(chick);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Chick"))
        {
            return;
        }

        Chick chick = other.GetComponent<Chick>();
        if (chick != null)
        {
            chicksInZone.Remove(chick);
        }
    }
}
