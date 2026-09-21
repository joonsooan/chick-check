using UnityEngine;

[CreateAssetMenu(fileName = "EconomyConfigData", menuName = "Chick Colony/Economy Config")]
public class EconomyConfigData : ScriptableObject
{
    [SerializeField, Min(0)] private int startingGold = 10;

    public int StartingGold => startingGold;
}
