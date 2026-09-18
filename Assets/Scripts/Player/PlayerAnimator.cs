using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private static readonly int IsMovingParam = Animator.StringToHash("IsMoving");
    private static readonly int IsSprintingParam = Animator.StringToHash("IsSprinting");
    private static readonly int PickUpParam = Animator.StringToHash("PickUp");
    private static readonly int MoveXParam = Animator.StringToHash("MoveX");
    private static readonly int MoveYParam = Animator.StringToHash("MoveY");

    private const float AxisBias = 0.001f;

    private PlayerMovement playerMovement;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private float facingMoveX = 1f;
    private float facingMoveY;

    public bool IsFacingHorizontal => facingMoveY == 0f;
    public float FacingSignX => Mathf.Sign(facingMoveX);
    public float FacingSignY => facingMoveY;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        playerMovement.OnInteractPressed += HandlePickUp;
    }

    private void OnDisable()
    {
        playerMovement.OnInteractPressed -= HandlePickUp;
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

    private void HandlePickUp()
    {
        animator.SetTrigger(PickUpParam);
    }
}
