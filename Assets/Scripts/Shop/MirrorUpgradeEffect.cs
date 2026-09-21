using System;
using UnityEngine;

[Serializable]
public class MirrorUpgradeEffect : ShopEffect
{
    [SerializeField] private GameObject replacementPrefab;

    public GameObject ReplacementPrefab => replacementPrefab;

    public override void Apply(UpgradeState state)
    {
        state.RequestMirrorUpgrade(replacementPrefab);
    }
}
