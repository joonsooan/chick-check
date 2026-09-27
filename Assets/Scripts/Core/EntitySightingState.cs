using System.Collections.Generic;

public class EntitySightingState
{
    private readonly HashSet<EconomyEntityData> seen = new HashSet<EconomyEntityData>();

    public void Record(EconomyEntityData data)
    {
        if (data == null)
        {
            return;
        }

        seen.Add(data);
    }

    public bool HasSeen(EconomyEntityData data)
    {
        return data != null && seen.Contains(data);
    }

    public bool HasSeenAny(IReadOnlyList<EconomyEntityData> candidates)
    {
        if (candidates == null || candidates.Count == 0)
        {
            return true;
        }

        for (int i = 0; i < candidates.Count; i++)
        {
            if (HasSeen(candidates[i]))
            {
                return true;
            }
        }

        return false;
    }
}
