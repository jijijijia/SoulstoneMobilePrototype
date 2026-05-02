using System.IO;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class ModularCombatContentInstaller
{
    private const string InstalledThisSessionKey = "Soulstone.ModularCombatInstaller.Ran";

    static ModularCombatContentInstaller()
    {
        EditorApplication.delayCall += EnsureInstalled;
    }

    [MenuItem("Soulstone/Install Modular Combat Sample")]
    public static void EnsureInstalled()
    {
        if (SessionState.GetBool(InstalledThisSessionKey, false))
        {
            return;
        }

        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            EditorApplication.delayCall += EnsureInstalled;
            return;
        }

        try
        {
            EnsureFolder("Assets/Data/AttackModules");
            EnsureFolder("Assets/Data/AttackModules/Runtime");
            EnsureFolder("Assets/Data/AttackModules/Targeting");
            EnsureFolder("Assets/Data/AttackModules/Delivery");
            EnsureFolder("Assets/Data/AttackModules/Impact");
            EnsureFolder("Assets/Data/AttackModules/Definitions");

            ModularAttackSkillRuntimeDefinition modularRuntime = LoadOrCreateAsset<ModularAttackSkillRuntimeDefinition>(
                "Assets/Data/AttackModules/Runtime/ModularAttackRuntimeDefinition.asset");
            ConfigurableSkillRuntimeDefinition configurableRuntime = LoadOrCreateAsset<ConfigurableSkillRuntimeDefinition>(
                "Assets/Data/AttackModules/Runtime/ConfigurableSkillRuntimeDefinition.asset");

            NearestEnemyTargetingDefinition nearestTargeting = LoadOrCreateAsset<NearestEnemyTargetingDefinition>(
                "Assets/Data/AttackModules/Targeting/Targeting_NearestEnemy.asset");
            RandomEnemyTargetingDefinition randomTargeting = LoadOrCreateAsset<RandomEnemyTargetingDefinition>(
                "Assets/Data/AttackModules/Targeting/Targeting_RandomEnemies.asset");
            ProjectileAttackDeliveryDefinition projectileDelivery = LoadOrCreateAsset<ProjectileAttackDeliveryDefinition>(
                "Assets/Data/AttackModules/Delivery/Delivery_Projectile.asset");
            AreaPulseAttackDeliveryDefinition areaDelivery = LoadOrCreateAsset<AreaPulseAttackDeliveryDefinition>(
                "Assets/Data/AttackModules/Delivery/Delivery_AreaPulse.asset");
            DamageAttackImpactDefinition spearDamage = LoadOrCreateAsset<DamageAttackImpactDefinition>(
                "Assets/Data/AttackModules/Impact/Impact_Damage_Spear.asset");
            DamageAttackImpactDefinition stormDamage = LoadOrCreateAsset<DamageAttackImpactDefinition>(
                "Assets/Data/AttackModules/Impact/Impact_Damage_Storm.asset");

            ConfigureNearestTargeting(nearestTargeting);
            ConfigureRandomTargeting(randomTargeting);
            ConfigureProjectileDelivery(projectileDelivery);
            ConfigureAreaDelivery(areaDelivery);
            ConfigureDamageImpact(spearDamage, 24f, 7f, 1f);
            ConfigureDamageImpact(stormDamage, 34f, 9f, 0.8f);

            AttackDefinition stoneSpearAttack = LoadOrCreateAsset<AttackDefinition>(
                "Assets/Data/AttackModules/Definitions/Attack_StoneSpear.asset");
            AttackDefinition stormCallAttack = LoadOrCreateAsset<AttackDefinition>(
                "Assets/Data/AttackModules/Definitions/Attack_StormCall.asset");

            ConfigureAttackDefinition(stoneSpearAttack, "stone_spear", "Stone Spear", 0.85f, 0.04f, nearestTargeting, projectileDelivery, spearDamage);
            ConfigureAttackDefinition(stormCallAttack, "storm_call", "Storm Call", 2.2f, 0.08f, randomTargeting, areaDelivery, stormDamage);

            SkillData stoneSpearSkill = LoadOrCreateAsset<SkillData>("Assets/Data/Skills/Skill_StoneSpear.asset");
            SkillData stormCallSkill = LoadOrCreateAsset<SkillData>("Assets/Data/Skills/Skill_StormCall.asset");
            SkillData configurableTemplateSkill = LoadOrCreateAsset<SkillData>("Assets/Data/Skills/Skill_Template_Configurable.asset");

            ConfigureSkill(stoneSpearSkill,
                "stone_spear",
                "Stone Spear",
                "Launch heavy spears at the nearest enemies",
                modularRuntime,
                new[] { "Projectile", "Physical", "Constructed" },
                stoneSpearAttack);
            ConfigureSkill(stormCallSkill,
                "storm_call",
                "Storm Call",
                "Call random lightning strikes onto enemies across the arena",
                modularRuntime,
                new[] { "Area", "Lightning", "Magic", "Constructed" },
                stormCallAttack);
            ConfigureConfigurableTemplateSkill(configurableTemplateSkill, configurableRuntime);

            WeaponData starterWeapon = AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/Data/Weapons/Weapon_Starter.asset");

            if (starterWeapon != null)
            {
                EnsureWeaponContainsSkill(starterWeapon, stoneSpearSkill);
                EnsureWeaponContainsSkill(starterWeapon, stormCallSkill);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            SessionState.SetBool(InstalledThisSessionKey, true);
        }
        catch
        {
            SessionState.SetBool(InstalledThisSessionKey, false);
            throw;
        }
    }

    private static void ConfigureNearestTargeting(NearestEnemyTargetingDefinition asset)
    {
        SerializedObject serializedObject = new(asset);
        serializedObject.FindProperty("targetCount").intValue = 2;
        serializedObject.FindProperty("maxDistance").floatValue = 13f;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(asset);
    }

    private static void ConfigureRandomTargeting(RandomEnemyTargetingDefinition asset)
    {
        SerializedObject serializedObject = new(asset);
        serializedObject.FindProperty("targetCount").intValue = 3;
        serializedObject.FindProperty("maxDistance").floatValue = 16f;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(asset);
    }

    private static void ConfigureProjectileDelivery(ProjectileAttackDeliveryDefinition asset)
    {
        SerializedObject serializedObject = new(asset);
        serializedObject.FindProperty("projectileSpeed").floatValue = 15f;
        serializedObject.FindProperty("projectileLifetime").floatValue = 2.4f;
        serializedObject.FindProperty("spawnHeight").floatValue = 0.8f;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(asset);
    }

    private static void ConfigureAreaDelivery(AreaPulseAttackDeliveryDefinition asset)
    {
        SerializedObject serializedObject = new(asset);
        serializedObject.FindProperty("radius").floatValue = 2.8f;
        serializedObject.FindProperty("visualLifetime").floatValue = 0.35f;
        serializedObject.FindProperty("visualHeight").floatValue = 0.1f;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(asset);
    }

    private static void ConfigureDamageImpact(DamageAttackImpactDefinition asset, float baseDamage, float damagePerRank, float ownerDamageMultiplier)
    {
        SerializedObject serializedObject = new(asset);
        serializedObject.FindProperty("baseDamage").floatValue = baseDamage;
        serializedObject.FindProperty("damagePerRank").floatValue = damagePerRank;
        serializedObject.FindProperty("ownerDamageMultiplier").floatValue = ownerDamageMultiplier;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(asset);
    }

    private static void ConfigureAttackDefinition(
        AttackDefinition asset,
        string attackId,
        string displayName,
        float baseCooldown,
        float cooldownReductionPerRank,
        AttackTargetingDefinition targeting,
        AttackDeliveryDefinition delivery,
        params AttackImpactDefinition[] impacts)
    {
        SerializedObject serializedObject = new(asset);
        serializedObject.FindProperty("attackId").stringValue = attackId;
        serializedObject.FindProperty("displayName").stringValue = displayName;
        serializedObject.FindProperty("baseCooldown").floatValue = baseCooldown;
        serializedObject.FindProperty("cooldownReductionPerRank").floatValue = cooldownReductionPerRank;
        serializedObject.FindProperty("targeting").objectReferenceValue = targeting;
        serializedObject.FindProperty("delivery").objectReferenceValue = delivery;

        SerializedProperty impactsProperty = serializedObject.FindProperty("impacts");
        impactsProperty.arraySize = impacts.Length;

        for (int i = 0; i < impacts.Length; i++)
        {
            impactsProperty.GetArrayElementAtIndex(i).objectReferenceValue = impacts[i];
        }

        serializedObject.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(asset);
    }

    private static void ConfigureSkill(
        SkillData skill,
        string skillId,
        string displayName,
        string description,
        SkillRuntimeDefinition runtimeDefinition,
        string[] tags,
        params AttackDefinition[] attacks)
    {
        SerializedObject serializedObject = new(skill);
        serializedObject.FindProperty("skillId").stringValue = skillId;
        serializedObject.FindProperty("displayName").stringValue = displayName;
        serializedObject.FindProperty("description").stringValue = description;
        serializedObject.FindProperty("runtimeDefinition").objectReferenceValue = runtimeDefinition;
        serializedObject.FindProperty("globalSkill").boolValue = false;
        serializedObject.FindProperty("maxRank").intValue = 5;
        serializedObject.FindProperty("visualPrefab").objectReferenceValue = null;
        serializedObject.FindProperty("icon").objectReferenceValue = null;

        SerializedProperty tagsProperty = serializedObject.FindProperty("tags");
        tagsProperty.arraySize = tags.Length;

        for (int i = 0; i < tags.Length; i++)
        {
            tagsProperty.GetArrayElementAtIndex(i).stringValue = tags[i];
        }

        SerializedProperty statusesProperty = serializedObject.FindProperty("appliedStatuses");
        statusesProperty.arraySize = 0;

        SerializedProperty attacksProperty = serializedObject.FindProperty("attackDefinitions");
        attacksProperty.arraySize = attacks.Length;

        for (int i = 0; i < attacks.Length; i++)
        {
            attacksProperty.GetArrayElementAtIndex(i).objectReferenceValue = attacks[i];
        }

        SerializedProperty parametersProperty = serializedObject.FindProperty("parameters");
        parametersProperty.arraySize = 0;

        serializedObject.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(skill);
    }

    private static void ConfigureConfigurableTemplateSkill(SkillData skill, SkillRuntimeDefinition runtimeDefinition)
    {
        SerializedObject serializedObject = new(skill);
        serializedObject.FindProperty("skillId").stringValue = "template_configurable";
        serializedObject.FindProperty("displayName").stringValue = "Template Configurable Skill";
        serializedObject.FindProperty("description").stringValue = "Duplicate this asset and tune the attack blocks below.";
        serializedObject.FindProperty("runtimeDefinition").objectReferenceValue = runtimeDefinition;
        serializedObject.FindProperty("globalSkill").boolValue = false;
        serializedObject.FindProperty("maxRank").intValue = 5;
        serializedObject.FindProperty("visualPrefab").objectReferenceValue = null;
        serializedObject.FindProperty("icon").objectReferenceValue = null;

        SerializedProperty tagsProperty = serializedObject.FindProperty("tags");
        tagsProperty.arraySize = 2;
        tagsProperty.GetArrayElementAtIndex(0).stringValue = "Template";
        tagsProperty.GetArrayElementAtIndex(1).stringValue = "Configurable";

        SerializedProperty statusesProperty = serializedObject.FindProperty("appliedStatuses");
        statusesProperty.arraySize = 0;

        SerializedProperty attackDefinitionsProperty = serializedObject.FindProperty("attackDefinitions");
        attackDefinitionsProperty.arraySize = 0;

        SerializedProperty configurableProperty = serializedObject.FindProperty("configurableAttacks");
        configurableProperty.arraySize = 1;

        SerializedProperty firstAttack = configurableProperty.GetArrayElementAtIndex(0);
        firstAttack.FindPropertyRelative("attackId").stringValue = "template_projectile";
        firstAttack.FindPropertyRelative("baseCooldown").floatValue = 0.9f;
        firstAttack.FindPropertyRelative("cooldownReductionPerRank").floatValue = 0.05f;
        firstAttack.FindPropertyRelative("targetingMode").enumValueIndex = (int)ConfigurableSkillTargetingMode.NearestEnemy;
        firstAttack.FindPropertyRelative("targetCount").intValue = 1;
        firstAttack.FindPropertyRelative("maxDistance").floatValue = 12f;
        firstAttack.FindPropertyRelative("deliveryMode").enumValueIndex = (int)ConfigurableSkillDeliveryMode.Projectile;
        firstAttack.FindPropertyRelative("projectileSpeed").floatValue = 14f;
        firstAttack.FindPropertyRelative("projectileLifetime").floatValue = 2.5f;
        firstAttack.FindPropertyRelative("spawnHeight").floatValue = 0.75f;
        firstAttack.FindPropertyRelative("areaRadius").floatValue = 3f;
        firstAttack.FindPropertyRelative("areaVisualLifetime").floatValue = 0.2f;
        firstAttack.FindPropertyRelative("baseDamage").floatValue = 20f;
        firstAttack.FindPropertyRelative("damagePerRank").floatValue = 6f;
        firstAttack.FindPropertyRelative("ownerDamageMultiplier").floatValue = 1f;

        SerializedProperty parametersProperty = serializedObject.FindProperty("parameters");
        parametersProperty.arraySize = 0;

        serializedObject.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(skill);
    }

    private static void EnsureWeaponContainsSkill(WeaponData weaponData, SkillData skill)
    {
        SerializedObject serializedObject = new(weaponData);
        SerializedProperty poolProperty = serializedObject.FindProperty("uniqueSkillPool");

        for (int i = 0; i < poolProperty.arraySize; i++)
        {
            if (poolProperty.GetArrayElementAtIndex(i).objectReferenceValue == skill)
            {
                return;
            }
        }

        int index = poolProperty.arraySize;
        poolProperty.arraySize += 1;
        poolProperty.GetArrayElementAtIndex(index).objectReferenceValue = skill;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(weaponData);
    }

    private static T LoadOrCreateAsset<T>(string path) where T : ScriptableObject
    {
        T asset = AssetDatabase.LoadAssetAtPath<T>(path);

        if (asset != null)
        {
            return asset;
        }

        asset = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(asset, path);
        return asset;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
        {
            return;
        }

        string parent = Path.GetDirectoryName(path)?.Replace("\\", "/");
        string folderName = Path.GetFileName(path);

        if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
        {
            EnsureFolder(parent);
        }

        AssetDatabase.CreateFolder(parent, folderName);
    }
}
