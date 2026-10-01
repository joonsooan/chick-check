using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class EntityWalkerTests
{
    private GameObject entityObject;
    private ChickData data;

    [TearDown]
    public void TearDown()
    {
        if (entityObject != null)
        {
            UnityEngine.Object.DestroyImmediate(entityObject);
        }

        if (data != null)
        {
            UnityEngine.Object.DestroyImmediate(data);
        }
    }

    private static void SetEconomyField(EconomyEntityData target, string fieldName, object value)
    {
        FieldInfo field = typeof(EconomyEntityData).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        field.SetValue(target, value);
    }

    private static void SetWalkerField(EntityWalker walker, string fieldName, object value)
    {
        FieldInfo field = typeof(EntityWalker).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        field.SetValue(walker, value);
    }

    private static object GetWalkerField(EntityWalker walker, string fieldName)
    {
        FieldInfo field = typeof(EntityWalker).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        return field.GetValue(walker);
    }

    private static object WalkState(string name)
    {
        Type stateType = typeof(EntityWalker).GetNestedType("WalkState", BindingFlags.NonPublic);
        return Enum.Parse(stateType, name);
    }

    private EntityWalker CreateWalker(out Transform transform, Func<Vector3, bool> isWalkable = null)
    {
        entityObject = new GameObject("Entity");
        SpriteRenderer spriteRenderer = entityObject.AddComponent<SpriteRenderer>();
        transform = entityObject.transform;

        data = ScriptableObject.CreateInstance<ChickData>();
        SetEconomyField(data, "moveSpeed", 1f);
        SetEconomyField(data, "walkIntervalRange", new Vector2(0f, 0f));
        SetEconomyField(data, "walkDistanceRange", new Vector2(1f, 1f));
        SetEconomyField(data, "walkTargetAttempts", 10);

        return new EntityWalker(transform, spriteRenderer, null, isWalkable);
    }

    [Test]
    public void Tick_WaitTimerElapsed_TransitionsToMoving()
    {
        EntityWalker walker = CreateWalker(out Transform transform);

        walker.Reset(data);
        walker.Tick(data, 0.01f);

        Assert.AreEqual(WalkState("Moving"), GetWalkerField(walker, "state"));
    }

    [Test]
    public void Tick_TargetReached_ReturnsToWaiting()
    {
        EntityWalker walker = CreateWalker(out Transform transform);

        walker.Reset(data);
        SetWalkerField(walker, "state", WalkState("Moving"));
        SetWalkerField(walker, "target", transform.position);

        walker.Tick(data, 0.01f);

        Assert.AreEqual(WalkState("Waiting"), GetWalkerField(walker, "state"));
    }

    [Test]
    public void Tick_NextCellBlocked_ResetsToWaiting()
    {
        Vector3 targetPosition = new Vector3(1f, 0f, 0f);
        EntityWalker walker = CreateWalker(out Transform transform, position => position.x < 0.5f);

        walker.Reset(data);
        SetWalkerField(walker, "state", WalkState("Moving"));
        SetWalkerField(walker, "target", targetPosition);

        walker.Tick(data, 10f);

        Assert.AreEqual(WalkState("Waiting"), GetWalkerField(walker, "state"));
    }

    [Test]
    public void WalkTo_StartingInsideBlockedCell_MovesOut()
    {
        EntityWalker walker = CreateWalker(out Transform transform, position => position.x > 0.5f);
        walker.Reset(data);

        walker.WalkTo(new Vector3(1f, 0f, 0f), data);
        walker.Tick(data, 0.1f);

        Assert.AreEqual(WalkState("Moving"), GetWalkerField(walker, "state"));
        Assert.Greater(transform.position.x, 0f);
    }

    [Test]
    public void WalkTo_EnteringBlockedCellFromWalkable_ResetsToWaiting()
    {
        EntityWalker walker = CreateWalker(out Transform transform, position => position.x < 0.5f);
        walker.Reset(data);

        walker.WalkTo(new Vector3(1f, 0f, 0f), data);
        walker.Tick(data, 10f);

        Assert.AreEqual(WalkState("Waiting"), GetWalkerField(walker, "state"));
    }

    [Test]
    public void WalkTo_IgnoreObstacles_EntersBlockedCell()
    {
        EntityWalker walker = CreateWalker(out Transform transform, position => position.x < 0.5f);
        walker.Reset(data);

        walker.WalkTo(new Vector3(1f, 0f, 0f), data, ignoreObstacles: true);
        walker.Tick(data, 0.1f);

        Assert.AreEqual(WalkState("Moving"), GetWalkerField(walker, "state"));
        Assert.Greater(transform.position.x, 0f);
    }

    [Test]
    public void WalkTo_TargetReached_ReturnsToWaiting()
    {
        EntityWalker walker = CreateWalker(out Transform transform);
        walker.Reset(data);

        walker.WalkTo(new Vector3(1f, 0f, 0f), data);
        walker.Tick(data, 10f);

        Assert.AreEqual(WalkState("Waiting"), GetWalkerField(walker, "state"));
        Assert.AreEqual(1f, transform.position.x, 0.0001f);
    }
}
