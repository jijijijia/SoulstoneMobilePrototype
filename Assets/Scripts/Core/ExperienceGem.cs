using UnityEngine;

public class ExperienceGem : MonoBehaviour, IPoolable
{
    [SerializeField] private int experienceValue = 1;
    [SerializeField] private float rotationSpeed = 90f;

    private CharacterSystem characterSystem;
    private ProgressionSystem progressionSystem;

    public void SetCollector(CharacterSystem targetCharacter)
    {
        characterSystem = targetCharacter;
        progressionSystem = targetCharacter != null ? targetCharacter.ProgressionSystem : null;
    }

    public void SetExperienceValue(int value)
    {
        experienceValue = Mathf.Max(1, value);
    }

    private void Update()
    {
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f, Space.World);

        ResolveTargetReferences();

        if (characterSystem == null || progressionSystem == null)
        {
            return;
        }

        Vector3 playerPosition = characterSystem.transform.position;
        playerPosition.y = transform.position.y;

        float distanceToPlayer = Vector3.Distance(transform.position, playerPosition);
        float pickupRadius = characterSystem.RuntimeStats != null
            ? characterSystem.RuntimeStats.GetValue(StatType.PickupRadius)
            : 0f;

        if (distanceToPlayer <= pickupRadius)
        {
            progressionSystem.AddExperience(experienceValue);
            PoolManager.Release(gameObject);
        }
    }

    public void OnTakenFromPool()
    {
        characterSystem = null;
        progressionSystem = null;
    }

    public void OnReturnedToPool()
    {
        characterSystem = null;
        progressionSystem = null;
    }

    private void ResolveTargetReferences()
    {
        if (characterSystem == null)
        {
            characterSystem = FindFirstObjectByType<CharacterSystem>();
        }

        if (characterSystem != null && progressionSystem == null)
        {
            progressionSystem = characterSystem.ProgressionSystem;
        }
    }
}
