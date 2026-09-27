using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class EntitySightingStateTests
{
    private ChickData dataA;
    private ChickData dataB;

    [SetUp]
    public void SetUp()
    {
        dataA = ScriptableObject.CreateInstance<ChickData>();
        dataB = ScriptableObject.CreateInstance<ChickData>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(dataA);
        Object.DestroyImmediate(dataB);
    }

    [Test]
    public void HasSeenAny_EmptyList_ReturnsTrue()
    {
        EntitySightingState state = new EntitySightingState();

        Assert.IsTrue(state.HasSeenAny(new List<EconomyEntityData>()));
    }

    [Test]
    public void HasSeenAny_NullList_ReturnsTrue()
    {
        EntitySightingState state = new EntitySightingState();

        Assert.IsTrue(state.HasSeenAny(null));
    }

    [Test]
    public void HasSeen_Unrecorded_ReturnsFalse()
    {
        EntitySightingState state = new EntitySightingState();

        Assert.IsFalse(state.HasSeen(dataA));
    }

    [Test]
    public void HasSeen_AfterRecord_ReturnsTrue()
    {
        EntitySightingState state = new EntitySightingState();

        state.Record(dataA);

        Assert.IsTrue(state.HasSeen(dataA));
    }

    [Test]
    public void HasSeenAny_OneOfSeveralRecorded_ReturnsTrue()
    {
        EntitySightingState state = new EntitySightingState();
        state.Record(dataB);

        bool result = state.HasSeenAny(new List<EconomyEntityData> { dataA, dataB });

        Assert.IsTrue(result);
    }

    [Test]
    public void HasSeenAny_NoneRecorded_ReturnsFalse()
    {
        EntitySightingState state = new EntitySightingState();

        bool result = state.HasSeenAny(new List<EconomyEntityData> { dataA, dataB });

        Assert.IsFalse(result);
    }
}
