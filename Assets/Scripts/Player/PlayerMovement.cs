using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float sprintMultiplier = 1.8f;

    private Rigidbody2D rb;
    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction sprintAction;
    private InputAction interactAction;

    private Vector2 moveInput;
    private bool isMovementLocked;
    private Coroutine movementLockRoutine;
    private float footstepTimer;
    private Vector2 startPosition;

    public float BaseMoveSpeed => UpgradeManager.Instance != null
        ? UpgradeManager.Instance.ApplyModifiers(ShopStatType.PlayerMoveSpeed, moveSpeed)
        : moveSpeed;
    public float SprintSpeed => BaseMoveSpeed * sprintMultiplier;
    public Vector2 MoveInput => moveInput;
    public bool IsMoving => moveInput.sqrMagnitude > 0f;
    public bool SprintUnlocked => UpgradeManager.Instance != null && UpgradeManager.Instance.SprintEnabled;
    public bool IsSprinting => SprintUnlocked && sprintAction.IsPressed();

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();

        moveAction = playerInput.actions.FindAction("Move");
        sprintAction = playerInput.actions.FindAction("Sprint");
        interactAction = playerInput.actions.FindAction("Jump");
    }

    private void Start()
    {
        startPosition = transform.position;
    }

    private void OnEnable()
    {
        moveAction.Enable();
        sprintAction.Enable();
        interactAction.Enable();
        interactAction.performed += HandleInteractPerformed;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnRoundEnding += HandleRoundEnding;
        }
    }

    private void OnDisable()
    {
        interactAction.performed -= HandleInteractPerformed;
        moveAction.Disable();
        sprintAction.Disable();
        interactAction.Disable();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnRoundEnding -= HandleRoundEnding;
        }
    }

    private void HandleRoundEnding(int roundNumber)
    {
        rb.linearVelocity = Vector2.zero;
        rb.position = startPosition;
        transform.position = startPosition;
        moveInput = Vector2.zero;
    }

    private bool CanMove => !isMovementLocked
        && (GameManager.Instance == null || GameManager.Instance.CurrentPhase == GamePhase.Playing);

    private void Update()
    {
        moveInput = CanMove ? moveAction.ReadValue<Vector2>() : Vector2.zero;
        UpdateFootsteps();
    }

    private void UpdateFootsteps()
    {
        if (!IsMoving || AudioManager.Instance == null)
        {
            footstepTimer = 0f;
            return;
        }

        footstepTimer -= Time.deltaTime;
        if (footstepTimer <= 0f)
        {
            AudioManager.Instance.PlayFootstep();
            float interval = AudioManager.Instance.FootstepInterval;
            footstepTimer = IsSprinting ? interval * 0.5f : interval;
        }
    }

    private void FixedUpdate()
    {
        if (!CanMove)
        {
            return;
        }

        float speed = IsSprinting ? SprintSpeed : BaseMoveSpeed;
        rb.MovePosition(rb.position + moveInput * (speed * Time.fixedDeltaTime));
    }

    private void HandleInteractPerformed(InputAction.CallbackContext context)
    {
        OnInteractPressed?.Invoke();
    }

    public void LockMovement(float duration)
    {
        if (movementLockRoutine != null)
        {
            StopCoroutine(movementLockRoutine);
        }

        movementLockRoutine = StartCoroutine(LockMovementRoutine(duration));
    }

    private IEnumerator LockMovementRoutine(float duration)
    {
        isMovementLocked = true;
        moveInput = Vector2.zero;

        yield return new WaitForSeconds(duration);

        isMovementLocked = false;
        movementLockRoutine = null;
    }

    public event Action OnInteractPressed;
}
