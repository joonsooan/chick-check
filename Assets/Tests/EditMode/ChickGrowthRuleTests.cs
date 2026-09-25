using NUnit.Framework;

[TestFixture]
public class ChickGrowthRuleTests
{
    [Test]
    public void LastNormalChick_IsLastOfKind()
    {
        Assert.IsFalse(ChickGrowthRule.IsNotLastOfKind(isMutant: false, chickCount: 1, mutantChickCount: 0));
    }

    [Test]
    public void NormalChick_WithOthers_IsNotLastOfKind()
    {
        Assert.IsTrue(ChickGrowthRule.IsNotLastOfKind(isMutant: false, chickCount: 2, mutantChickCount: 0));
    }

    [Test]
    public void NormalChickRule_IgnoresMutantCount()
    {
        Assert.IsTrue(ChickGrowthRule.IsNotLastOfKind(isMutant: false, chickCount: 2, mutantChickCount: 1));
        Assert.IsFalse(ChickGrowthRule.IsNotLastOfKind(isMutant: false, chickCount: 1, mutantChickCount: 5));
    }

    [Test]
    public void LastMutant_IsLastOfKind_EvenWithManyNormalChicks()
    {
        Assert.IsFalse(ChickGrowthRule.IsNotLastOfKind(isMutant: true, chickCount: 10, mutantChickCount: 1));
    }

    [Test]
    public void Mutant_WithAnotherMutant_IsNotLastOfKind()
    {
        Assert.IsTrue(ChickGrowthRule.IsNotLastOfKind(isMutant: true, chickCount: 2, mutantChickCount: 2));
    }
}
