using System;
using System.Collections;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private static readonly int IsMovingParam = Animator.StringToHash("IsMoving");
    private static readonly int IsSprintingParam = Animator.StringToHash("IsSprinting");
    private static readonly int PickUpParam = Animator.StringToHash("PickUp");
    private static readonly int MoveXParam = Animator.StringToHash("MoveX");
    private static readonly int MoveYParam = Animator.StringToHash("MoveY");

    private const float AxisBias = 0.001f;
    private const string PickUpStateName = "pick up";
    private const string IdleStateName = "idle";

    private PlayerMovement playerMovement;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private AnimationClip pickUpClip;

    private float facingMoveX = 1f;
    private float facingMoveY;
    private Coroutine reversePlaybackRoutine;
    private Coroutine pickUpGraspRoutine;

    public bool IsFacingHorizontal => facingMoveY == 0f;
    public float FacingSignX => Mathf.Sign(facingMoveX);
    public float FacingSignY => facingMoveY;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        pickUpClip = FindPickUpClip();
    }

    private AnimationClip FindPickUpClip()
    {
        RuntimeAnimatorController controller = animator.runtimeAnimatorController;
        if (controller == null)
        {
            return null;
        }

        AnimationClip[] clips = controller.animationClips;
        for (int i = 0; i < clips.Length; i++)
        {
            if (clips[i] != null && clips[i].name.IndexOf("pickup", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return clips[i];
            }
        }

        return null;
    }

    public void PlayPickUp()
    {
        PlayPickUpCore(-1, null);
    }

    public void PlayPickUp(int graspFrameIndex, Action onGraspFrameReached)
    {
        PlayPickUpCore(graspFrameIndex, onGraspFrameReached);
    }

    private void PlayPickUpCore(int graspFrameIndex, Action onGraspFrameReached)
    {
        if (reversePlaybackRoutine != null)
        {
            StopCoroutine(reversePlaybackRoutine);
            reversePlaybackRoutine = null;
        }

        if (pickUpGraspRoutine != null)
        {
            StopCoroutine(pickUpGraspRoutine);
            pickUpGraspRoutine = null;
        }

        animator.speed = 1f;
        animator.SetTrigger(PickUpParam);

        if (onGraspFrameReached != null)
        {
            pickUpGraspRoutine = StartCoroutine(PickUpGraspRoutine(graspFrameIndex, onGraspFrameReached));
        }
    }

    private IEnumerator PickUpGraspRoutine(int frameIndex, Action onGraspFrameReached)
    {
        float targetNormalizedTime = GetFrameNormalizedTime(frameIndex);
        if (targetNormalizedTime < 0f)
        {
            onGraspFrameReached();
            pickUpGraspRoutine = null;
            yield break;
        }

        while (!animator.GetCurrentAnimatorStateInfo(0).IsName(PickUpStateName))
        {
            yield return null;
        }

        while (true)
        {
            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
            if (!state.IsName(PickUpStateName) || state.normalizedTime >= targetNormalizedTime)
            {
                break;
            }

            yield return null;
        }

        onGraspFrameReached();
        pickUpGraspRoutine = null;
    }

    public void PlayDropReversed(float duration)
    {
        PlayDropReversedCore(duration, -1, null);
    }

    public void PlayDropReversed(float duration, int dropFrameIndex, Action onFrameReached)
    {
        PlayDropReversedCore(duration, dropFrameIndex, onFrameReached);
    }

    private void PlayDropReversedCore(float duration, int dropFrameIndex, Action onFrameReached)
    {
        if (reversePlaybackRoutine != null)
        {
            StopCoroutine(reversePlaybackRoutine);
        }

        if (pickUpGraspRoutine != null)
        {
            StopCoroutine(pickUpGraspRoutine);
            pickUpGraspRoutine = null;
        }

        reversePlaybackRoutine = StartCoroutine(PlayPickUpReversedRoutine(duration, dropFrameIndex, onFrameReached));
    }

    private float GetFrameNormalizedTime(int frameIndex)
    {
        if (pickUpClip == null || frameIndex < 0 || pickUpClip.length <= 0f)
        {
            return -1f;
        }

        return Mathf.Clamp01(frameIndex / (pickUpClip.frameRate * pickUpClip.length));
    }

    private IEnumerator PlayPickUpReversedRoutine(float duration, int dropFrameIndex, Action onFrameReached)
    {
        float targetNormalizedTime = GetFrameNormalizedTime(dropFrameIndex);
        bool callbackInvoked = onFrameReached == null;
        float previousNormalizedTime = 1f;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float normalizedTime = Mathf.Lerp(1f, 0f, elapsed / duration);
            animator.Play(PickUpStateName, 0, normalizedTime);
            animator.Update(0f);

            if (!callbackInvoked && targetNormalizedTime >= 0f &&
                previousNormalizedTime >= targetNormalizedTime && normalizedTime <= targetNormalizedTime)
            {
                onFrameReached();
                callbackInvoked = true;
            }

            previousNormalizedTime = normalizedTime;
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (!callbackInvoked)
        {
            onFrameReached();
        }

        animator.Play(IdleStateName, 0, 0f);
        reversePlaybackRoutine = null;
    }

    private void Update()
    {
        animator.SetBool(IsMovingParam, playerMovement.IsMoving);
        animator.SetBool(IsSprintingParam, playerMovement.IsSprinting);

        UpdateDirection();
    }

    private void UpdateDirection()
    {
        Vector2 moveInput = playerMovement.MoveInput;

        if (moveInput.sqrMagnitude > 0f)
        {
            bool isHorizontal = Mathf.Abs(moveInput.x) >= Mathf.Abs(moveInput.y);

            facingMoveX = isHorizontal ? Mathf.Sign(moveInput.x) : AxisBias;
            facingMoveY = isHorizontal ? 0f : Mathf.Sign(moveInput.y);

            spriteRenderer.flipX = facingMoveX < 0f;
        }

        animator.SetFloat(MoveXParam, Mathf.Abs(facingMoveX));
        animator.SetFloat(MoveYParam, facingMoveY);
    }
}
