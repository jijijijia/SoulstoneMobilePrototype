using UnityEngine;

public sealed class EnemyMovementController
{
    private readonly Transform ownerTransform;
    private readonly CharacterController characterController;
    private readonly float gravity;
    private readonly float meleeLaneOffsetStrength;
    private readonly float meleePreferredDistanceVariance;
    private readonly float obstacleProbeDistance;
    private readonly float obstacleProbeRadiusMultiplier;
    private readonly float obstacleProbeHeight;
    private readonly float[] avoidanceAngles;

    private const float ProbeInterval = 0.1f;

    private RuntimeStats runtimeStats;
    private EnemyData enemyData;
    private Vector3 velocity;
    private float laneOffset;
    private float personalPreferredDistance;
    private float nextProbeTime;
    private int cachedAvoidanceAngleIndex = -1;
    private float cachedTargetRadius = -1f;
    private Vector3 lastHorizontalVelocity;

    public EnemyMovementController(
        Transform ownerTransform,
        CharacterController characterController,
        float gravity,
        float meleeLaneOffsetStrength,
        float meleePreferredDistanceVariance,
        float obstacleProbeDistance,
        float obstacleProbeRadiusMultiplier,
        float obstacleProbeHeight,
        float[] avoidanceAngles)
    {
        this.ownerTransform = ownerTransform;
        this.characterController = characterController;
        this.gravity = gravity;
        this.meleeLaneOffsetStrength = meleeLaneOffsetStrength;
        this.meleePreferredDistanceVariance = meleePreferredDistanceVariance;
        this.obstacleProbeDistance = obstacleProbeDistance;
        this.obstacleProbeRadiusMultiplier = obstacleProbeRadiusMultiplier;
        this.obstacleProbeHeight = obstacleProbeHeight;
        this.avoidanceAngles = avoidanceAngles ?? new float[0];
    }

    public float PreferredCombatDistance => personalPreferredDistance > 0f
        ? personalPreferredDistance
        : (enemyData != null ? enemyData.PreferredDistance : 1.4f);
    public Vector3 LastHorizontalVelocity => lastHorizontalVelocity;

    public void Initialize(EnemyData data, RuntimeStats stats)
    {
        enemyData = data;
        runtimeStats = stats;
        velocity = Vector3.zero;
        lastHorizontalVelocity = Vector3.zero;
        InitializeMovementProfile();
    }

    public void Reset()
    {
        enemyData = null;
        runtimeStats = null;
        velocity = Vector3.zero;
        lastHorizontalVelocity = Vector3.zero;
        laneOffset = 0f;
        personalPreferredDistance = 0f;
        nextProbeTime = 0f;
        cachedAvoidanceAngleIndex = -1;
        cachedTargetRadius = -1f;
    }

    public void MoveToTarget(Transform combatTarget, Transform playerTarget, Transform summonTarget)
    {
        if (ownerTransform == null || characterController == null || runtimeStats == null || combatTarget == null)
        {
            lastHorizontalVelocity = Vector3.zero;
            return;
        }

        lastHorizontalVelocity = Vector3.zero;
        Vector3 direction = combatTarget.position - ownerTransform.position;
        direction.y = 0f;

        float distance = direction.magnitude;
        float desiredDistance = GetDesiredCombatDistance(combatTarget);

        if (enemyData != null && enemyData.AttackType == EnemyAttackType.RangedProjectile && distance < desiredDistance * 0.65f)
        {
            Vector3 retreatDirection = (-direction).normalized;
            Vector3 navigableRetreat = ResolveNavigableDirection(retreatDirection, distance, playerTarget, summonTarget);
            MoveHorizontally(navigableRetreat);
        }
        else if (distance > desiredDistance)
        {
            Vector3 moveDirection = GetMoveDirection(combatTarget, direction, distance, desiredDistance);
            Vector3 navigableDirection = ResolveNavigableDirection(moveDirection, distance - desiredDistance, playerTarget, summonTarget);
            MoveHorizontally(navigableDirection);
        }

        ApplyGravity();
    }

    public float GetContactAttackDistance(Transform combatTarget)
    {
        float selfRadius = characterController != null ? characterController.radius : 0.5f;

        if (cachedTargetRadius < 0f)
        {
            cachedTargetRadius = ResolveTargetRadius(combatTarget);
        }

        return Mathf.Max(0.65f, selfRadius + cachedTargetRadius + 0.18f);
    }

    private void InitializeMovementProfile()
    {
        float basePreferredDistance = enemyData != null ? enemyData.PreferredDistance : 1.4f;

        if (enemyData != null && enemyData.AttackType == EnemyAttackType.MeleeContact)
        {
            laneOffset = Random.Range(-meleeLaneOffsetStrength, meleeLaneOffsetStrength);
            personalPreferredDistance = Mathf.Max(
                0.6f,
                basePreferredDistance + Random.Range(-meleePreferredDistanceVariance, meleePreferredDistanceVariance));
            return;
        }

        laneOffset = 0f;
        personalPreferredDistance = basePreferredDistance;
    }

    private float GetDesiredCombatDistance(Transform combatTarget)
    {
        float configuredDistance = PreferredCombatDistance;

        if (enemyData != null && enemyData.AttackType == EnemyAttackType.RangedProjectile)
        {
            return configuredDistance;
        }

        return Mathf.Min(configuredDistance, Mathf.Max(0.45f, GetContactAttackDistance(combatTarget) * 0.85f));
    }

    private Vector3 GetMoveDirection(Transform combatTarget, Vector3 targetDirection, float distance, float desiredDistance)
    {
        Vector3 moveDirection = targetDirection.normalized;

        if (enemyData == null || enemyData.AttackType != EnemyAttackType.MeleeContact || Mathf.Abs(laneOffset) <= 0.01f)
        {
            return moveDirection;
        }

        Vector3 tangent = Vector3.Cross(Vector3.up, moveDirection).normalized;
        float laneWeight = Mathf.Clamp01((distance - desiredDistance) / 4f);
        Vector3 offsetTarget = combatTarget.position + tangent * laneOffset * laneWeight;
        Vector3 offsetDirection = offsetTarget - ownerTransform.position;
        offsetDirection.y = 0f;

        return offsetDirection.sqrMagnitude > 0.001f
            ? offsetDirection.normalized
            : moveDirection;
    }

    private Vector3 ResolveNavigableDirection(Vector3 desiredDirection, float remainingDistance, Transform playerTarget, Transform summonTarget)
    {
        if (desiredDirection.sqrMagnitude <= 0.001f)
        {
            return Vector3.zero;
        }

        Vector3 flattenedDirection = desiredDirection.normalized;

        if (Time.time >= nextProbeTime)
        {
            nextProbeTime = Time.time + ProbeInterval;
            float probeDistance = Mathf.Clamp(remainingDistance, 0.75f, obstacleProbeDistance);
            cachedAvoidanceAngleIndex = -1;

            if (!IsPathClear(flattenedDirection, probeDistance, playerTarget, summonTarget))
            {
                for (int i = 0; i < avoidanceAngles.Length; i++)
                {
                    Vector3 candidateDirection = Quaternion.Euler(0f, avoidanceAngles[i], 0f) * flattenedDirection;

                    if (IsPathClear(candidateDirection, probeDistance, playerTarget, summonTarget))
                    {
                        cachedAvoidanceAngleIndex = i;
                        break;
                    }
                }
            }
        }

        if (cachedAvoidanceAngleIndex >= 0)
        {
            return (Quaternion.Euler(0f, avoidanceAngles[cachedAvoidanceAngleIndex], 0f) * flattenedDirection).normalized;
        }

        return flattenedDirection;
    }

    private bool IsPathClear(Vector3 direction, float probeDistance, Transform playerTarget, Transform summonTarget)
    {
        if (direction.sqrMagnitude <= 0.001f || characterController == null)
        {
            return true;
        }

        Vector3 origin = ownerTransform.position + Vector3.up * obstacleProbeHeight;
        float probeRadius = Mathf.Max(0.1f, characterController.radius * obstacleProbeRadiusMultiplier);

        if (!Physics.SphereCast(origin, probeRadius, direction.normalized, out RaycastHit hit, probeDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            return true;
        }

        if (hit.collider == null)
        {
            return true;
        }

        Transform hitRoot = hit.collider.transform.root;

        if (hitRoot == ownerTransform.root)
        {
            return true;
        }

        if (playerTarget != null && hitRoot == playerTarget.root)
        {
            return true;
        }

        if (summonTarget != null && hitRoot == summonTarget.root)
        {
            return true;
        }

        return false;
    }

    private void MoveHorizontally(Vector3 direction)
    {
        if (direction.sqrMagnitude <= 0.001f)
        {
            return;
        }

        ownerTransform.forward = direction;
        lastHorizontalVelocity = direction * runtimeStats.GetValue(StatType.MoveSpeed);
        characterController.Move(lastHorizontalVelocity * Time.deltaTime);
    }

    private void ApplyGravity()
    {
        if (characterController.isGrounded && velocity.y < 0f)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    private static float ResolveTargetRadius(Transform targetTransform)
    {
        if (targetTransform == null)
        {
            return 0.5f;
        }

        CharacterController targetController = targetTransform.GetComponent<CharacterController>();

        if (targetController != null)
        {
            return targetController.radius;
        }

        Collider targetCollider = targetTransform.GetComponentInChildren<Collider>();

        if (targetCollider != null)
        {
            Vector3 extents = targetCollider.bounds.extents;
            return Mathf.Max(extents.x, extents.z);
        }

        return 0.5f;
    }
}
