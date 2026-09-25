using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCarry : MonoBehaviour
{
    public event Action<PlaceableEntity> OnPickedUp;

    private static readonly List<PlaceableEntity> carryQueryBuffer = new List<PlaceableEntity>();

    [SerializeField] private PlayerCarryData data;

    private PlayerMovement playerMovement;
    private PlayerAnimator playerAnimator;

    private PlaceableEntity carriedEntity;
    private bool isPickingUp;
    private int pendingActionToken;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerAnimator = GetComponent<PlayerAnimator>();
    }

    private void OnEnable()
    {
        playerMovement.OnInteractPressed += HandleInteractPressed;
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnRoundEnding += HandleRoundEnding;
        }
    }

    private void OnDisable()
    {
        playerMovement.OnInteractPressed -= HandleInteractPressed;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnRoundEnding -= HandleRoundEnding;
        }
    }

    private void HandleRoundEnding(int roundNumber)
    {
        pendingActionToken++;
        isPickingUp = false;
        playerAnimator.CancelPendingActions();

        PlaceableEntity released = carriedEntity;
        carriedEntity = null;

        if (released != null && released.gameObject.activeInHierarchy)
        {
            released.SetCarried(false);
        }
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

        int token = pendingActionToken;
        isPickingUp = true;
        playerAnimator.PlayPickUp(data.PickUpGraspFrameIndex, () =>
        {
            if (token != pendingActionToken)
            {
                return;
            }

            isPickingUp = false;

            if (target == null || !target.gameObject.activeInHierarchy
                || target.IsCarried || !target.CanBeCarried)
            {
                return;
            }

            target.SetCarried(true);
            carriedEntity = target;
            PunchEntity(target, data.PickUpPunch, data.PickUpPunchDuration);
            OnPickedUp?.Invoke(target);
        });
        playerMovement.LockMovement(data.PickUpLockDuration);
    }

    private void Drop()
    {
        PlaceableEntity target = carriedEntity;
        Vector3 dropPosition = transform.position + (Vector3)GetFacingDropOffset();
        int token = pendingActionToken;

        playerAnimator.PlayDropReversed(data.PickUpLockDuration, data.DropMoveFrameIndex, () =>
        {
            if (token != pendingActionToken)
            {
                return;
            }

            carriedEntity = null;

            if (target == null || !target.gameObject.activeInHierarchy || !target.IsCarried)
            {
                return;
            }

            target.transform.position = dropPosition;
            target.SyncSpatialCell();
            target.SetCarried(false);
            PunchEntity(target, data.DropPunch, data.DropPunchDuration);
        });
        playerMovement.LockMovement(data.PickUpLockDuration);
    }

    private static void PunchEntity(PlaceableEntity entity, Vector3 punch, float duration)
    {
        if (entity == null)
        {
            return;
        }

        entity.PlayPunchScale(punch, duration);
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
