using System.Collections.Generic;

public class ZoneDwellTracker<T> where T : class
{
    private readonly List<T> members = new List<T>();
    private readonly Dictionary<T, float> dwellTimeByItem = new Dictionary<T, float>();
    private readonly HashSet<T> nowInZoneSet = new HashSet<T>();

    public int Count => members.Count;

    public bool Contains(T item) => dwellTimeByItem.ContainsKey(item);

    public void Tick(IReadOnlyList<T> nowInZone, float dt, float dwellThreshold, List<T> completed)
    {
        nowInZoneSet.Clear();
        for (int i = 0; i < nowInZone.Count; i++)
        {
            nowInZoneSet.Add(nowInZone[i]);
        }

        for (int i = 0; i < nowInZone.Count; i++)
        {
            T item = nowInZone[i];
            if (!dwellTimeByItem.ContainsKey(item))
            {
                members.Add(item);
                dwellTimeByItem[item] = 0f;
            }
        }

        for (int i = members.Count - 1; i >= 0; i--)
        {
            T item = members[i];
            if (!nowInZoneSet.Contains(item))
            {
                members.RemoveAt(i);
                dwellTimeByItem.Remove(item);
            }
        }

        for (int i = members.Count - 1; i >= 0; i--)
        {
            T item = members[i];
            float t = dwellTimeByItem[item] + dt;

            if (t >= dwellThreshold)
            {
                members.RemoveAt(i);
                dwellTimeByItem.Remove(item);
                completed?.Add(item);
            }
            else
            {
                dwellTimeByItem[item] = t;
            }
        }
    }

    public void Remove(T item)
    {
        dwellTimeByItem.Remove(item);
        members.Remove(item);
    }

    public void Clear()
    {
        members.Clear();
        dwellTimeByItem.Clear();
    }
}
