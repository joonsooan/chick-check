using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCarry : MonoBehaviour
{
    public event Action<PlaceableEntity> OnPickedUp;
    public event Action<PlaceableEntity> OnDropped;

    private static readonly List<PlaceableEntity> carryQueryBuffer = new List<PlaceableEntity>();

    [SerializeField] private PlayerCarryData data;

    private PlayerMovement playerMovement;
    private PlayerAnimator playerAnimator;

    private PlaceableEntity carriedEntity;
    private bool isPickingUp;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerAnimator = GetComponent<PlayerAnimator>();
    }

    private void OnEnable()
    {
        playerMovement.OnInteractPressed += HandleInteractPressed;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnRoundEnding += HandleRoundEnding;
        }
    }

    private void OnDisable()
    {
        playerMovement.OnInteractPressed -= HandleInteractPressed;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnRoundEnding -= HandleRoundEnding;
        }
    }

    private void HandleRoundEnding(int roundNumber)
    {
        if (carriedEntity == null)
        {
            return;
        }

        carriedEntity.SetCarried(false);
        carriedEntity = null;
    }

    private void Update()
    {
        if (carriedEntity == null)
        {
            return;
        }

        carriedEntity.transform.position = (Vector2)transform.position + data.CarryOffset;
        carriedEntity.SyncSpatialCell();

        if (playerAnimator.IsFacingHorizontal)
        {
            carriedEntity.SpriteRenderer.flipX = playerAnimator.FacingSignX > 0f;
        }
    }

    private void HandleInteractPressed()
    {
        if (carriedEntity != null)
        {
            Drop();
            return;
        }

        if (isPickingUp)
        {
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

        isPickingUp = true;
        playerAnimator.PlayPickUp(data.PickUpGraspFrameIndex, () =>
        {
            isPickingUp = false;

            if (target == null || !target.gameObject.activeInHierarchy
                || target.IsCarried || !target.CanBeCarried)
            {
                return;
            }

            target.SetCarried(true);
            carriedEntity = target;
            OnPickedUp?.Invoke(target);
        });
        playerMovement.LockMovement(data.PickUpLockDuration);
    }

    private void Drop()
    {
        PlaceableEntity target = carriedEntity;
        Vector3 dropPosition = transform.position + (Vector3)GetFacingDropOffset();

        playerAnimator.PlayDropReversed(data.PickUpLockDuration, data.DropMoveFrameIndex, () =>
        {
            carriedEntity = null;

            if (target == null || !target.gameObject.activeInHierarchy)
            {
                return;
            }

            target.transform.position = dropPosition;
            target.SyncSpatialCell();
            target.SetCarried(false);
            OnDropped?.Invoke(target);
        });
        playerMovement.LockMovement(data.PickUpLockDuration);
    }

    private Vector2 GetFacingDropOffset()
    {
        if (playerAnimator.IsFacingHorizontal)
        {
            return new Vector2(playerAnimator.FacingSignX * data.DropOffsetX, 0f);
        }

        return new Vector2(0f, playerAnimator.FacingSignY > 0f ? data.DropOffsetYUp : -data.DropOffsetYDown);
    }

    private PlaceableEntity FindNearestEntity()
    {
        PlaceableEntity nearest = null;
        float nearestSqrDistance = data.CarryRadius * data.CarryRadius;

        carryQueryBuffer.Clear();
        PlaceableEntity.Spatial.Query(transform.position, data.CarryRadius, carryQueryBuffer);

        for (int i = 0; i < carryQueryBuffer.Count; i++)
        {
            PlaceableEntity candidate = carryQueryBuffer[i];
            if (candidate.IsCarried || !candidate.CanBeCarried)
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
