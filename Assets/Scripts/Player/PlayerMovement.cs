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

    public float SprintSpeed => moveSpeed * sprintMultiplier;
    public Vector2 MoveInput => moveInput;
    public bool IsMoving => moveInput.sqrMagnitude > 0f;
    public bool IsSprinting => sprintAction.IsPressed();

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();

        moveAction = playerInput.actions.FindAction("Move");
        sprintAction = playerInput.actions.FindAction("Sprint");
        interactAction = playerInput.actions.FindAction("Jump");
    }

    private void OnEnable()
    {
        moveAction.Enable();
        sprintAction.Enable();
        interactAction.Enable();
        interactAction.performed += HandleInteractPerformed;
    }

    private void OnDisable()
    {
        interactAction.performed -= HandleInteractPerformed;
        moveAction.Disable();
        sprintAction.Disable();
        interactAction.Disable();
    }

    private void Update()
    {
        moveInput = isMovementLocked ? Vector2.zero : moveAction.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        if (isMovementLocked)
        {
            return;
        }

        float speed = sprintAction.IsPressed() ? SprintSpeed : moveSpeed;
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
