using UnityEngine;

public class RuntimeSummonedMinion : MonoBehaviour
{
    private Transform owner;
    private StatusEffectData[] statuses;
    private float duration;
    private float moveSpeed;
    private float attackRange;
    private float attackInterval;
    private int damage;
    private float lifeTimer;
    private float attackTimer;

    public void Initialize(Transform ownerTransform, float activeDuration, float speed, float range, float interval, int summonDamage, StatusEffectData[] summonStatuses)
    {
        owner = ownerTransform;
        duration = Mathf.Max(0.1f, activeDuration);
        moveSpeed = Mathf.Max(0f, speed);
        attackRange = Mathf.Max(0.1f, range);
        attackInterval = Mathf.Max(0.05f, interval);
        damage = Mathf.Max(0, summonDamage);
        statuses = summonStatuses;
        lifeTimer = 0f;
        attackTimer = attackInterval;

        Collider collider = GetComponent<Collider>();

        if (collider != null)
        {
            collider.isTrigger = true;
        }
    }

    private void Update()
    {
        lifeTimer += Time.deltaTime;

        if (lifeTimer >= duration)
        {
            Destroy(gameObject);
            return;
        }

        EnemyAgent target = EnemyRegistry.GetClosestEnemy(transform.position, 24f);

        if (target == null)
        {
            FollowOwnerLoosely();
            return;
        }

        MoveToward(target.transform.position);
        TryAttack(target);
    }

    private void FollowOwnerLoosely()
    {
        if (owner == null)
        {
            return;
        }

        Vector3 toOwner = owner.position - transform.position;
        toOwner.y = 0f;

        if (toOwner.sqrMagnitude <= 4f)
        {
            return;
        }

        transform.position += toOwner.normalized * moveSpeed * Time.deltaTime;
    }

    private void MoveToward(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= attackRange * attackRange)
        {
            return;
        }

        Vector3 movement = direction.normalized * moveSpeed * Time.deltaTime;
        transform.position += movement;
        transform.forward = direction.normalized;
    }

    private void TryAttack(EnemyAgent target)
    {
        if (target == null)
        {
            return;
        }

        Vector3 delta = target.transform.position - transform.position;
        delta.y = 0f;

        if (delta.sqrMagnitude > attackRange * attackRange)
        {
            return;
        }

        attackTimer += Time.deltaTime;

        if (attackTimer < attackInterval)
        {
            return;
        }

        attackTimer = 0f;

        if (damage > 0)
        {
            target.TakeDamage(damage);
        }

        target.ApplyStatuses(statuses);
    }
}
