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
            return;
        }

        if (player == null)
        {
            return;
        }

        if (transform.position.y < destroyY)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f;

        float distance = toPlayer.magnitude;

        if (distance <= 0.01f)
        {
            return;
        }

        Vector3 direction = toPlayer.normalized;

        RotateToDirection(direction);

        if (distance > stopDistance)
        {
            MoveToPlayer(direction);
        }

        if (distance <= attackDistance)
        {
            TryPushPlayer(direction);
        }
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
}
