using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    public Animator animator;
    public PlayerController playerController;
    public Rigidbody playerRb;
    public Collider playerCol;

    public float groundCheckExtra = 0.15f;

    private bool isGrounded;

    void Update()
    {
        if (animator == null || playerController == null || playerRb == null || playerCol == null)
            return;

        CheckGround();

        float verticalSpeed = playerRb.linearVelocity.y;

        animator.SetBool("IsMoving", playerController.IsMoving);
        animator.SetBool("IsRunning", playerController.IsRunning);
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetFloat("VerticalSpeed", verticalSpeed);
    }

    void CheckGround()
    {
        Vector3 origin = playerCol.bounds.center;
        float rayLength = playerCol.bounds.extents.y + groundCheckExtra;

        isGrounded = Physics.Raycast(
            origin,
            Vector3.down,
            rayLength,
            ~0,
            QueryTriggerInteraction.Ignore
        );
    }
}
