using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class HeroCombatContentInstaller
{
    private const string InstalledThisSessionKey = "Soulstone.HeroCombatContentInstaller.Ran";
    private const string InstalledPersistentlyKey = "Soulstone.HeroCombatContentInstaller.Installed";
    private const string RosterPath = "Assets/Resources/CharacterRoster.asset";
    private const string RuntimePath = "Assets/Data/AttackModules/Runtime/ModularAttackRuntimeDefinition.asset";

    static HeroCombatContentInstaller()
    {
        EditorApplication.delayCall += EnsureInstalled;
    }

    [MenuItem("Soulstone/Install Hero Combat Content Pack")]
    public static void EnsureInstalled()
    {
        if (CoreAssetsExist())
        {
            EditorPrefs.SetBool(InstalledPersistentlyKey, true);
            SessionState.SetBool(InstalledThisSessionKey, true);
            return;
        }

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
            Install();
            EditorPrefs.SetBool(InstalledPersistentlyKey, true);
            SessionState.SetBool(InstalledThisSessionKey, true);
        }
        catch
        {
            SessionState.SetBool(InstalledThisSessionKey, false);
            throw;
        }
    }

    [MenuItem("Soulstone/Reinstall Hero Combat Content Pack")]
    public static void Reinstall()
    {
        if (CoreAssetsExist())
        {
            Debug.Log("Hero combat content already exists. Reinstall skipped to avoid overwriting edited content.");
            return;
        }

        Install();
        EditorPrefs.SetBool(InstalledPersistentlyKey, true);
        SessionState.SetBool(InstalledThisSessionKey, true);
    }

    private static void Install()
    {
        EnsureFolder("Assets/Data/Characters");
        EnsureFolder("Assets/Data/Weapons");
        EnsureFolder("Assets/Data/Skills");
        EnsureFolder("Assets/Data/Upgrades");
        EnsureFolder("Assets/Data/AttackModules/Targeting");
        EnsureFolder("Assets/Data/AttackModules/Delivery");
        EnsureFolder("Assets/Data/AttackModules/Impact");
        EnsureFolder("Assets/Data/AttackModules/Definitions");

        ModularAttackSkillRuntimeDefinition runtime = LoadOrCreateAsset<ModularAttackSkillRuntimeDefinition>(RuntimePath);

        NearestEnemyTargetingDefinition nearestTargeting = LoadOrCreateAsset<NearestEnemyTargetingDefinition>("Assets/Data/AttackModules/Targeting/Targeting_NearestEnemy.asset");
        RandomEnemyTargetingDefinition randomTargeting = LoadOrCreateAsset<RandomEnemyTargetingDefinition>("Assets/Data/AttackModules/Targeting/Targeting_RandomEnemies.asset");
        SelfPositionTargetingDefinition selfTargeting = LoadOrCreateAsset<SelfPositionTargetingDefinition>("Assets/Data/AttackModules/Targeting/Targeting_SelfPosition.asset");

        ConfigureNearest(nearestTargeting, 1, 16f);
        ConfigureRandom(randomTargeting, 1, 18f);

        ProjectileAttackDeliveryDefinition projectile = LoadOrCreateAsset<ProjectileAttackDeliveryDefinition>("Assets/Data/AttackModules/Delivery/Delivery_Projectile.asset");
        ProjectileSpreadAttackDeliveryDefinition knifeSpread = LoadOrCreateAsset<ProjectileSpreadAttackDeliveryDefinition>("Assets/Data/AttackModules/Delivery/Delivery_KnifeSpread.asset");
        ProjectileSpreadAttackDeliveryDefinition spearVolley = LoadOrCreateAsset<ProjectileSpreadAttackDeliveryDefinition>("Assets/Data/AttackModules/Delivery/Delivery_SpearVolley.asset");
        CircularSweepAttackDeliveryDefinition axeCleave = LoadOrCreateAsset<CircularSweepAttackDeliveryDefinition>("Assets/Data/AttackModules/Delivery/Delivery_VikingRoundCleave.asset");
        FrontalConeAttackDeliveryDefinition swordArc = LoadOrCreateAsset<FrontalConeAttackDeliveryDefinition>("Assets/Data/AttackModules/Delivery/Delivery_KnightSwordArc.asset");
        ChainAttackDeliveryDefinition lightningChain = LoadOrCreateAsset<ChainAttackDeliveryDefinition>("Assets/Data/AttackModules/Delivery/Delivery_LightningChain.asset");
        AreaPulseAttackDeliveryDefinition armageddonPulse = LoadOrCreateAsset<AreaPulseAttackDeliveryDefinition>("Assets/Data/AttackModules/Delivery/Delivery_ArmageddonPulse.asset");
        LineSlashAttackDeliveryDefinition doubleSlashLine = LoadOrCreateAsset<LineSlashAttackDeliveryDefinition>("Assets/Data/AttackModules/Delivery/Delivery_DoubleSlashLine.asset");

        ConfigureProjectile(projectile, 12f, 2.1f, 0.9f);
        ConfigureSpread(knifeSpread, 9, 70f, 18f, 1.8f, 0.85f);
        ConfigureSpread(spearVolley, 7, 24f, 17f, 2.3f, 0.9f);
        ConfigureCircularSweep(axeCleave, 2.8f, 0.22f, 80f, true);
        ConfigureCone(swordArc, 3.2f, 120f, 0.18f, 0.1f);
        ConfigureChain(lightningChain, 5, 6.5f, 0.08f);
        ConfigureArea(armageddonPulse, 4.2f, 0.45f, 0.12f);
        ConfigureLineSlash(doubleSlashLine, 5.2f, 0.95f, 2.6f, 2, 0.8f, 0.16f, 0.1f);

        SkillData vikingCleave = CreateSkill(runtime, "Skill_VikingWhirl.asset", "viking_whirl", "Вихрь Секиры", "Очень быстрый круговой удар секирой вокруг героя.", SkillHitType.AreaPulse, SkillElement.Physical, Tags("Melee", "Area", "Physical", "Axe"), SkillStats(16f, 0f, 0.06f, 2f, 0f, 0.45f, 1f, 1f, 1f, 0.04f), CreateAttack("Attack_VikingWhirl.asset", "viking_whirl", "Вихрь Секиры", 0.45f, 0f, selfTargeting, axeCleave, CreateDamage("Impact_Damage_VikingWhirl.asset", 0f, 0f, 1f)));
        SkillData assassinKnives = CreateSkill(runtime, "Skill_AssassinKnifeStorm.asset", "assassin_knife_storm", "Шторм Клинков", "Бросает множество клинков в ближайших врагов.", SkillHitType.Projectile, SkillElement.Physical, Tags("Projectile", "Physical", "Blade"), SkillStats(12f, 0f, 0.16f, 2.25f, 0f, 0.72f, 1f, 1f, 1f, 0.11f), CreateAttack("Attack_AssassinKnifeStorm.asset", "assassin_knife_storm", "Шторм Клинков", 0.72f, 0f, nearestTargeting, knifeSpread, CreateDamage("Impact_Damage_AssassinKnifeStorm.asset", 0f, 0f, 1f)));
        SkillData knightArc = CreateSkill(runtime, "Skill_KnightSwordArc.asset", "knight_sword_arc", "Рыцарский Размах", "Мощный удар мечом по сектору перед героем.", SkillHitType.MeleeArc, SkillElement.Physical, Tags("Melee", "Area", "Physical", "Sword"), SkillStats(24f, 0f, 0.04f, 1.9f, 0f, 0.68f, 1f, 1f, 1f, 0.03f), CreateAttack("Attack_KnightSwordArc.asset", "knight_sword_arc", "Рыцарский Размах", 0.68f, 0f, nearestTargeting, swordArc, CreateDamage("Impact_Damage_KnightSwordArc.asset", 0f, 0f, 1f)));

        SkillData boulderThrow = CreateSkill(runtime, "Skill_BoulderThrow.asset", "boulder_throw", "Бросок Булыжника", "Метает тяжелый булыжник в ближайшего врага.", SkillHitType.LobProjectile, SkillElement.Physical, Tags("Projectile", "Earth", "Physical"), SkillStats(42f, 0f, 0.05f, 2f, 0f, 1.35f, 1f, 1f, 1f, 0f), CreateAttack("Attack_BoulderThrow.asset", "boulder_throw", "Бросок Булыжника", 1.35f, 0f, nearestTargeting, projectile, CreateDamage("Impact_Damage_BoulderThrow.asset", 0f, 0f, 1f)));
        SkillData lightningSeries = CreateSkill(runtime, "Skill_LightningSeries.asset", "lightning_series", "Серия Молний", "Молния поражает одного врага и перескакивает дальше.", SkillHitType.Chain, SkillElement.Lightning, Tags("Chain", "Lightning", "Magic"), SkillStats(30f, 0f, 0.08f, 2f, 0f, 1.55f, 1f, 1f, 1f, 0f), CreateAttack("Attack_LightningSeries.asset", "lightning_series", "Серия Молний", 1.55f, 0f, nearestTargeting, lightningChain, CreateDamage("Impact_Damage_LightningSeries.asset", 0f, 0f, 1f)));
        SkillData doubleSlash = CreateSkill(runtime, "Skill_DoubleCleave.asset", "double_cleave", "Двойной Секущий Удар", "Две секущие линии поражают врагов в вытянутой зоне перед героем.", SkillHitType.MeleeArc, SkillElement.Physical, Tags("Melee", "Area", "Physical", "Slash"), SkillStats(22f, 0f, 0.06f, 2f, 0.1f, 1.05f, 1f, 1f, 1f, 0f), CreateAttack("Attack_DoubleCleave.asset", "double_cleave", "Двойной Секущий Удар", 1.05f, 0f, nearestTargeting, doubleSlashLine, CreateDamage("Impact_Damage_DoubleCleave.asset", 0f, 0f, 1f)));
        SkillData spearVolleySkill = CreateSkill(runtime, "Skill_SpearVolley.asset", "spear_volley", "Залп Копий", "Выпускает множество копий в направлении ближайшего врага.", SkillHitType.Projectile, SkillElement.Physical, Tags("Projectile", "Physical", "Spear"), SkillStats(18f, 0f, 0.05f, 2f, 0f, 1.1f, 1f, 1f, 1f, 0f), CreateAttack("Attack_SpearVolley.asset", "spear_volley", "Залп Копий", 1.1f, 0f, nearestTargeting, spearVolley, CreateDamage("Impact_Damage_SpearVolley.asset", 0f, 0f, 1f)));
        SkillData armageddon = CreateSkill(runtime, "Skill_Armageddon.asset", "armageddon", "Армагеддон", "С неба падает огромный булыжник и наносит мощный урон по области.", SkillHitType.RandomStrike, SkillElement.Fire, Tags("Area", "Earth", "Fire", "Meteor"), SkillStats(95f, 0f, 0.04f, 2f, 0.5f, 3.8f, 1f, 1f, 1f, 0f), CreateAttack("Attack_Armageddon.asset", "armageddon", "Армагеддон", 3.8f, 0f, randomTargeting, armageddonPulse, CreateDamage("Impact_Damage_Armageddon.asset", 0f, 0f, 1f)));

        SkillData[] commonSkills = { boulderThrow, lightningSeries, doubleSlash, spearVolleySkill, armageddon };

        WeaponData vikingAxe = CreateWeapon("Weapon_VikingAxe.asset", "viking_axe", "Секира Викинга", vikingCleave, commonSkills);
        WeaponData assassinBlades = CreateWeapon("Weapon_AssassinBlades.asset", "assassin_blades", "Клинки Ассасина", assassinKnives, commonSkills);
        WeaponData knightSword = CreateWeapon("Weapon_KnightSword.asset", "knight_sword", "Меч Рыцаря", knightArc, commonSkills);

        List<UpgradeData> upgrades = new()
        {
            CreateSkillUpgrade("Upgrade_AllSkillDamage.asset", "all_skill_damage", "Сила Всех Навыков", "Повышает урон всех активных навыков.", null, true, 7f, UpgradeRarity.Common),
            CreateSkillUpgrade("Upgrade_VikingWhirlDamage.asset", "viking_whirl_damage", "Острая Секира", "Повышает урон навыка Вихрь Секиры.", vikingCleave, false, 9f, UpgradeRarity.Uncommon),
            CreateSkillUpgrade("Upgrade_AssassinKnifeStormDamage.asset", "assassin_knife_storm_damage", "Отравленные Лезвия", "Повышает урон навыка Шторм Клинков.", assassinKnives, false, 7f, UpgradeRarity.Uncommon),
            CreateSkillUpgrade("Upgrade_KnightSwordArcDamage.asset", "knight_sword_arc_damage", "Закаленная Сталь", "Повышает урон навыка Рыцарский Размах.", knightArc, false, 10f, UpgradeRarity.Uncommon),
            CreateSkillUpgrade("Upgrade_BoulderThrowDamage.asset", "boulder_throw_damage", "Тяжелый Камень", "Повышает урон навыка Бросок Булыжника.", boulderThrow, false, 14f, UpgradeRarity.Uncommon),
            CreateSkillUpgrade("Upgrade_LightningSeriesDamage.asset", "lightning_series_damage", "Грозовой Заряд", "Повышает урон навыка Серия Молний.", lightningSeries, false, 10f, UpgradeRarity.Uncommon),
            CreateSkillUpgrade("Upgrade_DoubleCleaveDamage.asset", "double_cleave_damage", "Глубокий Разрез", "Повышает урон навыка Двойной Секущий Удар.", doubleSlash, false, 9f, UpgradeRarity.Uncommon),
            CreateSkillUpgrade("Upgrade_SpearVolleyDamage.asset", "spear_volley_damage", "Стальные Копья", "Повышает урон навыка Залп Копий.", spearVolleySkill, false, 8f, UpgradeRarity.Uncommon),
            CreateSkillUpgrade("Upgrade_ArmageddonDamage.asset", "armageddon_damage", "Небесная Катастрофа", "Повышает урон навыка Армагеддон.", armageddon, false, 22f, UpgradeRarity.Rare)
        };

        CharacterData viking = CreateCharacter("Hero_Viking.asset", "viking", "Викинг", vikingAxe, upgrades.ToArray(), Stats(
            (StatType.MaxHealth, 145f), (StatType.MoveSpeed, 5.7f), (StatType.PickupRadius, 2.6f), (StatType.Defense, 18f),
            (StatType.ParryChance, 0.03f), (StatType.ExperienceMultiplier, 1f), (StatType.DashCharges, 2f), (StatType.HealthRegenPercent, 0.01f),
            (StatType.LifeTotemCount, 0f)));

        CharacterData assassin = CreateCharacter("Hero_Assassin.asset", "assassin", "Ассасин", assassinBlades, upgrades.ToArray(), Stats(
            (StatType.MaxHealth, 90f), (StatType.MoveSpeed, 7.4f), (StatType.PickupRadius, 2.8f), (StatType.Defense, 4f),
            (StatType.ParryChance, 0.08f), (StatType.ExperienceMultiplier, 1.05f), (StatType.DashCharges, 3f), (StatType.HealthRegenPercent, 0f),
            (StatType.LifeTotemCount, 0f)));

        CharacterData knight = CreateCharacter("Hero_Knight.asset", "knight", "Рыцарь", knightSword, upgrades.ToArray(), Stats(
            (StatType.MaxHealth, 175f), (StatType.MoveSpeed, 5.2f), (StatType.PickupRadius, 2.5f), (StatType.Defense, 28f),
            (StatType.ParryChance, 0.12f), (StatType.ExperienceMultiplier, 1f), (StatType.DashCharges, 2f), (StatType.HealthRegenPercent, 0.012f),
            (StatType.LifeTotemCount, 0f)));

        AddCharactersToRoster(viking, assassin, knight);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static bool CoreAssetsExist()
    {
        return AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/Data/Characters/Hero_Viking.asset") != null
            && AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/Data/Characters/Hero_Assassin.asset") != null
            && AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/Data/Characters/Hero_Knight.asset") != null
            && AssetDatabase.LoadAssetAtPath<SkillData>("Assets/Data/Skills/Skill_Armageddon.asset") != null
            && AssetDatabase.LoadAssetAtPath<CircularSweepAttackDeliveryDefinition>("Assets/Data/AttackModules/Delivery/Delivery_DoubleCleaveSector.asset") != null;
    }

    private static SkillData CreateSkill(SkillRuntimeDefinition runtime, string fileName, string id, string displayName, string description, SkillHitType hitType, SkillElement element, string[] tags, SkillCombatConfig combat, AttackDefinition attack)
    {
        SkillData skill = LoadOrCreateAsset<SkillData>($"Assets/Data/Skills/{fileName}");
        SerializedObject so = new(skill);
        so.FindProperty("skillId").stringValue = id;
        so.FindProperty("displayName").stringValue = displayName;
        so.FindProperty("description").stringValue = description;
        so.FindProperty("hitType").enumValueIndex = (int)hitType;
        so.FindProperty("element").enumValueIndex = (int)element;
        so.FindProperty("runtimeDefinition").objectReferenceValue = runtime;
        so.FindProperty("icon").objectReferenceValue = null;
        so.FindProperty("visualPrefab").objectReferenceValue = null;
        so.FindProperty("baseDamage").floatValue = combat.BaseDamage;
        so.FindProperty("flatDamageBonus").floatValue = combat.FlatDamageBonus;
        so.FindProperty("critChance").floatValue = combat.CritChance;
        so.FindProperty("critMultiplier").floatValue = combat.CritMultiplier;
        so.FindProperty("areaBonus").floatValue = combat.AreaBonus;
        so.FindProperty("cooldown").floatValue = combat.Cooldown;
        so.FindProperty("cooldownCoefficient").floatValue = combat.CooldownCoefficient;
        so.FindProperty("cooldownDivider").floatValue = combat.CooldownDivider;
        so.FindProperty("areaMultiplier").floatValue = combat.AreaMultiplier;
        so.FindProperty("repeatChance").floatValue = combat.RepeatChance;
        so.FindProperty("globalSkill").boolValue = false;
        so.FindProperty("maxRank").intValue = 1;
        SetStringArray(so.FindProperty("tags"), tags);
        so.FindProperty("appliedStatuses").arraySize = 0;
        so.FindProperty("configurableAttacks").arraySize = 0;
        SetObjectArray(so.FindProperty("attackDefinitions"), attack);
        so.FindProperty("parameters").arraySize = 0;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(skill);
        return skill;
    }

    private static AttackDefinition CreateAttack(string fileName, string id, string displayName, float cooldown, float cooldownReduction, AttackTargetingDefinition targeting, AttackDeliveryDefinition delivery, AttackImpactDefinition impact)
    {
        AttackDefinition attack = LoadOrCreateAsset<AttackDefinition>($"Assets/Data/AttackModules/Definitions/{fileName}");
        SerializedObject so = new(attack);
        so.FindProperty("attackId").stringValue = id;
        so.FindProperty("displayName").stringValue = displayName;
        so.FindProperty("baseCooldown").floatValue = cooldown;
        so.FindProperty("cooldownReductionPerRank").floatValue = cooldownReduction;
        so.FindProperty("targeting").objectReferenceValue = targeting;
        so.FindProperty("delivery").objectReferenceValue = delivery;
        SetObjectArray(so.FindProperty("impacts"), impact);
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(attack);
        return attack;
    }

    private static DamageAttackImpactDefinition CreateDamage(string fileName, float baseDamage, float damagePerRank, float ownerDamageMultiplier)
    {
        DamageAttackImpactDefinition impact = LoadOrCreateAsset<DamageAttackImpactDefinition>($"Assets/Data/AttackModules/Impact/{fileName}");
        SerializedObject so = new(impact);
        so.FindProperty("baseDamage").floatValue = baseDamage;
        so.FindProperty("damagePerRank").floatValue = damagePerRank;
        so.FindProperty("ownerDamageMultiplier").floatValue = ownerDamageMultiplier;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(impact);
        return impact;
    }

    private static WeaponData CreateWeapon(string fileName, string id, string displayName, SkillData startingSkill, SkillData[] commonSkills)
    {
        WeaponData weapon = LoadOrCreateAsset<WeaponData>($"Assets/Data/Weapons/{fileName}");
        List<SkillData> pool = new() { startingSkill };
        pool.AddRange(commonSkills);

        SerializedObject so = new(weapon);
        so.FindProperty("weaponId").stringValue = id;
        so.FindProperty("displayName").stringValue = displayName;
        so.FindProperty("icon").objectReferenceValue = null;
        so.FindProperty("startingSkill").objectReferenceValue = startingSkill;
        SetObjectArray(so.FindProperty("uniqueSkillPool"), pool.ToArray());
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(weapon);
        return weapon;
    }

    private static CharacterData CreateCharacter(string fileName, string id, string displayName, WeaponData weapon, UpgradeData[] upgrades, StatValue[] stats)
    {
        CharacterData character = LoadOrCreateAsset<CharacterData>($"Assets/Data/Characters/{fileName}");
        SerializedObject so = new(character);
        so.FindProperty("characterId").stringValue = id;
        so.FindProperty("displayName").stringValue = displayName;
        so.FindProperty("modelPrefab").objectReferenceValue = null;
        SetStatArray(so.FindProperty("baseStats"), stats);
        so.FindProperty("startingWeapon").objectReferenceValue = weapon;
        SetObjectArray(so.FindProperty("availableWeapons"), weapon);
        SetObjectArray(so.FindProperty("globalUpgradePool"), upgrades);
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(character);
        return character;
    }

    private static UpgradeData CreateSkillUpgrade(string fileName, string id, string displayName, string description, SkillData targetSkill, bool allSkills, float damageBonus, UpgradeRarity rarity)
    {
        UpgradeData upgrade = LoadOrCreateAsset<UpgradeData>($"Assets/Data/Upgrades/{fileName}");
        SerializedObject so = new(upgrade);
        so.FindProperty("useBuilderDefinition").boolValue = true;
        so.FindProperty("upgradeId").stringValue = id;
        so.FindProperty("displayName").stringValue = displayName;
        so.FindProperty("description").stringValue = description;
        so.FindProperty("icon").objectReferenceValue = null;
        so.FindProperty("rarity").enumValueIndex = (int)rarity;
        so.FindProperty("targetType").enumValueIndex = (int)UpgradeTargetType.Skill;
        so.FindProperty("skillTargetMode").enumValueIndex = allSkills ? (int)UpgradeSkillTargetMode.AllActiveSkills : (int)UpgradeSkillTargetMode.SpecificSkill;
        so.FindProperty("targetStat").enumValueIndex = (int)StatType.Damage;
        so.FindProperty("targetStatValue").floatValue = damageBonus;
        so.FindProperty("scope").enumValueIndex = allSkills ? (int)UpgradeScope.GlobalStats : (int)UpgradeScope.SpecificSkill;
        so.FindProperty("repeatable").boolValue = true;
        so.FindProperty("targetSkill").objectReferenceValue = targetSkill;
        so.FindProperty("targetTags").arraySize = 0;
        so.FindProperty("statModifiers").arraySize = 0;
        so.FindProperty("tagStatModifiers").arraySize = 0;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(upgrade);
        return upgrade;
    }

    private static void AddCharactersToRoster(params CharacterData[] characters)
    {
        EnsureFolder("Assets/Resources");
        CharacterRoster roster = LoadOrCreateAsset<CharacterRoster>(RosterPath);
        SerializedObject so = new(roster);
        SerializedProperty property = so.FindProperty("characters");
        List<Object> existing = new();

        for (int i = 0; i < property.arraySize; i++)
        {
            Object value = property.GetArrayElementAtIndex(i).objectReferenceValue;

            if (value != null && !existing.Contains(value))
            {
                existing.Add(value);
            }
        }

        foreach (CharacterData character in characters)
        {
            if (character != null && !existing.Contains(character))
            {
                existing.Add(character);
            }
        }

        property.arraySize = existing.Count;

        for (int i = 0; i < existing.Count; i++)
        {
            property.GetArrayElementAtIndex(i).objectReferenceValue = existing[i];
        }

        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(roster);
    }

    private static void ConfigureNearest(NearestEnemyTargetingDefinition asset, int targetCount, float maxDistance)
    {
        SerializedObject so = new(asset);
        so.FindProperty("targetCount").intValue = targetCount;
        so.FindProperty("maxDistance").floatValue = maxDistance;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(asset);
    }

    private static void ConfigureRandom(RandomEnemyTargetingDefinition asset, int targetCount, float maxDistance)
    {
        SerializedObject so = new(asset);
        so.FindProperty("targetCount").intValue = targetCount;
        so.FindProperty("maxDistance").floatValue = maxDistance;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(asset);
    }

    private static void ConfigureProjectile(ProjectileAttackDeliveryDefinition asset, float speed, float lifetime, float spawnHeight)
    {
        SerializedObject so = new(asset);
        so.FindProperty("projectileSpeed").floatValue = speed;
        so.FindProperty("projectileLifetime").floatValue = lifetime;
        so.FindProperty("spawnHeight").floatValue = spawnHeight;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(asset);
    }

    private static void ConfigureSpread(ProjectileSpreadAttackDeliveryDefinition asset, int count, float spreadAngle, float speed, float lifetime, float spawnHeight)
    {
        SerializedObject so = new(asset);
        so.FindProperty("projectileCount").intValue = count;
        so.FindProperty("spreadAngle").floatValue = spreadAngle;
        so.FindProperty("projectileSpeed").floatValue = speed;
        so.FindProperty("projectileLifetime").floatValue = lifetime;
        so.FindProperty("spawnHeight").floatValue = spawnHeight;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(asset);
    }

    private static void ConfigureCone(FrontalConeAttackDeliveryDefinition asset, float radius, float angle, float visualLifetime, float visualHeight)
    {
        SerializedObject so = new(asset);
        so.FindProperty("radius").floatValue = radius;
        so.FindProperty("angle").floatValue = angle;
        so.FindProperty("visualLifetime").floatValue = visualLifetime;
        so.FindProperty("visualHeight").floatValue = visualHeight;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(asset);
    }

    private static void ConfigureCircularSweep(CircularSweepAttackDeliveryDefinition asset, float radius, float sweepDuration, float arcWidth, bool clockwise)
    {
        SerializedObject so = new(asset);
        so.FindProperty("radius").floatValue = radius;
        so.FindProperty("sweepDuration").floatValue = sweepDuration;
        so.FindProperty("arcWidth").floatValue = arcWidth;
        so.FindProperty("clockwise").boolValue = clockwise;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(asset);
    }

    private static void ConfigureChain(ChainAttackDeliveryDefinition asset, int maxJumps, float jumpRadius, float falloff)
    {
        SerializedObject so = new(asset);
        so.FindProperty("maxJumps").intValue = maxJumps;
        so.FindProperty("jumpRadius").floatValue = jumpRadius;
        so.FindProperty("damageFalloffPerJump").floatValue = falloff;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(asset);
    }

    private static void ConfigureArea(AreaPulseAttackDeliveryDefinition asset, float radius, float visualLifetime, float visualHeight)
    {
        SerializedObject so = new(asset);
        so.FindProperty("radius").floatValue = radius;
        so.FindProperty("visualLifetime").floatValue = visualLifetime;
        so.FindProperty("visualHeight").floatValue = visualHeight;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(asset);
    }

    private static void ConfigureLineSlash(LineSlashAttackDeliveryDefinition asset, float length, float width, float forwardOffset, int strikeCount, float lateralStep, float visualLifetime, float visualHeight)
    {
        SerializedObject so = new(asset);
        so.FindProperty("length").floatValue = length;
        so.FindProperty("width").floatValue = width;
        so.FindProperty("forwardOffset").floatValue = forwardOffset;
        so.FindProperty("strikeCount").intValue = strikeCount;
        so.FindProperty("lateralStep").floatValue = lateralStep;
        so.FindProperty("visualLifetime").floatValue = visualLifetime;
        so.FindProperty("visualHeight").floatValue = visualHeight;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(asset);
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

    private static string[] Tags(params string[] tags) => tags;

    private static SkillCombatConfig SkillStats(
        float baseDamage,
        float flatDamageBonus,
        float critChance,
        float critMultiplier,
        float areaBonus,
        float cooldown,
        float cooldownCoefficient,
        float cooldownDivider,
        float areaMultiplier,
        float repeatChance)
    {
        return new SkillCombatConfig
        {
            BaseDamage = baseDamage,
            FlatDamageBonus = flatDamageBonus,
            CritChance = critChance,
            CritMultiplier = critMultiplier,
            AreaBonus = areaBonus,
            Cooldown = cooldown,
            CooldownCoefficient = cooldownCoefficient,
            CooldownDivider = cooldownDivider,
            AreaMultiplier = areaMultiplier,
            RepeatChance = repeatChance
        };
    }

    private static StatValue[] Stats(params (StatType statType, float value)[] values)
    {
        StatValue[] result = new StatValue[values.Length];

        for (int i = 0; i < values.Length; i++)
        {
            result[i] = new StatValue
            {
                statType = values[i].statType,
                value = values[i].value
            };
        }

        return result;
    }

    private static void SetObjectArray<T>(SerializedProperty property, params T[] values) where T : Object
    {
        property.arraySize = values.Length;

        for (int i = 0; i < values.Length; i++)
        {
            property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }
    }

    private static void SetStringArray(SerializedProperty property, string[] values)
    {
        property.arraySize = values.Length;

        for (int i = 0; i < values.Length; i++)
        {
            property.GetArrayElementAtIndex(i).stringValue = values[i];
        }
    }

    private static void SetStatArray(SerializedProperty property, StatValue[] values)
    {
        property.arraySize = values.Length;

        for (int i = 0; i < values.Length; i++)
        {
            SerializedProperty item = property.GetArrayElementAtIndex(i);
            item.FindPropertyRelative("statType").enumValueIndex = (int)values[i].statType;
            item.FindPropertyRelative("value").floatValue = values[i].value;
        }
    }

    private struct SkillCombatConfig
    {
        public float BaseDamage;
        public float FlatDamageBonus;
        public float CritChance;
        public float CritMultiplier;
        public float AreaBonus;
        public float Cooldown;
        public float CooldownCoefficient;
        public float CooldownDivider;
        public float AreaMultiplier;
        public float RepeatChance;
    }
}
