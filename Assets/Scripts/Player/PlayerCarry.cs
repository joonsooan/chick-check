using UnityEngine;

public class PlayerCarry : MonoBehaviour
{
    [SerializeField] private PlayerCarryData data;

    private PlayerMovement playerMovement;
    private PlayerAnimator playerAnimator;

    private PlaceableEntity carriedEntity;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerAnimator = GetComponent<PlayerAnimator>();
    }

    private void OnEnable()
    {
        playerMovement.OnInteractPressed += HandleInteractPressed;
    }

    private void OnDisable()
    {
        playerMovement.OnInteractPressed -= HandleInteractPressed;
    }

    private void Update()
    {
        if (carriedEntity == null)
        {
            return;
        }

        carriedEntity.transform.position = (Vector2)transform.position + data.CarryOffset;
    }

    private void HandleInteractPressed()
    {
        if (carriedEntity != null)
        {
            Drop();
            return;
        }

        TryPickUp();
    }

    private void TryPickUp()
    {
        PlaceableEntity target = FindNearestEntity();
        if (target == null)
        {
            return;
        }

        carriedEntity = target;
        carriedEntity.SetCarried(true);
        playerMovement.LockMovement(data.PickUpLockDuration);
    }

    private void Drop()
    {
        Vector3 dropPosition = transform.position + (Vector3)GetFacingDropOffset();
        carriedEntity.transform.position = dropPosition;

        carriedEntity.SetCarried(false);
        carriedEntity = null;
        playerMovement.LockMovement(data.PickUpLockDuration);
    }

    private Vector2 GetFacingDropOffset()
    {
        if (playerAnimator.IsFacingHorizontal)
        {
            return new Vector2(playerAnimator.FacingSignX * data.DropOffsetX, 0f);
        }

        return new Vector2(0f, data.DropOffsetY);
    }

    private PlaceableEntity FindNearestEntity()
    {
        PlaceableEntity nearest = null;
        float nearestSqrDistance = data.CarryRadius * data.CarryRadius;

        var activeEntities = PlaceableEntity.ActiveEntities;
        for (int i = 0; i < activeEntities.Count; i++)
        {
            PlaceableEntity candidate = activeEntities[i];
            if (candidate.IsCarried)
            {
                continue;
            }

            float sqrDistance = (candidate.transform.position - transform.position).sqrMagnitude;
            if (sqrDistance <= nearestSqrDistance)
            {
                nearestSqrDistance = sqrDistance;
                nearest = candidate;
            }
        }

        return nearest;
    }
}
