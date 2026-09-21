using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class WalkableGridTests
{
    [Test]
    public void NullWalkableSet_TreatsEverythingAsWalkable()
    {
        WalkableGrid grid = new WalkableGrid(null);

        Assert.IsTrue(grid.IsWalkable(new Vector3Int(100, 100, 0)));
    }

    [Test]
    public void CellOutsideWalkableSet_IsNotWithinBounds()
    {
        HashSet<Vector3Int> cells = new HashSet<Vector3Int> { new Vector3Int(0, 0, 0) };
        WalkableGrid grid = new WalkableGrid(cells);

        Assert.IsFalse(grid.IsWithinBounds(new Vector3Int(1, 1, 0)));
        Assert.IsFalse(grid.IsWalkable(new Vector3Int(1, 1, 0)));
    }

    [Test]
    public void SetBlocked_TogglesWalkability()
    {
        HashSet<Vector3Int> cells = new HashSet<Vector3Int> { new Vector3Int(0, 0, 0) };
        WalkableGrid grid = new WalkableGrid(cells);
        Vector3Int cell = new Vector3Int(0, 0, 0);

        Assert.IsTrue(grid.IsWalkable(cell));

        grid.SetBlocked(cell, true);
        Assert.IsFalse(grid.IsWalkable(cell));

        grid.SetBlocked(cell, false);
        Assert.IsTrue(grid.IsWalkable(cell));
    }

    [Test]
    public void TryFindWalkable_ReturnsFirstWalkableCandidate()
    {
        HashSet<Vector3Int> cells = new HashSet<Vector3Int> { new Vector3Int(2, 0, 0) };
        WalkableGrid grid = new WalkableGrid(cells);

        Vector3[] candidates =
        {
            new Vector3(0f, 0f, 0f),
            new Vector3(2f, 0f, 0f),
        };

        bool found = grid.TryFindWalkable(
            Vector3.zero,
            candidates.Length,
            attempt => candidates[attempt],
            v => new Vector3Int(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y), 0),
            out Vector3 result);

        Assert.IsTrue(found);
        Assert.AreEqual(candidates[1], result);
    }

    [Test]
    public void TryFindWalkable_AllFail_ReturnsFallback()
    {
        WalkableGrid grid = new WalkableGrid(new HashSet<Vector3Int>());

        bool found = grid.TryFindWalkable(
            Vector3.one,
            3,
            attempt => Vector3.zero,
            v => Vector3Int.zero,
            out Vector3 result);

        Assert.IsFalse(found);
        Assert.AreEqual(Vector3.one, result);
    }
}
