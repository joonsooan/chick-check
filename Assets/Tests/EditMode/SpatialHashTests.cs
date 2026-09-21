using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class SpatialHashTests
{
    [Test]
    public void Query_ReturnsItemWithinRadius()
    {
        SpatialHash<string> hash = new SpatialHash<string>(1f);
        hash.Insert("a", new Vector3(0f, 0f, 0f));

        List<string> results = new List<string>();
        hash.Query(new Vector3(0.2f, 0f, 0f), 1f, results);

        Assert.IsTrue(results.Contains("a"));
    }

    [Test]
    public void Query_DoesNotReturnItemOutsideCellRange()
    {
        SpatialHash<string> hash = new SpatialHash<string>(1f);
        hash.Insert("far", new Vector3(50f, 50f, 0f));

        List<string> results = new List<string>();
        hash.Query(new Vector3(0f, 0f, 0f), 1f, results);

        Assert.IsFalse(results.Contains("far"));
    }

    [Test]
    public void Query_AtCellBoundary_IncludesAdjacentCellItems()
    {
        SpatialHash<string> hash = new SpatialHash<string>(1f);
        hash.Insert("neighbor", new Vector3(1.1f, 0f, 0f));

        List<string> results = new List<string>();
        hash.Query(new Vector3(0.9f, 0f, 0f), 0.5f, results);

        Assert.IsTrue(results.Contains("neighbor"));
    }

    [Test]
    public void Move_UpdatesCell_OldCellNoLongerReturnsItem()
    {
        SpatialHash<string> hash = new SpatialHash<string>(1f);
        hash.Insert("item", new Vector3(0f, 0f, 0f));

        hash.Move("item", new Vector3(10f, 10f, 0f));

        List<string> oldResults = new List<string>();
        hash.Query(new Vector3(0f, 0f, 0f), 0.5f, oldResults);
        Assert.IsFalse(oldResults.Contains("item"));

        List<string> newResults = new List<string>();
        hash.Query(new Vector3(10f, 10f, 0f), 0.5f, newResults);
        Assert.IsTrue(newResults.Contains("item"));
    }

    [Test]
    public void Remove_DecrementsCountAndStopsQueryResults()
    {
        SpatialHash<string> hash = new SpatialHash<string>(1f);
        hash.Insert("item", new Vector3(0f, 0f, 0f));

        bool removed = hash.Remove("item");

        Assert.IsTrue(removed);
        Assert.AreEqual(0, hash.Count);

        List<string> results = new List<string>();
        hash.Query(new Vector3(0f, 0f, 0f), 0.5f, results);
        Assert.IsFalse(results.Contains("item"));
    }

    [Test]
    public void Move_UnregisteredItem_IsIgnored()
    {
        SpatialHash<string> hash = new SpatialHash<string>(1f);

        hash.Move("ghost", new Vector3(5f, 5f, 0f));

        Assert.AreEqual(0, hash.Count);
    }

    [Test]
    public void Constructor_ZeroCellSize_Throws()
    {
        Assert.Throws<System.ArgumentOutOfRangeException>(() => new SpatialHash<string>(0f));
    }

    [Test]
    public void QueryBox_ReturnsItemInsideBox()
    {
        SpatialHash<string> hash = new SpatialHash<string>(1f);
        hash.Insert("inside", new Vector3(0.3f, 0.1f, 0f));

        List<string> results = new List<string>();
        hash.QueryBox(new Vector3(0f, 0f, 0f), new Vector2(0.5f, 0.25f), results);

        Assert.IsTrue(results.Contains("inside"));
    }

    [Test]
    public void QueryBox_DoesNotReturnItemOutsideCellRange()
    {
        SpatialHash<string> hash = new SpatialHash<string>(1f);
        hash.Insert("far", new Vector3(50f, 50f, 0f));

        List<string> results = new List<string>();
        hash.QueryBox(new Vector3(0f, 0f, 0f), new Vector2(0.5f, 0.25f), results);

        Assert.IsFalse(results.Contains("far"));
    }

    [Test]
    public void QueryBox_AtCellBoundary_IncludesAdjacentCellItems()
    {
        SpatialHash<string> hash = new SpatialHash<string>(1f);
        hash.Insert("neighbor", new Vector3(1.1f, 0f, 0f));

        List<string> results = new List<string>();
        hash.QueryBox(new Vector3(0.9f, 0f, 0f), new Vector2(0.5f, 0.5f), results);

        Assert.IsTrue(results.Contains("neighbor"));
    }

    [Test]
    public void QueryBox_SpanningMultipleCells_DoesNotMissCandidates()
    {
        SpatialHash<string> hash = new SpatialHash<string>(1f);
        hash.Insert("topLeft", new Vector3(-2.4f, 2.4f, 0f));
        hash.Insert("topRight", new Vector3(2.4f, 2.4f, 0f));
        hash.Insert("bottomLeft", new Vector3(-2.4f, -2.4f, 0f));
        hash.Insert("bottomRight", new Vector3(2.4f, -2.4f, 0f));
        hash.Insert("center", new Vector3(0f, 0f, 0f));

        List<string> results = new List<string>();
        hash.QueryBox(new Vector3(0f, 0f, 0f), new Vector2(2.5f, 2.5f), results);

        Assert.IsTrue(results.Contains("topLeft"));
        Assert.IsTrue(results.Contains("topRight"));
        Assert.IsTrue(results.Contains("bottomLeft"));
        Assert.IsTrue(results.Contains("bottomRight"));
        Assert.IsTrue(results.Contains("center"));
    }
}
