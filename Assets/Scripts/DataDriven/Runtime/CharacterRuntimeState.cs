using System;

public class CharacterRuntimeState
{
    private float currentHealth;
    private float maxHealth;
    private int spentTotems;
    private bool isDead;

    public event Action<float, float> HealthChanged;
    public event Action Died;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsDead => isDead;
    public int SpentTotems => spentTotems;

    public void Reset(float newMaxHealth)
    {
        maxHealth = Math.Max(0f, newMaxHealth);
        currentHealth = maxHealth;
        spentTotems = 0;
        isDead = false;
        HealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void ReconcileMaxHealth(float newMaxHealth)
    {
        float previousMaxHealth = maxHealth;
        maxHealth = Math.Max(0f, newMaxHealth);

        if (maxHealth > previousMaxHealth)
        {
            currentHealth = Math.Min(maxHealth, currentHealth + (maxHealth - previousMaxHealth));
        }
        else
        {
            currentHealth = Math.Min(currentHealth, maxHealth);
        }

        HealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public bool ApplyDamage(float damage, int availableTotems, float reviveHealthPercent)
    {
        if (isDead)
        {
            return true;
        }

        currentHealth = Math.Max(0f, currentHealth - damage);

        if (currentHealth <= 0f)
        {
            int remainingTotems = Math.Max(0, availableTotems - spentTotems);

            if (remainingTotems > 0)
            {
                spentTotems++;
                currentHealth = Math.Max(1f, maxHealth * reviveHealthPercent);
                HealthChanged?.Invoke(currentHealth, maxHealth);
                return false;
            }

            currentHealth = 0f;
            isDead = true;
            HealthChanged?.Invoke(currentHealth, maxHealth);
            Died?.Invoke();
            return true;
        }

        HealthChanged?.Invoke(currentHealth, maxHealth);
        return false;
    }

    public void RestoreHealth(float amount)
    {
        if (isDead)
        {
            return;
        }

        currentHealth = Math.Min(maxHealth, currentHealth + amount);
        HealthChanged?.Invoke(currentHealth, maxHealth);
    }
}
