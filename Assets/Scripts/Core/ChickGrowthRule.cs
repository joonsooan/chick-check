public static class ChickGrowthRule
{
    public static bool CanGrow(bool isMutant, int chickCount, int mutantChickCount)
    {
        return isMutant ? mutantChickCount > 1 : chickCount > 1;
    }
}
