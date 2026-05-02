using UnityEngine;

public class DataDrivenProjectile : MonoBehaviour, IPoolable
{
    private Vector3 moveDirection;
    private float moveSpeed;
    private float lifeTime;
    private int damage;
    private StatusEffectData[] statusPayload;
    private float lifeTimer;
    private bool isActive;

    public void Initialize(Vector3 direction, float speed, float duration, int projectileDamage, StatusEffectData[] statuses)
    {
        moveDirection = direction.normalized;
        moveSpeed = speed;
        lifeTime = duration;
        damage = projectileDamage;
        statusPayload = statuses;
        lifeTimer = 0f;
        isActive = true;
    }

    private void Update()
    {
        if (!isActive)
        {
            return;
        }

        lifeTimer += Time.deltaTime;

        if (lifeTimer >= lifeTime)
        {
            PoolManager.Release(gameObject);
            return;
        }

        transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        EnemyAgent enemy = other.GetComponent<EnemyAgent>();

        if (enemy == null)
        {
            return;
        }

        enemy.TakeDamage(damage);
        enemy.ApplyStatuses(statusPayload);
        PoolManager.Release(gameObject);
    }

    public void OnTakenFromPool()
    {
        lifeTimer = 0f;
        isActive = false;
        moveDirection = Vector3.zero;
        statusPayload = null;
    }

    public void OnReturnedToPool()
    {
        lifeTimer = 0f;
        isActive = false;
        moveDirection = Vector3.zero;
        statusPayload = null;
    }
}
