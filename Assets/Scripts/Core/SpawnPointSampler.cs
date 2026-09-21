using UnityEngine;

public static class SpawnPointSampler
{
    public static Vector3 Sample(Vector3 center, float outerRadius, float innerRadius, float jitter, System.Random rng)
    {
        if (outerRadius <= 0f || innerRadius >= outerRadius)
        {
            return center;
        }

        float angle = (float)rng.NextDouble() * Mathf.PI * 2f;
        float radiusSqr = Mathf.Lerp(innerRadius * innerRadius, outerRadius * outerRadius, (float)rng.NextDouble());
        float radius = Mathf.Sqrt(radiusSqr);
        Vector3 point = center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);

        float jitterAngle = (float)rng.NextDouble() * Mathf.PI * 2f;
        float jitterRadius = Mathf.Sqrt((float)rng.NextDouble()) * jitter;
        point += new Vector3(Mathf.Cos(jitterAngle) * jitterRadius, Mathf.Sin(jitterAngle) * jitterRadius, 0f);

        return point;
    }
}
