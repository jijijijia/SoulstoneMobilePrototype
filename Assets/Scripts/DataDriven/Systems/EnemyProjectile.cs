using UnityEngine;

public class EnemyProjectile : MonoBehaviour, IPoolable
{
    private Vector3 moveDirection;
    private float moveSpeed;
    private float lifeTime;
    private float damage;
    private StatusEffectData[] statusPayload;
    private float lifeTimer;

    public void Initialize(Vector3 direction, float speed, float duration, float projectileDamage, StatusEffectData[] statuses)
    {
        moveDirection = direction.sqrMagnitude > 0.001f ? direction.normalized : Vector3.forward;
        moveSpeed = speed;
        lifeTime = duration;
        damage = projectileDamage;
        statusPayload = statuses;
        lifeTimer = 0f;

        if (moveDirection.sqrMagnitude > 0.001f)
        {
            transform.forward = moveDirection;
        }
    }

    private void Update()
    {
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
        CharacterSystem player = other.GetComponent<CharacterSystem>();

        if (player == null)
        {
            player = other.GetComponentInParent<CharacterSystem>();
        }

        if (player == null)
        {
            return;
        }

        player.TakeDamage(damage);
        player.StatusController?.ApplyStatuses(statusPayload);
        PoolManager.Release(gameObject);
    }

    public void OnTakenFromPool()
    {
        lifeTimer = 0f;
    }

    public void OnReturnedToPool()
    {
        moveDirection = Vector3.zero;
        moveSpeed = 0f;
        lifeTime = 0f;
        damage = 0f;
        statusPayload = null;
        lifeTimer = 0f;
    }
}
