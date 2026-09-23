using System.Collections.Generic;

public class ZoneDwellTracker<T> where T : class
{
    private readonly List<T> members = new List<T>();
    private readonly Dictionary<T, float> dwell = new Dictionary<T, float>();
    private readonly HashSet<T> nowInZoneSet = new HashSet<T>();

    public int Count => members.Count;

    public bool Contains(T item) => dwell.ContainsKey(item);

    public void Update(IReadOnlyList<T> nowInZone, float dt, float threshold, List<T> reached)
    {
        nowInZoneSet.Clear();
        for (int i = 0; i < nowInZone.Count; i++)
        {
            nowInZoneSet.Add(nowInZone[i]);
        }

        for (int i = 0; i < nowInZone.Count; i++)
        {
            T item = nowInZone[i];
            if (!dwell.ContainsKey(item))
            {
                members.Add(item);
                dwell[item] = 0f;
            }
        }

        for (int i = members.Count - 1; i >= 0; i--)
        {
            T item = members[i];
            if (!nowInZoneSet.Contains(item))
            {
                members.RemoveAt(i);
                dwell.Remove(item);
            }
        }

        for (int i = members.Count - 1; i >= 0; i--)
        {
            T item = members[i];
            float t = dwell[item] + dt;

            if (t >= threshold)
            {
                members.RemoveAt(i);
                dwell.Remove(item);
                reached?.Add(item);
            }
            else
            {
                dwell[item] = t;
            }
        }
    }

    public void Remove(T item)
    {
        dwell.Remove(item);
        members.Remove(item);
    }

    public void Clear()
    {
        members.Clear();
        dwell.Clear();
    }
}
