using UnityEngine;

public class ExperienceGem : MonoBehaviour, IPoolable
{
    [SerializeField] private int experienceValue = 1;
    [SerializeField] private float rotationSpeed = 90f;

    private CharacterSystem characterSystem;
    private ProgressionSystem progressionSystem;
    private bool attemptedFallbackResolve;

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

        ResolveTargetReferencesOnce();

        if (characterSystem == null || progressionSystem == null)
        {
            return;
        }

        Vector3 playerPosition = characterSystem.transform.position;
        playerPosition.y = transform.position.y;

        float pickupRadius = characterSystem.RuntimeStats != null
            ? characterSystem.RuntimeStats.GetValue(StatType.PickupRadius)
            : 0f;

        float pickupRadiusSqr = pickupRadius * pickupRadius;

        if ((transform.position - playerPosition).sqrMagnitude <= pickupRadiusSqr)
        {
            progressionSystem.AddExperience(experienceValue);
            PoolManager.Release(gameObject);
        }
    }

    public void OnTakenFromPool()
    {
        characterSystem = null;
        progressionSystem = null;
        attemptedFallbackResolve = false;
    }

    public void OnReturnedToPool()
    {
        characterSystem = null;
        progressionSystem = null;
        attemptedFallbackResolve = false;
    }

    private void ResolveTargetReferencesOnce()
    {
        if (attemptedFallbackResolve || characterSystem != null)
        {
            return;
        }

        attemptedFallbackResolve = true;

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
