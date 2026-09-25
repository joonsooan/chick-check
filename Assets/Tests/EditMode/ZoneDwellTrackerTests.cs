using System.Collections.Generic;
using NUnit.Framework;

[TestFixture]
public class ZoneDwellTrackerTests
{
    private class Dummy
    {
    }

    [Test]
    public void Update_AccumulatesDwellAcrossFrames()
    {
        ZoneDwellTracker<Dummy> tracker = new ZoneDwellTracker<Dummy>();
        Dummy item = new Dummy();
        List<Dummy> nowInZone = new List<Dummy> { item };
        List<Dummy> reached = new List<Dummy>();

        tracker.Tick(nowInZone, 0.4f, 1f, reached);
        tracker.Tick(nowInZone, 0.4f, 1f, reached);

        Assert.AreEqual(0, reached.Count);
        Assert.IsTrue(tracker.Contains(item));
    }

    [Test]
    public void Update_LeavingZoneResetsDwell()
    {
        ZoneDwellTracker<Dummy> tracker = new ZoneDwellTracker<Dummy>();
        Dummy item = new Dummy();
        List<Dummy> nowInZone = new List<Dummy> { item };
        List<Dummy> reached = new List<Dummy>();

        tracker.Tick(nowInZone, 0.9f, 1f, reached);
        tracker.Tick(new List<Dummy>(), 0.9f, 1f, reached);
        tracker.Tick(nowInZone, 0.9f, 1f, reached);

        Assert.AreEqual(0, reached.Count);
        Assert.IsTrue(tracker.Contains(item));
    }

    [Test]
    public void Update_ReachingThreshold_ReportsExactlyOnceAndRemoves()
    {
        ZoneDwellTracker<Dummy> tracker = new ZoneDwellTracker<Dummy>();
        Dummy item = new Dummy();
        List<Dummy> nowInZone = new List<Dummy> { item };
        List<Dummy> reached = new List<Dummy>();

        tracker.Tick(nowInZone, 1.5f, 1f, reached);

        Assert.AreEqual(1, reached.Count);
        Assert.AreSame(item, reached[0]);
        Assert.IsFalse(tracker.Contains(item));

        reached.Clear();
        tracker.Tick(nowInZone, 1.5f, 1f, reached);

        Assert.AreEqual(1, reached.Count);
    }

    [Test]
    public void Clear_RemovesAllMembers()
    {
        ZoneDwellTracker<Dummy> tracker = new ZoneDwellTracker<Dummy>();
        Dummy item = new Dummy();
        List<Dummy> nowInZone = new List<Dummy> { item };
        List<Dummy> reached = new List<Dummy>();

        tracker.Tick(nowInZone, 0.1f, 1f, reached);
        tracker.Clear();

        Assert.AreEqual(0, tracker.Count);
        Assert.IsFalse(tracker.Contains(item));
    }
}
