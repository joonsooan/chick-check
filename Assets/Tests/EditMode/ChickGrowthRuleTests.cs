using NUnit.Framework;

[TestFixture]
public class ChickGrowthRuleTests
{
    [Test]
    public void LastNormalChick_CannotGrow()
    {
        Assert.IsFalse(ChickGrowthRule.CanGrow(isMutant: false, chickCount: 1, mutantChickCount: 0));
    }

    [Test]
    public void NormalChick_WithOthers_CanGrow()
    {
        Assert.IsTrue(ChickGrowthRule.CanGrow(isMutant: false, chickCount: 2, mutantChickCount: 0));
    }

    [Test]
    public void NormalChickRule_IgnoresMutantCount()
    {
        Assert.IsTrue(ChickGrowthRule.CanGrow(isMutant: false, chickCount: 2, mutantChickCount: 1));
        Assert.IsFalse(ChickGrowthRule.CanGrow(isMutant: false, chickCount: 1, mutantChickCount: 5));
    }

    [Test]
    public void LastMutant_CannotGrow_EvenWithManyNormalChicks()
    {
        Assert.IsFalse(ChickGrowthRule.CanGrow(isMutant: true, chickCount: 10, mutantChickCount: 1));
    }

    [Test]
    public void Mutant_WithAnotherMutant_CanGrow()
    {
        Assert.IsTrue(ChickGrowthRule.CanGrow(isMutant: true, chickCount: 2, mutantChickCount: 2));
    }
}
