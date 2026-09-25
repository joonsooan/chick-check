using System.Reflection;
using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class ShopEffectTests
{
    [Test]
    public void StatModifierEffect_Add_AppliesAdditiveBonus()
    {
        UpgradeState state = new UpgradeState();
        ShopItemData item = ShopItemData.CreateForTest(effect: BuildStatModifier(ShopStatType.ChickGrowthChance, ShopModifierOp.Add, 0.05f));

        item.Effect.Apply(state);

        Assert.AreEqual(0.05f, state.Modifiers.GetAdditive(ShopStatType.ChickGrowthChance), 0.0001f);
        Object.DestroyImmediate(item);
    }

    [Test]
    public void StatModifierEffect_Add_AppliesToDuckGrowthChance()
    {
        UpgradeState state = new UpgradeState();
        ShopItemData item = ShopItemData.CreateForTest(effect: BuildStatModifier(ShopStatType.DuckGrowthChance, ShopModifierOp.Add, 0.15f));

        item.Effect.Apply(state);

        Assert.AreEqual(0.15f, state.Modifiers.GetAdditive(ShopStatType.DuckGrowthChance), 0.0001f);
        Object.DestroyImmediate(item);
    }

    [Test]
    public void StatModifierEffect_Multiply_AppliesMultiplier()
    {
        UpgradeState state = new UpgradeState();
        ShopItemData item = ShopItemData.CreateForTest(effect: BuildStatModifier(ShopStatType.PlayerMoveSpeed, ShopModifierOp.Multiply, 1.1f));

        item.Effect.Apply(state);

        Assert.AreEqual(1.1f, state.Modifiers.GetMultiplier(ShopStatType.PlayerMoveSpeed), 0.0001f);
        Object.DestroyImmediate(item);
    }

    [Test]
    public void EnableMutationEffect_IncrementsMutantSpawnCount()
    {
        UpgradeState state = new UpgradeState();
        new EnableMutationEffect().Apply(state);
        new EnableMutationEffect().Apply(state);

        Assert.AreEqual(2, state.Get<MutationState>().MutantSpawnCountPerRound);
    }

    [Test]
    public void EnableContagionEffect_IsAvailable_RequiresMutantSpawn()
    {
        UpgradeState state = new UpgradeState();
        EnableContagionEffect effect = new EnableContagionEffect();

        Assert.IsFalse(effect.IsAvailable(state));

        state.Get<MutationState>().MutantSpawnCountPerRound = 1;
        Assert.IsTrue(effect.IsAvailable(state));

        effect.Apply(state);
        Assert.IsTrue(state.Get<ContagionState>().Enabled);
    }

    [Test]
    public void EnableSprintEffect_EnablesSprint()
    {
        UpgradeState state = new UpgradeState();
        new EnableSprintEffect().Apply(state);

        Assert.IsTrue(state.Get<SprintState>().Enabled);
    }

    [Test]
    public void GoldMineEffect_SecondPurchase_ScalesGoldAmount()
    {
        UpgradeState state = new UpgradeState();
        GoldMineEffect first = BuildGoldMine(5f, 0.5f, 1, 10);
        GoldMineEffect second = BuildGoldMine(5f, 0.5f, 1, 10);

        first.Apply(state);
        Assert.AreEqual(1, state.Get<GoldMineState>().GoldAmount);

        second.Apply(state);
        Assert.AreEqual(11, state.Get<GoldMineState>().GoldAmount);
    }

    [Test]
    public void MultiLayEffect_AccumulatesBonusCount()
    {
        UpgradeState state = new UpgradeState();
        BuildMultiLay(0.5f, 1).Apply(state);
        BuildMultiLay(0.2f, 2).Apply(state);

        Assert.AreEqual(0.2f, state.Get<MultiLayState>().Chance, 0.0001f);
        Assert.AreEqual(3, state.Get<MultiLayState>().BonusEggCount);
    }

    [Test]
    public void MirrorUpgradeEffect_SetsPendingPrefabUntilConsumed()
    {
        UpgradeState state = new UpgradeState();
        GameObject prefab = new GameObject("MirrorPrefab");
        MirrorUpgradeEffect effect = new MirrorUpgradeEffect();
        SetPrivateField(effect, "replacementPrefab", prefab);

        effect.Apply(state);

        Assert.AreEqual(prefab, state.PendingMirrorPrefab);
        Assert.AreEqual(prefab, state.ConsumePendingMirrorPrefab());
        Assert.IsNull(state.PendingMirrorPrefab);
        Object.DestroyImmediate(prefab);
    }

    [Test]
    public void Get_ReturnsSameSlotForSameType()
    {
        UpgradeState state = new UpgradeState();

        Assert.AreSame(state.Get<GoldMineState>(), state.Get<GoldMineState>());
    }

    [Test]
    public void Reset_ClearsAllStateIncludingModifiers()
    {
        UpgradeState state = new UpgradeState();
        state.Modifiers.AddAdditive(ShopStatType.ChickGrowthChance, 0.5f);
        state.Modifiers.AddMultiplier(ShopStatType.PlayerMoveSpeed, 1.5f);
        new EnableMutationEffect().Apply(state);
        new EnableContagionEffect().Apply(state);
        new EnableSprintEffect().Apply(state);
        BuildGoldMine(5f, 0.5f, 1, 1).Apply(state);
        BuildMultiLay(0.5f, 1).Apply(state);

        state.Reset();

        Assert.AreEqual(0f, state.Modifiers.GetAdditive(ShopStatType.ChickGrowthChance));
        Assert.AreEqual(1f, state.Modifiers.GetMultiplier(ShopStatType.PlayerMoveSpeed));
        Assert.AreEqual(0, state.Get<MutationState>().MutantSpawnCountPerRound);
        Assert.IsFalse(state.Get<ContagionState>().Enabled);
        Assert.IsFalse(state.Get<SprintState>().Enabled);
        Assert.AreEqual(0, state.Get<GoldMineState>().Level);
        Assert.AreEqual(0, state.Get<GoldMineState>().GoldAmount);
        Assert.AreEqual(0f, state.Get<MultiLayState>().Chance);
        Assert.AreEqual(0, state.Get<MultiLayState>().BonusEggCount);
    }

    private static StatModifierEffect BuildStatModifier(ShopStatType stat, ShopModifierOp op, float value)
    {
        StatModifierEffect effect = new StatModifierEffect();
        SetPrivateField(effect, "targetStat", stat);
        SetPrivateField(effect, "op", op);
        SetPrivateField(effect, "value", value);
        return effect;
    }

    private static GoldMineEffect BuildGoldMine(float interval, float chance, int baseGold, int goldPerPurchase)
    {
        GoldMineEffect effect = new GoldMineEffect();
        SetPrivateField(effect, "interval", interval);
        SetPrivateField(effect, "chance", chance);
        SetPrivateField(effect, "baseGold", baseGold);
        SetPrivateField(effect, "goldPerPurchase", goldPerPurchase);
        return effect;
    }

    private static MultiLayEffect BuildMultiLay(float chance, int countPerPurchase)
    {
        MultiLayEffect effect = new MultiLayEffect();
        SetPrivateField(effect, "chance", chance);
        SetPrivateField(effect, "countPerPurchase", countPerPurchase);
        return effect;
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        FieldInfo field = target.GetType().GetField(
            fieldName,
            BindingFlags.Instance | BindingFlags.NonPublic);
        field?.SetValue(target, value);
    }
}
