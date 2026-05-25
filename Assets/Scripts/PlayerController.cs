using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float tiredSpeedMultiplier = 0.4f;

    [Header("Jump")]
    public float jumpForce = 7f;
    public float groundCheckExtra = 0.15f;

    [Header("Look")]
    public Transform cameraTransform;
    public float minPitch = -80f;
    public float maxPitch = 80f;

    [Header("Stats")]
    public PlayerStats stats;

    [Header("Knockback")]
    public float knockbackControlLockTime = 0.35f;
    public bool resetHorizontalVelocityOnKnockback = true;
    public bool debugKnockback = true;

    [Header("Debug")]
    public bool enableDebugLog = false;
    public float debugLogEverySeconds = 0.5f;

    public bool IsMoving { get; private set; }
    public bool IsRunning { get; private set; }
    public bool IsKnockedBack => knockbackTimer > 0f;

    private Rigidbody rb;
    private Collider col;

    private float pitch;
    private bool isGrounded;
    private bool jumpQueued;
    private float knockbackTimer;
    private float debugTimer;

    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction lookAction;
    private InputAction runAction;

    [Header("Knockback Test")]
    public bool enableKnockbackTestKey = true;
    public KeyCode knockbackTestKey = KeyCode.K;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        moveAction = new InputAction("Move", InputActionType.Value);
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");

        jumpAction = new InputAction("Jump", InputActionType.Button, "<Keyboard>/space");
        lookAction = new InputAction("Look", InputActionType.Value, "<Mouse>/delta");
        runAction = new InputAction("Run", InputActionType.Button, "<Keyboard>/leftShift");
    }

    private void OnEnable()
    {
        moveAction.Enable();
        jumpAction.Enable();
        lookAction.Enable();
        runAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
        jumpAction.Disable();
        lookAction.Disable();
        runAction.Disable();
    }

    private void Start()
    {
        if (rb == null)
        {
            Debug.LogError("PlayerController: Rigidbody is missing.", this);
            enabled = false;
            return;
        }

        if (col == null)
        {
            Debug.LogError("PlayerController: Collider is missing.", this);
            enabled = false;
            return;
        }

        if (cameraTransform == null)
        {
            Debug.LogError("PlayerController: cameraTransform is not assigned.", this);
            enabled = false;
            return;
        }

        rb.useGravity = true;
        rb.isKinematic = false;
        rb.freezeRotation = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandleLook();

        if (!GamePauseState.IsPaused && jumpAction.WasPressedThisFrame())
        {
            jumpQueued = true;
        }

        if (enableKnockbackTestKey && Input.GetKeyDown(knockbackTestKey))
        {
            ApplyKnockback(-transform.forward, 10f, 1.2f);
        }

        DebugTick(Time.deltaTime);
    }

    private void FixedUpdate()
    {
        if (GamePauseState.IsPaused)
        {
            SetMovementFlags(false, false);
            return;
        }

        CheckGround();

        if (knockbackTimer > 0f)
        {
            knockbackTimer -= Time.fixedDeltaTime;
            HandleJump();
            TickStats(false);
            SetMovementFlags(false, false);
            return;
        }

        HandleMove();
        HandleJump();
        TickStats(IsMoving);
    }

    private void HandleMove()
    {
        Vector2 input = moveAction.ReadValue<Vector2>();

        bool movingNow = input.sqrMagnitude > 0.01f;
        bool runningNow = runAction.IsPressed() && movingNow;

        SetMovementFlags(movingNow, runningNow);

        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;

        camForward.y = 0f;
        camRight.y = 0f;

        camForward.Normalize();
        camRight.Normalize();

        Vector3 direction = (camRight * input.x + camForward * input.y).normalized;
        float currentSpeed = runningNow ? runSpeed : walkSpeed;

        float speedMultiplier = 1f;
        if (stats != null && !stats.HasStamina())
        {
            speedMultiplier = tiredSpeedMultiplier;
        }

        Vector3 newPosition = rb.position + direction * currentSpeed * speedMultiplier * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);
    }

    private void HandleJump()
    {
        if (!jumpQueued)
        {
            return;
        }

        if (isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        jumpQueued = false;
    }

    private void HandleLook()
    {
        if (GamePauseState.IsPaused || cameraTransform == null)
        {
            return;
        }

        Vector2 delta = lookAction.ReadValue<Vector2>();
        float sensitivity = GameSettings.mouseSensitivity;

        float mouseX = delta.x * sensitivity;
        float mouseY = delta.y * sensitivity;

        transform.Rotate(Vector3.up * mouseX);

        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    private void CheckGround()
    {
        Vector3 origin = col.bounds.center;
        float rayLength = col.bounds.extents.y + groundCheckExtra;

        isGrounded = Physics.Raycast(origin, Vector3.down, rayLength, ~0, QueryTriggerInteraction.Ignore);
    }

    public void ApplyKnockback(Vector3 direction, float force, float upForce)
    {
        if (rb == null)
        {
            return;
        }

        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
        {
            direction = -transform.forward;
        }

        direction.Normalize();
        knockbackTimer = knockbackControlLockTime;

        if (resetHorizontalVelocityOnKnockback)
        {
            Vector3 velocity = GetRigidbodyVelocity();
            velocity.x = 0f;
            velocity.z = 0f;
            SetRigidbodyVelocity(velocity);
        }

        rb.AddForce(direction * force + Vector3.up * upForce, ForceMode.VelocityChange);

        if (debugKnockback)
        {
            Debug.Log("PlayerController knockback applied. Direction: " + direction + ", force: " + force + ", upForce: " + upForce, this);
        }
    }

    private Vector3 GetRigidbodyVelocity()
    {
#if UNITY_6000_0_OR_NEWER
        return rb.linearVelocity;
#else
        return rb.velocity;
#endif
    }

    private void SetRigidbodyVelocity(Vector3 velocity)
    {
#if UNITY_6000_0_OR_NEWER
        rb.linearVelocity = velocity;
#else
        rb.velocity = velocity;
#endif
    }

    private void SetMovementFlags(bool moving, bool running)
    {
        IsMoving = moving;
        IsRunning = running;

        if (stats != null)
        {
            stats.isMoving = moving;
        }
    }

    private void TickStats(bool moving)
    {
        if (stats == null)
        {
            return;
        }

        stats.isMoving = moving;
        stats.TickStamina(Time.fixedDeltaTime);
    }

    private void DebugTick(float deltaTime)
    {
        if (!enableDebugLog)
        {
            return;
        }

        debugTimer += deltaTime;
        if (debugTimer < debugLogEverySeconds)
        {
            return;
        }

        debugTimer = 0f;

        float currentSpeed = rb != null ? GetRigidbodyVelocity().magnitude : 0f;

        Debug.Log(
            "Player debug"
            + " | grounded: " + isGrounded
            + " | speed: " + currentSpeed.ToString("0.00")
            + " | moving: " + IsMoving
            + " | running: " + IsRunning
            + " | knocked: " + IsKnockedBack
            + (stats != null
                ? " | stamina: " + stats.stamina.ToString("0.0") + "/" + stats.staminaMax.ToString("0.0")
                : " | stamina: (no PlayerStats)")
        );
    }
}
