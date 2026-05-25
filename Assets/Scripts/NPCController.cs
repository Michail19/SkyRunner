using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// NPC zombie controller for NavMeshAgent.
///
/// Required on zombie:
/// - NavMeshAgent
/// - Animator
///
/// Required on player:
/// - Tag "Player" or manually assigned player Transform
/// - PlayerController with Rigidbody
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class NPCController : MonoBehaviour
{
    private enum ZombieState
    {
        Patrol,
        Chase,
        Attack,
        Dead
    }

    [Header("Target")]
    [SerializeField] private Transform player;
    [SerializeField] private string playerTag = "Player";

    [Header("Patrol")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float patrolPointReachDistance = 1f;
    [SerializeField] private bool loopPatrol = true;

    [Header("Detection")]
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private float detectRadius = 10f;
    [SerializeField] private float loseRadius = 15f;
    [SerializeField] private float eyeHeight = 1.5f;

    [Header("Movement")]
    [SerializeField] private float patrolSpeed = 1.5f;
    [SerializeField] private float chaseSpeed = 3.5f;
    [SerializeField] private float stoppingDistanceWhileChasing = 1.4f;

    [Header("Attack")]
    [SerializeField] private float attackDistance = 1.6f;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private bool rotateToPlayerOnAttack = true;
    [SerializeField] private float attackRotationSpeed = 10f;

    [Header("Attack Knockback")]
    [SerializeField] private float knockbackForce = 8f;
    [SerializeField] private float knockbackUpForce = 1.2f;
    [SerializeField] private float knockbackHitDistanceExtra = 1.0f;

    [Tooltip("If false, knockback happens immediately when Attack trigger is fired. If true, call ApplyKnockbackToPlayer from Animation Event.")]
    [SerializeField] private bool useAnimationEventForKnockback = false;

    [SerializeField] private bool knockbackOnlyOncePerAttack = true;

    [Tooltip("Recommended true while testing. Wrong obstacleMask can otherwise block knockback.")]
    [SerializeField] private bool ignoreLineOfSightForKnockback = true;

    [SerializeField] private bool debugKnockback = true;

    [Header("OffMeshLink Jump")]
    [SerializeField] private bool useOffMeshLinkJump = true;
    [SerializeField] private float jumpDuration = 0.5f;
    [SerializeField] private float jumpHeight = 1.2f;

    [Header("Animator")]
    [SerializeField] private Animator animator;
    [SerializeField] private string moveSpeedParameter = "MoveSpeed";
    [SerializeField] private string isChasingParameter = "IsChasing";
    [SerializeField] private string attackTrigger = "Attack";
    [SerializeField] private string deadTrigger = "Dead";
    [SerializeField] private string jumpTrigger = "Jump";
    [SerializeField] private string landTrigger = "Land";

    [SerializeField] private float knockbackDelayAfterAttackStart = 0.45f;

    private Coroutine delayedKnockbackCoroutine;

    private NavMeshAgent agent;
    private ZombieState state = ZombieState.Patrol;

    private int patrolIndex;
    private int patrolDirection = 1;
    private bool isJumping;
    private float nextAttackTime;
    private bool hasKnockedBackThisAttack;

    private bool hasMoveSpeedParameter;
    private bool hasIsChasingParameter;
    private bool hasAttackTrigger;
    private bool hasDeadTrigger;
    private bool hasJumpTrigger;
    private bool hasLandTrigger;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (player == null && !string.IsNullOrWhiteSpace(playerTag))
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }

        CacheAnimatorParameters();
    }

    private void Start()
    {
        if (agent == null)
        {
            Debug.LogError("NPCController: NavMeshAgent is missing.", this);
            enabled = false;
            return;
        }

        agent.autoTraverseOffMeshLink = false;
        SetState(ZombieState.Patrol, true);
        SetNextPatrolDestination();
    }

    private void Update()
    {
        if (state == ZombieState.Dead)
        {
            UpdateAnimatorMovement();
            return;
        }

        if (agent.enabled && useOffMeshLinkJump && agent.isOnOffMeshLink && !isJumping)
        {
            StartCoroutine(JumpOffMeshLink());
            return;
        }

        if (player == null)
        {
            PatrolUpdate();
            UpdateAnimatorMovement();
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        switch (state)
        {
            case ZombieState.Patrol:
                if (distanceToPlayer <= detectRadius && CanSeePlayer())
                {
                    SetState(ZombieState.Chase);
                }
                else
                {
                    PatrolUpdate();
                }
                break;

            case ZombieState.Chase:
                if (distanceToPlayer > loseRadius)
                {
                    SetState(ZombieState.Patrol);
                    SetNextPatrolDestination();
                }
                else if (distanceToPlayer <= attackDistance)
                {
                    SetState(ZombieState.Attack);
                    AttackUpdate();
                }
                else
                {
                    ChaseUpdate();
                }
                break;

            case ZombieState.Attack:
                if (distanceToPlayer > attackDistance + knockbackHitDistanceExtra)
                {
                    SetState(distanceToPlayer <= loseRadius ? ZombieState.Chase : ZombieState.Patrol);

                    if (state == ZombieState.Patrol)
                    {
                        SetNextPatrolDestination();
                    }
                }
                else
                {
                    AttackUpdate();
                }
                break;
        }

        UpdateAnimatorMovement();
    }

    private void SetState(ZombieState newState, bool force = false)
    {
        if (!force && state == newState)
        {
            return;
        }

        state = newState;

        switch (state)
        {
            case ZombieState.Patrol:
                if (agent.enabled)
                {
                    agent.isStopped = false;
                    agent.speed = patrolSpeed;
                    agent.stoppingDistance = 0f;
                }
                SetAnimatorBool(isChasingParameter, hasIsChasingParameter, false);
                break;

            case ZombieState.Chase:
                if (agent.enabled)
                {
                    agent.isStopped = false;
                    agent.speed = chaseSpeed;
                    agent.stoppingDistance = stoppingDistanceWhileChasing;
                }
                SetAnimatorBool(isChasingParameter, hasIsChasingParameter, true);
                break;

            case ZombieState.Attack:
                if (agent.enabled)
                {
                    agent.ResetPath();
                    agent.isStopped = true;
                }
                SetAnimatorBool(isChasingParameter, hasIsChasingParameter, true);
                break;

            case ZombieState.Dead:
                if (agent.enabled)
                {
                    agent.ResetPath();
                    agent.isStopped = true;
                    agent.enabled = false;
                }
                SetAnimatorBool(isChasingParameter, hasIsChasingParameter, false);
                SetAnimatorTrigger(deadTrigger, hasDeadTrigger);
                break;
        }
    }

    private void PatrolUpdate()
    {
        if (patrolPoints == null || patrolPoints.Length == 0 || !agent.enabled || agent.pathPending)
        {
            return;
        }

        if (!agent.hasPath || agent.remainingDistance <= patrolPointReachDistance)
        {
            MoveToNextPatrolPoint();
            SetNextPatrolDestination();
        }
    }

    private void ChaseUpdate()
    {
        if (player == null || !agent.enabled)
        {
            return;
        }

        agent.isStopped = false;
        agent.SetDestination(player.position);
    }

    private void AttackUpdate()
    {
        if (player == null)
        {
            return;
        }

        if (rotateToPlayerOnAttack)
        {
            RotateTowardsPlayer();
        }

        if (Time.time < nextAttackTime)
        {
            return;
        }

        if (debugKnockback)
        {
            Debug.Log("NPCController attack: attack started.", this);
        }

        hasKnockedBackThisAttack = false;

        SetAnimatorTrigger(attackTrigger, hasAttackTrigger);

        if (delayedKnockbackCoroutine != null)
        {
            StopCoroutine(delayedKnockbackCoroutine);
        }

        delayedKnockbackCoroutine = StartCoroutine(ApplyKnockbackAfterDelay());

        nextAttackTime = Time.time + attackCooldown;
    }

    private IEnumerator ApplyKnockbackAfterDelay()
    {
        yield return new WaitForSeconds(knockbackDelayAfterAttackStart);

        if (state != ZombieState.Attack)
        {
            yield break;
        }

        ApplyKnockbackToPlayer();
    }

    /// <summary>
    /// Can be called from zombie attack Animation Event.
    /// </summary>
    public void ApplyKnockbackToPlayer()
    {
        if (player == null || state == ZombieState.Dead)
        {
            return;
        }

        if (knockbackOnlyOncePerAttack && hasKnockedBackThisAttack)
        {
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer > attackDistance + knockbackHitDistanceExtra)
        {
            LogKnockback("Too far: " + distanceToPlayer.ToString("0.00"));
            return;
        }

        if (!ignoreLineOfSightForKnockback && !CanSeePlayer())
        {
            LogKnockback("Line of sight blocked.");
            return;
        }

        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
        {
            direction = transform.forward;
        }

        direction.Normalize();

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
            hasKnockedBackThisAttack = true;
            LogKnockback("Applied through PlayerController.");
            return;
        }

        Rigidbody playerRigidbody = player.GetComponent<Rigidbody>();
        if (playerRigidbody == null)
        {
            playerRigidbody = player.GetComponentInParent<Rigidbody>();
        }
        if (playerRigidbody == null)
        {
            playerRigidbody = player.GetComponentInChildren<Rigidbody>();
        }

        if (playerRigidbody != null)
        {
            playerRigidbody.AddForce(direction * knockbackForce + Vector3.up * knockbackUpForce, ForceMode.VelocityChange);
            hasKnockedBackThisAttack = true;
            LogKnockback("Applied through Rigidbody fallback.");
            return;
        }

        LogKnockback("No PlayerController or Rigidbody found.");
    }


    private void LogKnockback(string message)
    {
        if (debugKnockback)
        {
            Debug.Log("NPCController knockback: " + message, this);
        }
    }

    private void MoveToNextPatrolPoint()
    {
        if (patrolPoints == null || patrolPoints.Length <= 1)
        {
            patrolIndex = 0;
            return;
        }

        if (loopPatrol)
        {
            patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
            return;
        }

        patrolIndex += patrolDirection;

        if (patrolIndex >= patrolPoints.Length)
        {
            patrolIndex = patrolPoints.Length - 2;
            patrolDirection = -1;
        }
        else if (patrolIndex < 0)
        {
            patrolIndex = 1;
            patrolDirection = 1;
        }

        patrolIndex = Mathf.Clamp(patrolIndex, 0, patrolPoints.Length - 1);
    }

    private void SetNextPatrolDestination()
    {
        if (patrolPoints == null || patrolPoints.Length == 0 || !agent.enabled)
        {
            return;
        }

        patrolIndex = Mathf.Clamp(patrolIndex, 0, patrolPoints.Length - 1);

        if (patrolPoints[patrolIndex] != null)
        {
            agent.SetDestination(patrolPoints[patrolIndex].position);
        }
    }

    private bool CanSeePlayer()
    {
        if (player == null)
        {
            return false;
        }

        Vector3 start = transform.position + Vector3.up * eyeHeight;
        Vector3 end = player.position + Vector3.up * eyeHeight;
        Vector3 direction = end - start;
        float distance = direction.magnitude;

        if (distance <= 0.01f)
        {
            return true;
        }

        return !Physics.Raycast(start, direction.normalized, distance, obstacleMask, QueryTriggerInteraction.Ignore);
    }

    private IEnumerator JumpOffMeshLink()
    {
        isJumping = true;
        SetAnimatorTrigger(jumpTrigger, hasJumpTrigger);

        OffMeshLinkData data = agent.currentOffMeshLinkData;
        Vector3 startPosition = transform.position;
        Vector3 endPosition = data.endPos + Vector3.up * agent.baseOffset;

        agent.isStopped = true;

        float timer = 0f;
        while (timer < 1f)
        {
            Vector3 position = Vector3.Lerp(startPosition, endPosition, timer);
            position.y += Mathf.Sin(timer * Mathf.PI) * jumpHeight;
            transform.position = position;

            timer += Time.deltaTime / Mathf.Max(0.01f, jumpDuration);
            yield return null;
        }

        transform.position = endPosition;
        agent.CompleteOffMeshLink();
        agent.isStopped = false;

        SetAnimatorTrigger(landTrigger, hasLandTrigger);
        isJumping = false;
    }

    private void RotateTowardsPlayer()
    {
        if (player == null)
        {
            return;
        }

        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * attackRotationSpeed);
    }

    private void UpdateAnimatorMovement()
    {
        if (animator == null || !hasMoveSpeedParameter)
        {
            return;
        }

        float normalizedSpeed = 0f;

        if (agent != null && agent.enabled && !agent.isStopped && agent.speed > 0f)
        {
            normalizedSpeed = Mathf.Clamp01(agent.velocity.magnitude / agent.speed);
        }

        animator.SetFloat(moveSpeedParameter, normalizedSpeed);
    }

    public void Die()
    {
        SetState(ZombieState.Dead);
    }

    public void SetPlayer(Transform newPlayer)
    {
        player = newPlayer;
    }

    private void CacheAnimatorParameters()
    {
        if (animator == null || animator.runtimeAnimatorController == null)
        {
            return;
        }

        hasMoveSpeedParameter = HasAnimatorParameter(moveSpeedParameter, AnimatorControllerParameterType.Float);
        hasIsChasingParameter = HasAnimatorParameter(isChasingParameter, AnimatorControllerParameterType.Bool);
        hasAttackTrigger = HasAnimatorParameter(attackTrigger, AnimatorControllerParameterType.Trigger);
        hasDeadTrigger = HasAnimatorParameter(deadTrigger, AnimatorControllerParameterType.Trigger);
        hasJumpTrigger = HasAnimatorParameter(jumpTrigger, AnimatorControllerParameterType.Trigger);
        hasLandTrigger = HasAnimatorParameter(landTrigger, AnimatorControllerParameterType.Trigger);
    }

    private bool HasAnimatorParameter(string parameterName, AnimatorControllerParameterType parameterType)
    {
        if (string.IsNullOrWhiteSpace(parameterName) || animator == null)
        {
            return false;
        }

        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.name == parameterName && parameter.type == parameterType)
            {
                return true;
            }
        }

        return false;
    }

    private void SetAnimatorBool(string parameterName, bool parameterExists, bool value)
    {
        if (animator != null && parameterExists && !string.IsNullOrWhiteSpace(parameterName))
        {
            animator.SetBool(parameterName, value);
        }
    }

    private void SetAnimatorTrigger(string triggerName, bool parameterExists)
    {
        if (animator != null && parameterExists && !string.IsNullOrWhiteSpace(triggerName))
        {
            animator.SetTrigger(triggerName);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, loseRadius);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, attackDistance);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, attackDistance + knockbackHitDistanceExtra);
    }
}
