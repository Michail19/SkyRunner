using UnityEngine;
public class NetworkPlayerAnimationController : MonoBehaviour
{
    public Animator animator;
    public NetworkPlayerController playerController;
    void Update()
    {
        if (animator == null || playerController == null)
            return;
        animator.SetBool("IsMoving", playerController.IsMoving);
        animator.SetBool("IsRunning", playerController.IsRunning);
        animator.SetBool("IsGrounded", playerController.IsGrounded);
        animator.SetFloat("VerticalSpeed", playerController.VerticalSpeed);
    }
}
