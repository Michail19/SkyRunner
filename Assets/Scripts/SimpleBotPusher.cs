using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class SimpleBotPusher : MonoBehaviour
{
    [Header("Target")]
    public Transform player;
    public string playerTag = "Player";

    [Header("Movement")]
    public float moveSpeed = 3.5f;
    public float rotationSpeed = 10f;
    public float stopDistance = 1.2f;

    [Header("Attack")]
    public float attackDistance = 1.7f;
    public float attackCooldown = 1.2f;
    public float knockbackForce = 9f;
    public float knockbackUpForce = 1.2f;

    [Header("Falling")]
    public float destroyY = -10f;

    private Rigidbody rb;
    private float nextAttackTime;

    [Header("Animation")]
    public Animator animator;
    public string moveSpeedParameter = "MoveSpeed";
    public string attackTrigger = "Attack";

    private bool isMoving;

    [Header("Hole Falling")]
    public ArenaGenerator arenaGenerator;
    public bool canFallIntoHoles = true;
    public float holeDetectionRadius = 3.2f;
    public float holeFallDistance = 1.6f;
    public float holePullSpeed = 5f;
    public float holeFallChance = 0.35f;
    public float holeCheckInterval = 0.35f;
    public float holeCommitDistance = 0.4f;

    private bool isFallingIntoHole;
    private Vector3 targetHolePosition;
    private float nextHoleCheckTime;

    [Header("Step Jump")]
    public bool useStepJump = true;
    public float groundCheckDistance = 1.2f;
    public float stepCheckDistance = 0.65f;
    public float stepCheckHeight = 0.25f;
    public float stepJumpForce = 3.5f;
    public float stepJumpCooldown = 0.35f;
    public LayerMask groundMask = ~0;

    private float nextStepJumpTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (player == null && !string.IsNullOrWhiteSpace(playerTag))
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);

            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }

        rb.useGravity = true;
        rb.isKinematic = false;
        rb.freezeRotation = true;
    }

    private void FixedUpdate()
    {
        if (GamePauseState.IsPaused)
        {
            UpdateAnimation();
            return;
        }

        if (player == null)
        {
            UpdateAnimation();
            return;
        }

        if (transform.position.y < destroyY)
        {
            Destroy(gameObject);
            return;
        }

        if (canFallIntoHoles)
        {
            HandleHoleFall();

            if (isFallingIntoHole)
            {
                UpdateAnimation();
                return;
            }
        }

        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f;

        float distance = toPlayer.magnitude;

        if (distance <= 0.01f)
        {
            UpdateAnimation();
            return;
        }

        Vector3 direction = toPlayer.normalized;

        RotateToDirection(direction);

        isMoving = false;

        if (distance > stopDistance)
        {
            TryStepJump(direction);
            MoveToPlayer(direction);
            isMoving = true;
        }

        if (distance <= attackDistance)
        {
            TryPushPlayer(direction);
        }

        UpdateAnimation();
    }

    private void MoveToPlayer(Vector3 direction)
    {
        Vector3 newPosition = rb.position + direction * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);
    }

    private void RotateToDirection(Vector3 direction)
    {
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        Quaternion newRotation = Quaternion.Slerp(
            rb.rotation,
            targetRotation,
            rotationSpeed * Time.fixedDeltaTime
        );

        rb.MoveRotation(newRotation);
    }

    private void TryPushPlayer(Vector3 direction)
    {
        if (Time.time < nextAttackTime)
        {
            return;
        }

        nextAttackTime = Time.time + attackCooldown;

        if (animator != null)
        {
            animator.SetTrigger(attackTrigger);
        }

        PlayerController playerController = player.GetComponent<PlayerController>();

        if (playerController == null)
        {
            playerController = player.GetComponentInParent<PlayerController>();
        }

        if (playerController == null)
        {
            playerController = player.GetComponentInChildren<PlayerController>();
        }

        if (playerController != null)
        {
            playerController.ApplyKnockback(direction, knockbackForce, knockbackUpForce);
            return;
        }

        Rigidbody playerRb = player.GetComponent<Rigidbody>();

        if (playerRb != null)
        {
            playerRb.AddForce(
                direction * knockbackForce + Vector3.up * knockbackUpForce,
                ForceMode.VelocityChange
            );
        }
    }

    private void UpdateAnimation()
    {
        if (animator == null || rb == null)
        {
            return;
        }

        Vector3 velocity = GetRigidbodyVelocity();
        velocity.y = 0f;

        float normalizedSpeed = Mathf.Clamp01(velocity.magnitude / moveSpeed);

        animator.SetFloat(moveSpeedParameter, isMoving ? 1f : 0f);
    }

    private Vector3 GetRigidbodyVelocity()
    {
    #if UNITY_6000_0_OR_NEWER
        return rb.linearVelocity;
    #else
        return rb.velocity;
    #endif
    }

    private void HandleHoleFall()
    {
        if (arenaGenerator == null)
        {
            return;
        }

        if (isFallingIntoHole)
        {
            MoveIntoHole();
            return;
        }

        if (Time.time < nextHoleCheckTime)
        {
            return;
        }

        nextHoleCheckTime = Time.time + holeCheckInterval;

        ArenaTile nearestHole = FindNearestDestroyedTile();

        if (nearestHole == null)
        {
            return;
        }

        float distanceToHole = Vector3.Distance(
            new Vector3(transform.position.x, 0f, transform.position.z),
            new Vector3(nearestHole.transform.position.x, 0f, nearestHole.transform.position.z)
        );

        if (distanceToHole > holeFallDistance)
        {
            return;
        }

        if (Random.value > holeFallChance)
        {
            return;
        }

        StartFallingIntoHole(nearestHole.transform.position);
    }

    private ArenaTile FindNearestDestroyedTile()
    {
        ArenaTile nearestTile = null;
        float nearestDistance = float.MaxValue;

        foreach (ArenaTile tile in arenaGenerator.tiles)
        {
            if (tile == null || !tile.isDestroyed)
            {
                continue;
            }

            float distance = Vector3.Distance(
                new Vector3(transform.position.x, 0f, transform.position.z),
                new Vector3(tile.transform.position.x, 0f, tile.transform.position.z)
            );

            if (distance > holeDetectionRadius)
            {
                continue;
            }

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestTile = tile;
            }
        }

        return nearestTile;
    }

    private void StartFallingIntoHole(Vector3 holePosition)
    {
        isFallingIntoHole = true;

        targetHolePosition = new Vector3(
            holePosition.x,
            transform.position.y,
            holePosition.z
        );

        // Чтобы бот не цеплялся за край во время падения.
        Collider botCollider = GetComponent<Collider>();

        if (botCollider != null)
        {
            botCollider.enabled = false;
        }
    }

    private void MoveIntoHole()
    {
        Vector3 currentPosition = rb.position;

        Vector3 targetPosition = new Vector3(
            targetHolePosition.x,
            currentPosition.y,
            targetHolePosition.z
        );

        Vector3 newPosition = Vector3.MoveTowards(
            currentPosition,
            targetPosition,
            holePullSpeed * Time.fixedDeltaTime
        );

        rb.MovePosition(newPosition);

        Vector3 flatDelta = new Vector3(
            targetPosition.x - newPosition.x,
            0f,
            targetPosition.z - newPosition.z
        );

        if (flatDelta.magnitude <= holeCommitDistance)
        {
            rb.useGravity = true;

        #if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = new Vector3(0f, -4f, 0f);
        #else
            rb.velocity = new Vector3(0f, -4f, 0f);
        #endif
        }
    }

    private void TryStepJump(Vector3 direction)
    {
        if (!useStepJump)
        {
            return;
        }

        if (Time.time < nextStepJumpTime)
        {
            return;
        }

        if (!IsGrounded())
        {
            return;
        }

        if (!HasStepInFront(direction))
        {
            return;
        }

        nextStepJumpTime = Time.time + stepJumpCooldown;

        #if UNITY_6000_0_OR_NEWER
            Vector3 velocity = rb.linearVelocity;
            velocity.y = 0f;
            rb.linearVelocity = velocity;
        #else
            Vector3 velocity = rb.velocity;
            velocity.y = 0f;
            rb.velocity = velocity;
        #endif

        rb.AddForce(Vector3.up * stepJumpForce, ForceMode.VelocityChange);
    }

    private bool IsGrounded()
    {
        Vector3 origin = transform.position + Vector3.up * 0.1f;

        return Physics.Raycast(
            origin,
            Vector3.down,
            groundCheckDistance,
            groundMask,
            QueryTriggerInteraction.Ignore
        );
    }

    private bool HasStepInFront(Vector3 direction)
    {
        // Нижний луч: видит, что прямо перед ботом есть стенка/уступ.
        Vector3 lowerOrigin = transform.position + Vector3.up * stepCheckHeight;

        bool lowerHit = Physics.Raycast(
            lowerOrigin,
            direction,
            stepCheckDistance,
            groundMask,
            QueryTriggerInteraction.Ignore
        );

        if (!lowerHit)
        {
            return false;
        }

        // Верхний луч: проверяет, что это не высокая стена, а небольшой порожек.
        Vector3 upperOrigin = transform.position + Vector3.up * (stepCheckHeight + 0.65f);

        bool upperHit = Physics.Raycast(
            upperOrigin,
            direction,
            stepCheckDistance,
            groundMask,
            QueryTriggerInteraction.Ignore
        );

        return !upperHit;
    }
}
