using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class MirrorExitMotionTests
{
    [Test]
    public void Tick_ForcedPhase_MovesTowardTargetByForcedDistance()
    {
        MirrorExitMotion motion = new MirrorExitMotion((min, max) => 0f);
        Vector3 position = Vector3.zero;

        motion.Begin(position, Vector2.right, moveSpeed: 1f, forcedDistance: 2f, randomAngleRange: 30f, randomDuration: 1f);

        motion.Tick(0.5f, ref position);

        Assert.AreEqual(new Vector3(0.5f, 0f, 0f), position);
        Assert.IsTrue(motion.IsActive);
    }

    [Test]
    public void Tick_ForcedTargetReached_TransitionsToRandomAndRotatesDirection()
    {
        MirrorExitMotion motion = new MirrorExitMotion((min, max) => max);
        Vector3 position = Vector3.zero;

        motion.Begin(position, Vector2.right, moveSpeed: 1f, forcedDistance: 1f, randomAngleRange: 90f, randomDuration: 1f);

        bool directionChanged = motion.Tick(1f, ref position);

        Assert.IsTrue(directionChanged);
        Assert.IsTrue(motion.IsActive);
        Assert.AreEqual(0f, motion.Direction.x, 0.0001f);
        Assert.AreEqual(1f, motion.Direction.y, 0.0001f);
    }

    [Test]
    public void Tick_RandomDurationElapsed_Deactivates()
    {
        MirrorExitMotion motion = new MirrorExitMotion((min, max) => 0f);
        Vector3 position = Vector3.zero;

        motion.Begin(position, Vector2.right, moveSpeed: 1f, forcedDistance: 1f, randomAngleRange: 30f, randomDuration: 0.5f);

        motion.Tick(1f, ref position);
        Assert.IsTrue(motion.IsActive);

        motion.Tick(0.6f, ref position);

        Assert.IsFalse(motion.IsActive);
    }
}
