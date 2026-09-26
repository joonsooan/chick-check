using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class MergeFormationTests
{
    [Test]
    public void RingPositions_ReturnsExactCount()
    {
        List<Vector3> positions = MergeFormation.RingPositions(Vector3.zero, 25, 0.6f, 0.4f, 0.35f);

        Assert.AreEqual(25, positions.Count);
    }

    [Test]
    public void RingPositions_InnerRingCapacityMatchesArcRule()
    {
        float baseRadius = 0.6f;
        float minArcSpacing = 0.35f;
        int expectedInnerCapacity = Mathf.Max(1, Mathf.FloorToInt((2f * Mathf.PI * baseRadius) / minArcSpacing));

        List<Vector3> positions = MergeFormation.RingPositions(
            Vector3.zero, expectedInnerCapacity, baseRadius, 0.4f, minArcSpacing);

        Assert.AreEqual(expectedInnerCapacity, positions.Count);
        foreach (Vector3 position in positions)
        {
            Assert.AreEqual(baseRadius, position.magnitude, 0.001f);
        }
    }

    [Test]
    public void RingPositions_OverflowsToSecondRing()
    {
        float baseRadius = 0.6f;
        float minArcSpacing = 0.35f;
        int innerCapacity = Mathf.Max(1, Mathf.FloorToInt((2f * Mathf.PI * baseRadius) / minArcSpacing));

        List<Vector3> positions = MergeFormation.RingPositions(
            Vector3.zero, innerCapacity + 1, baseRadius, 0.4f, minArcSpacing);

        Assert.AreEqual(innerCapacity + 1, positions.Count);

        Vector3 outer = positions[positions.Count - 1];
        Assert.AreEqual(baseRadius + 0.4f, outer.magnitude, 0.001f);
    }

    [Test]
    public void RingPositions_AllPointsAtLeastBaseRadiusFromCenter()
    {
        Vector3 center = new Vector3(5f, -3f, 0f);
        List<Vector3> positions = MergeFormation.RingPositions(center, 60, 0.6f, 0.4f, 0.35f);

        foreach (Vector3 position in positions)
        {
            Assert.GreaterOrEqual((position - center).magnitude, 0.6f - 0.001f);
        }
    }

    [Test]
    public void NearestIndices_ReturnsClosestPointsInOrder()
    {
        List<Vector3> positions = new List<Vector3>
        {
            new Vector3(10f, 0f, 0f),
            new Vector3(1f, 0f, 0f),
            new Vector3(5f, 0f, 0f),
            new Vector3(0.5f, 0f, 0f),
        };

        int[] result = MergeFormation.NearestIndices(positions, Vector3.zero, 2);

        Assert.AreEqual(2, result.Length);
        Assert.AreEqual(3, result[0]);
        Assert.AreEqual(1, result[1]);
    }

    [Test]
    public void NearestIndices_CountExceedsPositions_ReturnsAllClamped()
    {
        List<Vector3> positions = new List<Vector3> { Vector3.zero, Vector3.right };

        int[] result = MergeFormation.NearestIndices(positions, Vector3.zero, 10);

        Assert.AreEqual(2, result.Length);
    }
    [Test]
    public void Centroid_AllPositions_ReturnsAverage()
    {
        List<Vector3> positions = new List<Vector3> { Vector3.zero, new Vector3(2f, 0f, 0f), new Vector3(0f, 4f, 0f) };

        Vector3 result = MergeFormation.Centroid(positions);

        Assert.AreEqual(2f / 3f, result.x, 0.0001f);
        Assert.AreEqual(4f / 3f, result.y, 0.0001f);
    }

    [Test]
    public void Centroid_WithIndices_AveragesOnlySelected()
    {
        List<Vector3> positions = new List<Vector3> { Vector3.zero, new Vector3(2f, 0f, 0f), new Vector3(100f, 100f, 0f) };

        Vector3 result = MergeFormation.Centroid(positions, new[] { 0, 1 });

        Assert.AreEqual(1f, result.x, 0.0001f);
        Assert.AreEqual(0f, result.y, 0.0001f);
    }

    [Test]
    public void Centroid_Empty_ReturnsZero()
    {
        Assert.AreEqual(Vector3.zero, MergeFormation.Centroid(new List<Vector3>()));
    }
    [Test]
    public void OffsetFrom_MovesExactDistanceTowardsTarget()
    {
        Vector3 result = MergeFormation.OffsetFrom(Vector3.zero, new Vector3(10f, 0f, 0f), 2f);

        Assert.AreEqual(2f, result.x, 0.0001f);
        Assert.AreEqual(0f, result.y, 0.0001f);
    }

    [Test]
    public void OffsetFrom_ZeroDistance_ReturnsOrigin()
    {
        Vector3 origin = new Vector3(1f, 2f, 0f);

        Assert.AreEqual(origin, MergeFormation.OffsetFrom(origin, Vector3.zero, 0f));
    }

    [Test]
    public void OffsetFrom_TargetAtOrigin_FallsBackToUp()
    {
        Vector3 result = MergeFormation.OffsetFrom(Vector3.zero, Vector3.zero, 3f);

        Assert.AreEqual(0f, result.x, 0.0001f);
        Assert.AreEqual(3f, result.y, 0.0001f);
    }
}
