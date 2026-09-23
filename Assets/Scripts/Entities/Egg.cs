using System;
using DG.Tweening;
using UnityEngine;

public class Egg : PlaceableEntity
{
    [SerializeField] private EggData data;

    private bool isHatching;

    public override EconomyEntityData EconomyData => data;

    protected override ShopStatType GrowthChanceStat => ShopStatType.EggGrowthChance;

    public override ShopStatType SellPriceStat => ShopStatType.SellPriceEgg;

    public override bool CanBeCarried => base.CanBeCarried && !isHatching;

    protected override void OnEnable()
    {
        base.OnEnable();
        isHatching = false;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        isHatching = false;
    }

    protected override void GrowToNextStage(GameObject nextStagePrefab)
    {
        if (nextStagePrefab == null)
        {
            return;
        }

        PlayHatchSquash(() =>
        {
            SpawnNextStage(nextStagePrefab);
            DespawnSelf();
        });
    }

    private void PlayHatchSquash(Action onComplete)
    {
        if (data == null || (data.StretchDuration <= 0f && data.SquashDuration <= 0f))
        {
            onComplete();
            return;
        }

        isHatching = true;

        transform.DOKill();
        transform.localScale = DefaultScale;
        Vector3 baseScale = DefaultScale;

        Sequence sequence = DOTween.Sequence();
        sequence.Append(transform
            .DOScale(new Vector3(baseScale.x * data.StretchScale.x, baseScale.y * data.StretchScale.y, baseScale.z),
                data.StretchDuration)
            .SetEase(Ease.OutQuad));
        sequence.Append(transform
            .DOScale(new Vector3(baseScale.x * data.SquashScale.x, baseScale.y * data.SquashScale.y, baseScale.z),
                data.SquashDuration)
            .SetEase(Ease.InQuad));
        sequence.OnComplete(() => onComplete());
    }
}
