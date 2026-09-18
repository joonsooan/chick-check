using System;
using System.Collections.Generic;
using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance { get; private set; }

    [SerializeField] private int startingGold = 10;

    public int Gold { get; private set; }
    public event Action<int> OnGoldChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        Gold = startingGold;
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnRoundEnded += HandleRoundEnded;
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnRoundEnded -= HandleRoundEnded;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void HandleRoundEnded(int roundNumber)
    {
        PlaceableEntity[] entities = new List<PlaceableEntity>(PlaceableEntity.ActiveEntities).ToArray();
        foreach (PlaceableEntity entity in entities)
        {
            if (entity == null || entity.IsCarried)
            {
                continue;
            }

            SellEntity(entity);
        }
    }

    public void AddGold(int amount)
    {
        Gold += amount;
        OnGoldChanged?.Invoke(Gold);
    }

    public bool TrySpendGold(int amount)
    {
        if (Gold < amount)
        {
            return false;
        }

        Gold -= amount;
        OnGoldChanged?.Invoke(Gold);
        return true;
    }

    public void SellEntity(PlaceableEntity entity)
    {
        int sellPrice = entity.EconomyData.SellPrice;
        if (UpgradeManager.Instance != null)
        {
            sellPrice = Mathf.RoundToInt(UpgradeManager.Instance.ApplyModifiers(ShopStatType.SellPrice, sellPrice));
        }

        AddGold(sellPrice);
        entity.HandleSold();
    }
}
