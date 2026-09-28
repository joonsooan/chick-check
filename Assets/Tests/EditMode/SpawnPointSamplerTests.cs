using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class SpawnPointSamplerTests
{
    [Test]
    public void OuterRadiusZero_ReturnsCenter()
    {
        Vector3 center = new Vector3(1f, 2f, 3f);
        Vector3 result = SpawnPointSampler.Sample(center, 0f, 0f, 0f, new System.Random(1));
        Assert.AreEqual(center, result);
    }

    [Test]
    public void InnerRadiusNotSmallerThanOuter_ReturnsCenter()
    {
        Vector3 center = new Vector3(1f, 2f, 3f);
        Assert.AreEqual(center, SpawnPointSampler.Sample(center, 5f, 5f, 0f, new System.Random(1)));
        Assert.AreEqual(center, SpawnPointSampler.Sample(center, 5f, 6f, 0f, new System.Random(1)));
    }

    [Test]
    public void Sample_WithoutJitter_StaysWithinRing()
    {
        Vector3 center = new Vector3(2f, -1f, 0f);
        float inner = 1f;
        float outer = 3f;
        System.Random rng = new System.Random(12345);

        for (int i = 0; i < 200; i++)
        {
            Vector3 result = SpawnPointSampler.Sample(center, outer, inner, 0f, rng);
            float distance = Vector3.Distance(center, result);
            Assert.GreaterOrEqual(distance, inner - 1e-4f);
            Assert.LessOrEqual(distance, outer + 1e-4f);
        }
    }

    [Test]
    public void Sample_WithJitter_StaysWithinOuterPlusJitter()
    {
        Vector3 center = new Vector3(0f, 0f, 0f);
        float inner = 0.5f;
        float outer = 2f;
        float jitter = 0.75f;
        System.Random rng = new System.Random(54321);

        for (int i = 0; i < 200; i++)
        {
            Vector3 result = SpawnPointSampler.Sample(center, outer, inner, jitter, rng);
            float distance = Vector3.Distance(center, result);
            Assert.LessOrEqual(distance, outer + jitter + 1e-4f);
        }
    }

    [Test]
    public void Sample_KeepsZ()
    {
        Vector3 center = new Vector3(1f, 1f, 7.5f);
        Vector3 result = SpawnPointSampler.Sample(center, 2f, 0.5f, 0.3f, new System.Random(99));
        Assert.AreEqual(center.z, result.z);
    }

    [Test]
    public void SameSeed_ProducesSameSequence()
    {
        Vector3 center = new Vector3(0f, 0f, 0f);
        Vector3 first = SpawnPointSampler.Sample(center, 2f, 0.5f, 0.3f, new System.Random(12345));
        Vector3 second = SpawnPointSampler.Sample(center, 2f, 0.5f, 0.3f, new System.Random(12345));
        Assert.AreEqual(first, second);
    }
}
