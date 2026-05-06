using UnityEngine;

public static class DefaultRuntimePrefabFactory
{
    private static GameObject projectilePrefab;
    private static GameObject orbitBladePrefab;
    private static GameObject shockwavePrefab;
    private static GameObject experienceGemPrefab;
    private static GameObject enemyProjectilePrefab;
    private static GameObject modularProjectilePrefab;
    private static GameObject circularSweepHitboxPrefab;
    private static GameObject delayedAreaStrikePrefab;
    private static GameObject orbitalAttackPrefab;
    private static GameObject summonPrefab;
    private static GameObject beamVisualPrefab;

    public static void ResetCachedPrefabs()
    {
        DestroyCachedPrefab(ref projectilePrefab);
        DestroyCachedPrefab(ref orbitBladePrefab);
        DestroyCachedPrefab(ref shockwavePrefab);
        DestroyCachedPrefab(ref experienceGemPrefab);
        DestroyCachedPrefab(ref enemyProjectilePrefab);
        DestroyCachedPrefab(ref modularProjectilePrefab);
        DestroyCachedPrefab(ref circularSweepHitboxPrefab);
        DestroyCachedPrefab(ref delayedAreaStrikePrefab);
        DestroyCachedPrefab(ref orbitalAttackPrefab);
        DestroyCachedPrefab(ref summonPrefab);
        DestroyCachedPrefab(ref beamVisualPrefab);
    }

    public static GameObject GetProjectilePrefab()
    {
        if (projectilePrefab == null)
        {
            projectilePrefab = CreateProjectilePrefab();
        }

        return projectilePrefab;
    }

    public static GameObject GetOrbitBladePrefab()
    {
        if (orbitBladePrefab == null)
        {
            orbitBladePrefab = CreateOrbitBladePrefab();
        }

        return orbitBladePrefab;
    }

    public static GameObject GetShockwavePrefab()
    {
        if (shockwavePrefab == null)
        {
            shockwavePrefab = CreateShockwavePrefab();
        }

        return shockwavePrefab;
    }

    public static GameObject GetExperienceGemPrefab()
    {
        if (experienceGemPrefab == null)
        {
            experienceGemPrefab = CreateExperienceGemPrefab();
        }

        return experienceGemPrefab;
    }

    public static GameObject GetEnemyProjectilePrefab()
    {
        if (enemyProjectilePrefab == null)
        {
            enemyProjectilePrefab = CreateEnemyProjectilePrefab();
        }

        return enemyProjectilePrefab;
    }

    public static GameObject GetModularProjectilePrefab()
    {
        if (modularProjectilePrefab == null)
        {
            modularProjectilePrefab = CreateModularProjectilePrefab();
        }

        return modularProjectilePrefab;
    }

    public static GameObject GetCircularSweepHitboxPrefab()
    {
        if (circularSweepHitboxPrefab == null)
        {
            circularSweepHitboxPrefab = CreateRuntimeBehaviourPrefab<RuntimeCircularSweepHitbox>("DefaultCircularSweepHitboxPrefab");
        }

        return circularSweepHitboxPrefab;
    }

    public static GameObject GetDelayedAreaStrikePrefab()
    {
        if (delayedAreaStrikePrefab == null)
        {
            delayedAreaStrikePrefab = CreateRuntimeBehaviourPrefab<RuntimeDelayedAreaStrike>("DefaultDelayedAreaStrikePrefab");
        }

        return delayedAreaStrikePrefab;
    }

    public static GameObject GetOrbitalAttackPrefab()
    {
        if (orbitalAttackPrefab == null)
        {
            orbitalAttackPrefab = CreateRuntimeBehaviourPrefab<RuntimeOrbitalAttack>("DefaultOrbitalAttackPrefab");
        }

        return orbitalAttackPrefab;
    }

    public static GameObject GetSummonPrefab()
    {
        if (summonPrefab == null)
        {
            summonPrefab = CreateSummonPrefab();
        }

        return summonPrefab;
    }

    public static GameObject GetBeamVisualPrefab()
    {
        if (beamVisualPrefab == null)
        {
            beamVisualPrefab = CreateBeamVisualPrefab();
        }

        return beamVisualPrefab;
    }

    private static GameObject CreateProjectilePrefab()
    {
        GameObject prefab = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        prefab.name = "DefaultProjectilePrefab";
        prefab.transform.localScale = Vector3.one * 0.3f;
        Collider collider = prefab.GetComponent<Collider>();
        collider.isTrigger = true;

        Rigidbody rigidbodyComponent = prefab.AddComponent<Rigidbody>();
        rigidbodyComponent.useGravity = false;
        rigidbodyComponent.isKinematic = true;

        Renderer rendererComponent = prefab.GetComponent<Renderer>();
        rendererComponent.material.color = new Color(1f, 0.85f, 0.2f);

        prefab.AddComponent<DataDrivenProjectile>();
        prefab.AddComponent<PoolableObject>();
        prefab.SetActive(false);
        prefab.hideFlags = HideFlags.HideAndDontSave;
        return prefab;
    }

    private static GameObject CreateOrbitBladePrefab()
    {
        GameObject prefab = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        prefab.name = "DefaultOrbitBladePrefab";
        prefab.transform.localScale = Vector3.one * 0.45f;
        Collider collider = prefab.GetComponent<Collider>();
        collider.isTrigger = true;

        Rigidbody rigidbodyComponent = prefab.AddComponent<Rigidbody>();
        rigidbodyComponent.useGravity = false;
        rigidbodyComponent.isKinematic = true;

        Renderer rendererComponent = prefab.GetComponent<Renderer>();
        rendererComponent.material.color = new Color(0.35f, 0.85f, 1f);

        prefab.AddComponent<DataDrivenOrbitingBladeHitbox>();
        prefab.AddComponent<PoolableObject>();
        prefab.SetActive(false);
        prefab.hideFlags = HideFlags.HideAndDontSave;
        return prefab;
    }

    private static GameObject CreateShockwavePrefab()
    {
        GameObject prefab = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        prefab.name = "DefaultShockwavePrefab";
        Collider collider = prefab.GetComponent<Collider>();

        if (collider != null)
        {
            Object.DestroyImmediate(collider);
        }

        Renderer rendererComponent = prefab.GetComponent<Renderer>();
        rendererComponent.material.color = new Color(1f, 0.55f, 0.15f, 0.45f);

        prefab.AddComponent<ShockwavePulseVisual>();
        prefab.AddComponent<PoolableObject>();
        prefab.SetActive(false);
        prefab.hideFlags = HideFlags.HideAndDontSave;
        return prefab;
    }

    private static GameObject CreateExperienceGemPrefab()
    {
        GameObject prefab = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        prefab.name = "DefaultExperienceGemPrefab";
        prefab.transform.localScale = Vector3.one * 0.35f;
        Renderer rendererComponent = prefab.GetComponent<Renderer>();
        rendererComponent.material.color = new Color(0.25f, 0.8f, 1f);

        SphereCollider collider = prefab.GetComponent<SphereCollider>();
        collider.isTrigger = true;

        Rigidbody rigidbodyComponent = prefab.AddComponent<Rigidbody>();
        rigidbodyComponent.useGravity = false;
        rigidbodyComponent.isKinematic = true;

        prefab.AddComponent<ExperienceGem>();
        prefab.AddComponent<PoolableObject>();
        prefab.SetActive(false);
        prefab.hideFlags = HideFlags.HideAndDontSave;
        return prefab;
    }

    private static GameObject CreateEnemyProjectilePrefab()
    {
        GameObject prefab = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        prefab.name = "DefaultEnemyProjectilePrefab";
        prefab.transform.localScale = Vector3.one * 0.4f;

        SphereCollider collider = prefab.GetComponent<SphereCollider>();
        collider.isTrigger = true;

        Rigidbody rigidbodyComponent = prefab.AddComponent<Rigidbody>();
        rigidbodyComponent.useGravity = false;
        rigidbodyComponent.isKinematic = true;

        Renderer rendererComponent = prefab.GetComponent<Renderer>();
        rendererComponent.material.color = new Color(0.35f, 1f, 0.3f);

        prefab.AddComponent<EnemyProjectile>();
        prefab.AddComponent<PoolableObject>();
        prefab.SetActive(false);
        prefab.hideFlags = HideFlags.HideAndDontSave;
        return prefab;
    }

    private static GameObject CreateModularProjectilePrefab()
    {
        GameObject prefab = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        prefab.name = "DefaultModularProjectilePrefab";
        prefab.transform.localScale = Vector3.one * 0.32f;

        SphereCollider collider = prefab.GetComponent<SphereCollider>();
        collider.isTrigger = true;

        Rigidbody rigidbodyComponent = prefab.AddComponent<Rigidbody>();
        rigidbodyComponent.useGravity = false;
        rigidbodyComponent.isKinematic = true;

        Renderer rendererComponent = prefab.GetComponent<Renderer>();
        rendererComponent.material.color = new Color(0.9f, 0.9f, 1f);

        prefab.AddComponent<ModularProjectile>();
        prefab.AddComponent<PoolableObject>();
        prefab.SetActive(false);
        prefab.hideFlags = HideFlags.HideAndDontSave;
        return prefab;
    }

    private static GameObject CreateRuntimeBehaviourPrefab<T>(string prefabName) where T : Component
    {
        GameObject prefab = new(prefabName);
        prefab.AddComponent<T>();
        prefab.AddComponent<PoolableObject>();
        prefab.SetActive(false);
        prefab.hideFlags = HideFlags.HideAndDontSave;
        return prefab;
    }

    private static GameObject CreateSummonPrefab()
    {
        GameObject prefab = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        prefab.name = "DefaultSummonPrefab";
        prefab.transform.localScale = new Vector3(0.65f, 1f, 0.65f);

        Renderer rendererComponent = prefab.GetComponent<Renderer>();
        rendererComponent.material.color = new Color(0.45f, 0.42f, 0.38f, 1f);

        prefab.AddComponent<RuntimeSummonedMinion>();
        prefab.AddComponent<PoolableObject>();
        prefab.SetActive(false);
        prefab.hideFlags = HideFlags.HideAndDontSave;
        return prefab;
    }

    private static GameObject CreateBeamVisualPrefab()
    {
        GameObject prefab = GameObject.CreatePrimitive(PrimitiveType.Cube);
        prefab.name = "DefaultBeamVisualPrefab";

        Collider collider = prefab.GetComponent<Collider>();

        if (collider != null)
        {
            Object.DestroyImmediate(collider);
        }

        Renderer rendererComponent = prefab.GetComponent<Renderer>();
        rendererComponent.material.color = new Color(0.7f, 0.85f, 1f, 0.75f);

        prefab.AddComponent<RuntimeTimedDestroy>();
        prefab.AddComponent<PoolableObject>();
        prefab.SetActive(false);
        prefab.hideFlags = HideFlags.HideAndDontSave;
        return prefab;
    }

    private static void DestroyCachedPrefab(ref GameObject prefab)
    {
        if (prefab == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Object.Destroy(prefab);
        }
        else
        {
            Object.DestroyImmediate(prefab);
        }

        prefab = null;
    }
}
