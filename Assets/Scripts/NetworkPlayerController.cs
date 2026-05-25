using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;

public class NetworkPlayerController : NetworkBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;

    [Header("Jump")]
    public float jumpForce = 7f;
    public float groundCheckExtra = 0.15f;

    [Header("Camera")]
    public Transform cameraTransform;
    public GameObject playerCamera;
    public AudioListener audioListener;

    [Header("Look")]
    public float minPitch = -80f;
    public float maxPitch = 80f;

    [SyncVar] private bool netIsMoving;
    [SyncVar] private bool netIsRunning;
    [SyncVar] private bool netIsGrounded;
    [SyncVar] private float netVerticalSpeed;

    private bool localIsMoving;
    private bool localIsRunning;
    private bool localIsGrounded;
    private float localVerticalSpeed;

    public bool IsMoving => isLocalPlayer ? localIsMoving : netIsMoving;
    public bool IsRunning => isLocalPlayer ? localIsRunning : netIsRunning;
    public bool IsGrounded => isLocalPlayer ? localIsGrounded : netIsGrounded;
    public float VerticalSpeed => isLocalPlayer ? localVerticalSpeed : netVerticalSpeed;

    private Rigidbody rb;
    private Collider col;

    private float pitch;
    private bool jumpQueued;

    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction lookAction;
    private InputAction runAction;

    void Awake()
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

        if (playerCamera != null)
            playerCamera.SetActive(false);

        if (audioListener != null)
            audioListener.enabled = false;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!isLocalPlayer)
        {
            if (playerCamera != null)
                playerCamera.SetActive(false);

            if (audioListener != null)
                audioListener.enabled = false;

            if (rb != null)
                rb.isKinematic = true;
        }
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        if (rb != null)
            rb.isKinematic = false;

        moveAction.Enable();
        jumpAction.Enable();
        lookAction.Enable();
        runAction.Enable();

        if (playerCamera != null)
            playerCamera.SetActive(true);

        if (audioListener != null)
            audioListener.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnDisable()
    {
        moveAction.Disable();
        jumpAction.Disable();
        lookAction.Disable();
        runAction.Disable();
    }

    void Update()
    {
        if (!isLocalPlayer)
            return;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        HandleLook();

        if (jumpAction.WasPressedThisFrame())
            jumpQueued = true;
    }

    void FixedUpdate()
    {
        if (!isLocalPlayer)
            return;

        CheckGround();
        HandleMove();
        HandleJump();
        SendAnimationStateToServer();
    }

    void HandleMove()
    {
        Vector2 input = moveAction.ReadValue<Vector2>();

        localIsMoving = input.sqrMagnitude > 0.01f;
        localIsRunning = runAction.IsPressed() && localIsMoving;

        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;

        camForward.y = 0f;
        camRight.y = 0f;

        camForward.Normalize();
        camRight.Normalize();

        Vector3 direction = (camRight * input.x + camForward * input.y).normalized;

        float currentSpeed = localIsRunning ? runSpeed : walkSpeed;

        Vector3 newPosition = rb.position + direction * currentSpeed * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);
    }

    void HandleJump()
    {
        if (!jumpQueued)
            return;

        if (localIsGrounded)
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

        jumpQueued = false;
    }

    void HandleLook()
    {
        if (cameraTransform == null)
            return;

        Vector2 delta = lookAction.ReadValue<Vector2>();

        float sensitivity = GameSettings.mouseSensitivity;

        float mouseX = delta.x * sensitivity;
        float mouseY = delta.y * sensitivity;

        transform.Rotate(Vector3.up * mouseX);

        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    void CheckGround()
    {
        Vector3 origin = col.bounds.center;
        float rayLength = col.bounds.extents.y + groundCheckExtra;

        localIsGrounded = Physics.Raycast(
            origin,
            Vector3.down,
            rayLength,
            ~0,
            QueryTriggerInteraction.Ignore
        );

        localVerticalSpeed = rb.linearVelocity.y;
    }

    void SendAnimationStateToServer()
    {
        CmdUpdateAnimationState(
            localIsMoving,
            localIsRunning,
            localIsGrounded,
            localVerticalSpeed
        );
    }

    [Command]
    void CmdUpdateAnimationState(bool moving, bool running, bool grounded, float verticalSpeed)
    {
        netIsMoving = moving;
        netIsRunning = running;
        netIsGrounded = grounded;
        netVerticalSpeed = verticalSpeed;
    }
}
