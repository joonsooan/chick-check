using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private PlayerData data;

    private Rigidbody2D rb;
    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction sprintAction;
    private InputAction interactAction;

    private Vector2 moveInput;
    private bool isMovementLocked;
    private bool sprintToggled;
    private Coroutine movementLockRoutine;
    private float footstepTimer;
    private Vector2 startPosition;
    private float cachedWalkSpeed;
    private bool sprintUnlocked;

    public float WalkSpeed => cachedWalkSpeed;
    public float SprintSpeed => WalkSpeed * data.SprintMultiplier;
    public Vector2 MoveInput => moveInput;
    public bool IsMoving => moveInput.sqrMagnitude > 0f;
    public bool IsSprinting => sprintUnlocked && sprintToggled;

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

        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.OnModifiersChanged += HandleModifiersChanged;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnRoundEnding += HandleRoundEnding;
        }

        RefreshUpgrades();
    }

    private void OnDestroy()
    {
        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.OnModifiersChanged -= HandleModifiersChanged;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnRoundEnding -= HandleRoundEnding;
        }
    }

    private void OnEnable()
    {
        moveAction.Enable();
        sprintAction.Enable();
        interactAction.Enable();
        interactAction.performed += HandleInteractPerformed;
        sprintAction.performed += HandleSprintPerformed;
    }

    private void OnDisable()
    {
        interactAction.performed -= HandleInteractPerformed;
        sprintAction.performed -= HandleSprintPerformed;
        moveAction.Disable();
        sprintAction.Disable();
        interactAction.Disable();
    }

    private void HandleModifiersChanged()
    {
        RefreshUpgrades();
    }

    private void RefreshUpgrades()
    {
        UpgradeManager manager = UpgradeManager.Instance;
        cachedWalkSpeed = manager != null
            ? manager.ApplyModifiers(ShopStatType.PlayerMoveSpeed, data.MoveSpeed)
            : data.MoveSpeed;
        sprintUnlocked = manager != null && manager.State.Get<SprintState>().Enabled;
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
            footstepTimer = IsSprinting ? interval * data.SprintFootstepIntervalRatio : interval;
        }
    }

    private void FixedUpdate()
    {
        if (!CanMove)
        {
            return;
        }

        float speed = IsSprinting ? SprintSpeed : WalkSpeed;
        rb.MovePosition(rb.position + moveInput * (speed * Time.fixedDeltaTime));
    }

    private void HandleInteractPerformed(InputAction.CallbackContext context)
    {
        OnInteractPressed?.Invoke();
    }

    private void HandleSprintPerformed(InputAction.CallbackContext context)
    {
        sprintToggled = !sprintToggled;
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
