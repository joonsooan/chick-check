using System.Collections.Generic;
using UnityEngine;

public static class MergeFormation
{
    public static List<Vector3> RingPositions(Vector3 center, int count, float baseRadius, float spacing, float minArcSpacing)
    {
        List<Vector3> positions = new List<Vector3>(Mathf.Max(0, count));
        if (count <= 0)
        {
            return positions;
        }

        float radius = baseRadius;
        int remaining = count;
        while (remaining > 0)
        {
            int capacity = RingCapacity(radius, minArcSpacing);
            int take = Mathf.Min(capacity, remaining);

            for (int i = 0; i < take; i++)
            {
                float angle = (Mathf.PI * 2f * i) / take;
                Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius;
                positions.Add(center + offset);
            }

            remaining -= take;
            radius += spacing;
        }

        return positions;
    }

    private static int RingCapacity(float radius, float minArcSpacing)
    {
        if (radius <= 0f)
        {
            return 1;
        }

        int capacity = Mathf.FloorToInt((2f * Mathf.PI * radius) / minArcSpacing);
        return Mathf.Max(1, capacity);
    }

    public static Vector3 Centroid(IReadOnlyList<Vector3> positions, IReadOnlyList<int> indices = null)
    {
        int count = indices != null ? indices.Count : positions.Count;
        if (count == 0)
        {
            return Vector3.zero;
        }

        Vector3 sum = Vector3.zero;
        for (int i = 0; i < count; i++)
        {
            sum += positions[indices != null ? indices[i] : i];
        }

        return sum / count;
    }

    public static Vector3 OffsetFrom(Vector3 origin, Vector3 towards, float distance)
    {
        if (distance <= 0f)
        {
            return origin;
        }

        Vector3 direction = towards - origin;
        direction.z = 0f;
        if (direction.sqrMagnitude <= 0.0001f)
        {
            direction = Vector3.up;
        }

        return origin + direction.normalized * distance;
    }

    public static int[] NearestIndices(IReadOnlyList<Vector3> positions, Vector3 center, int count)
    {
        int take = Mathf.Clamp(count, 0, positions.Count);
        int[] indices = new int[positions.Count];
        for (int i = 0; i < indices.Length; i++)
        {
            indices[i] = i;
        }

        float[] distancesSqr = new float[positions.Count];
        for (int i = 0; i < positions.Count; i++)
        {
            distancesSqr[i] = (positions[i] - center).sqrMagnitude;
        }

        System.Array.Sort(indices, (a, b) => distancesSqr[a].CompareTo(distancesSqr[b]));

        int[] result = new int[take];
        System.Array.Copy(indices, result, take);
        return result;
    }
}
