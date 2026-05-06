using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class MedievalFantasyContentInstaller
{
    private const string InstalledThisSessionKey = "Soulstone.MedievalFantasyContentInstaller.Ran.V2";
    private const string RosterPath = "Assets/Resources/CharacterRoster.asset";
    private const string RuntimePath = "Assets/Data/AttackModules/Runtime/ModularAttackRuntimeDefinition.asset";

    static MedievalFantasyContentInstaller()
    {
        EditorApplication.delayCall += EnsureInstalled;
    }

    [MenuItem("Soulstone/Install Medieval Fantasy Roster")]
    public static void EnsureInstalled()
    {
        if (CoreAssetsExist())
        {
            SessionState.SetBool(InstalledThisSessionKey, true);
            return;
        }

        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            EditorApplication.delayCall += EnsureInstalled;
            return;
        }

        Install();
        SessionState.SetBool(InstalledThisSessionKey, true);
    }

    private static void Install()
    {
        EnsureFolders();

        ModularAttackSkillRuntimeDefinition runtime = LoadOrCreateAsset<ModularAttackSkillRuntimeDefinition>(RuntimePath);
        NearestEnemyTargetingDefinition nearest = LoadOrCreateAsset<NearestEnemyTargetingDefinition>("Assets/Data/AttackModules/Targeting/Targeting_NearestEnemy.asset");
        RandomEnemyTargetingDefinition random = LoadOrCreateAsset<RandomEnemyTargetingDefinition>("Assets/Data/AttackModules/Targeting/Targeting_RandomEnemies.asset");
        SelfPositionTargetingDefinition self = LoadOrCreateAsset<SelfPositionTargetingDefinition>("Assets/Data/AttackModules/Targeting/Targeting_SelfPosition.asset");

        ConfigureNearest(nearest, 1, 17f);
        ConfigureRandom(random, 3, 18f);

        ProjectileAttackDeliveryDefinition projectile = LoadOrCreateAsset<ProjectileAttackDeliveryDefinition>("Assets/Data/AttackModules/Delivery/Delivery_Projectile.asset");
        ProjectileSpreadAttackDeliveryDefinition knifeSpread = LoadOrCreateAsset<ProjectileSpreadAttackDeliveryDefinition>("Assets/Data/AttackModules/Delivery/Delivery_KnifeSpread.asset");
        ProjectileSpreadAttackDeliveryDefinition arrowRain = LoadOrCreateAsset<ProjectileSpreadAttackDeliveryDefinition>("Assets/Data/AttackModules/Delivery/Delivery_ArrowRain.asset");
        ProjectileSpreadAttackDeliveryDefinition spearVolley = LoadOrCreateAsset<ProjectileSpreadAttackDeliveryDefinition>("Assets/Data/AttackModules/Delivery/Delivery_SpearVolley.asset");
        CircularSweepAttackDeliveryDefinition axeSweep = LoadOrCreateAsset<CircularSweepAttackDeliveryDefinition>("Assets/Data/AttackModules/Delivery/Delivery_VikingRoundCleave.asset");
        FrontalConeAttackDeliveryDefinition swordArc = LoadOrCreateAsset<FrontalConeAttackDeliveryDefinition>("Assets/Data/AttackModules/Delivery/Delivery_KnightSwordArc.asset");
        FrontalConeAttackDeliveryDefinition holyHammerArc = LoadOrCreateAsset<FrontalConeAttackDeliveryDefinition>("Assets/Data/AttackModules/Delivery/Delivery_HolyHammerArc.asset");
        ChainAttackDeliveryDefinition chain = LoadOrCreateAsset<ChainAttackDeliveryDefinition>("Assets/Data/AttackModules/Delivery/Delivery_LightningChain.asset");
        LastingAreaAttackDeliveryDefinition cursedGround = LoadOrCreateAsset<LastingAreaAttackDeliveryDefinition>("Assets/Data/AttackModules/Delivery/Delivery_CursedGround.asset");
        AreaPulseAttackDeliveryDefinition fireRain = LoadOrCreateAsset<AreaPulseAttackDeliveryDefinition>("Assets/Data/AttackModules/Delivery/Delivery_FireRainPulse.asset");
        AreaPulseAttackDeliveryDefinition runeQuake = LoadOrCreateAsset<AreaPulseAttackDeliveryDefinition>("Assets/Data/AttackModules/Delivery/Delivery_RuneQuakePulse.asset");
        AreaPulseAttackDeliveryDefinition armageddonPulse = LoadOrCreateAsset<AreaPulseAttackDeliveryDefinition>("Assets/Data/AttackModules/Delivery/Delivery_ArmageddonPulse.asset");
        LineSlashAttackDeliveryDefinition doubleSlash = LoadOrCreateAsset<LineSlashAttackDeliveryDefinition>("Assets/Data/AttackModules/Delivery/Delivery_DoubleSlashLine.asset");

        ConfigureProjectile(projectile, 13f, 2.1f, 0.9f);
        ConfigureSpread(knifeSpread, 9, 70f, 18f, 1.8f, 0.85f);
        ConfigureSpread(arrowRain, 8, 42f, 20f, 2.1f, 0.9f);
        ConfigureSpread(spearVolley, 7, 24f, 17f, 2.3f, 0.9f);
        ConfigureCircularSweep(axeSweep, 2.8f, 0.22f, 80f, true);
        ConfigureCone(swordArc, 3.2f, 120f, 0.18f, 0.1f);
        ConfigureCone(holyHammerArc, 3.0f, 150f, 0.22f, 0.1f);
        ConfigureChain(chain, 5, 6.5f, 0.08f);
        ConfigureLastingArea(cursedGround, 3.0f, 4.5f, 0.55f, 0.08f);
        ConfigureArea(fireRain, 2.6f, 0.32f, 0.1f);
        ConfigureArea(runeQuake, 3.4f, 0.4f, 0.1f);
        ConfigureArea(armageddonPulse, 4.2f, 0.45f, 0.12f);
        ConfigureLineSlash(doubleSlash, 5.2f, 0.95f, 2.6f, 2, 0.8f, 0.16f, 0.1f);

        SkillData vikingWhirl = CreateSkill(runtime, "Skill_VikingWhirl.asset", "viking_whirl", "Вихрь секиры", "Секира проходит полным кругом вокруг героя, постепенно задевая врагов по дуге.", SkillHitType.AreaPulse, SkillElement.Physical, Tags("Melee", "Area", "Physical", "Axe"), SkillStats(16f, 0f, 0.06f, 2f, 0f, 0.45f, 1f, 1f, 1f, 0.04f), CreateAttack("Attack_VikingWhirl.asset", "viking_whirl", "Вихрь секиры", 0.45f, self, axeSweep));
        SkillData shadowKnives = CreateSkill(runtime, "Skill_AssassinKnifeStorm.asset", "shadow_knife_storm", "Шквал кинжалов", "Теневой клинок бросает веер быстрых кинжалов в ближайших врагов.", SkillHitType.Projectile, SkillElement.Physical, Tags("Projectile", "Physical", "Blade", "Shadow"), SkillStats(12f, 0f, 0.18f, 2.3f, 0f, 0.72f, 1f, 1.05f, 1f, 0.12f), CreateAttack("Attack_AssassinKnifeStorm.asset", "shadow_knife_storm", "Шквал кинжалов", 0.72f, nearest, knifeSpread));
        SkillData knightArc = CreateSkill(runtime, "Skill_KnightSwordArc.asset", "knight_sword_arc", "Рыцарский размах", "Меч рассекает широкий сектор перед героем.", SkillHitType.MeleeArc, SkillElement.Physical, Tags("Melee", "Area", "Physical", "Sword"), SkillStats(24f, 0f, 0.04f, 1.9f, 0f, 0.68f, 1f, 1f, 1f, 0.03f), CreateAttack("Attack_KnightSwordArc.asset", "knight_sword_arc", "Рыцарский размах", 0.68f, nearest, swordArc));
        SkillData fireRainSkill = CreateSkill(runtime, "Skill_FireRain.asset", "fire_rain", "Огненный дождь", "С неба падают огненные вспышки по случайным врагам.", SkillHitType.RandomStrike, SkillElement.Fire, Tags("Area", "Fire", "Magic"), SkillStats(34f, 0f, 0.08f, 2f, 0.2f, 1.45f, 1f, 1f, 1f, 0.02f), CreateAttack("Attack_FireRain.asset", "fire_rain", "Огненный дождь", 1.45f, random, fireRain));
        SkillData arrowRainSkill = CreateSkill(runtime, "Skill_ArrowRain.asset", "arrow_rain", "Град стрел", "Следопыт выпускает плотный залп стрел.", SkillHitType.Projectile, SkillElement.Physical, Tags("Projectile", "Physical", "Bow"), SkillStats(15f, 0f, 0.12f, 2.1f, 0f, 0.82f, 1f, 1.08f, 1f, 0.06f), CreateAttack("Attack_ArrowRain.asset", "arrow_rain", "Град стрел", 0.82f, nearest, arrowRain));
        SkillData cursedGroundSkill = CreateSkill(runtime, "Skill_CursedGround.asset", "cursed_ground", "Проклятая земля", "Некромант оставляет темную область, пожирающую врагов.", SkillHitType.LastingArea, SkillElement.Shadow, Tags("Area", "Shadow", "Curse", "Magic"), SkillStats(10f, 0f, 0.05f, 2f, 0.35f, 2.2f, 1f, 1f, 1f, 0f), CreateAttack("Attack_CursedGround.asset", "cursed_ground", "Проклятая земля", 2.2f, random, cursedGround));
        SkillData holyHammerSkill = CreateSkill(runtime, "Skill_HolyHammer.asset", "holy_hammer", "Святой молот", "Паладин обрушивает священный молот по широкой дуге.", SkillHitType.MeleeArc, SkillElement.Holy, Tags("Melee", "Area", "Holy", "Hammer"), SkillStats(28f, 0f, 0.05f, 2f, 0.05f, 0.95f, 1f, 1f, 1f, 0.03f), CreateAttack("Attack_HolyHammer.asset", "holy_hammer", "Святой молот", 0.95f, nearest, holyHammerArc));
        SkillData runeQuakeSkill = CreateSkill(runtime, "Skill_RuneQuake.asset", "rune_quake", "Рунный раскол", "Рунный кузнец разбивает землю ударом молота.", SkillHitType.AreaPulse, SkillElement.Arcane, Tags("Area", "Earth", "Arcane", "Hammer"), SkillStats(38f, 0f, 0.04f, 2f, 0.25f, 1.35f, 1f, 1f, 1.1f, 0f), CreateAttack("Attack_RuneQuake.asset", "rune_quake", "Рунный раскол", 1.35f, self, runeQuake));

        SkillData boulderThrow = CreateSkill(runtime, "Skill_BoulderThrow.asset", "boulder_throw", "Бросок булыжника", "Метается тяжелый булыжник в ближайшего врага.", SkillHitType.LobProjectile, SkillElement.Physical, Tags("Projectile", "Earth", "Physical"), SkillStats(42f, 0f, 0.05f, 2f, 0f, 1.35f, 1f, 1f, 1f, 0f), CreateAttack("Attack_BoulderThrow.asset", "boulder_throw", "Бросок булыжника", 1.35f, nearest, projectile));
        SkillData lightningSeries = CreateSkill(runtime, "Skill_LightningSeries.asset", "lightning_series", "Серия молний", "Молния поражает одного врага и перескакивает дальше.", SkillHitType.Chain, SkillElement.Lightning, Tags("Chain", "Lightning", "Magic"), SkillStats(30f, 0f, 0.08f, 2f, 0f, 1.55f, 1f, 1f, 1f, 0f), CreateAttack("Attack_LightningSeries.asset", "lightning_series", "Серия молний", 1.55f, nearest, chain));
        SkillData doubleCleave = CreateSkill(runtime, "Skill_DoubleCleave.asset", "double_cleave", "Двойной секущий удар", "Две секущие линии поражают врагов в вытянутой зоне.", SkillHitType.MeleeArc, SkillElement.Physical, Tags("Melee", "Area", "Physical", "Slash"), SkillStats(22f, 0f, 0.06f, 2f, 0.1f, 1.05f, 1f, 1f, 1f, 0f), CreateAttack("Attack_DoubleCleave.asset", "double_cleave", "Двойной секущий удар", 1.05f, nearest, doubleSlash));
        SkillData spearVolleySkill = CreateSkill(runtime, "Skill_SpearVolley.asset", "spear_volley", "Залп копий", "Выпускает множество копий в направлении ближайшего врага.", SkillHitType.Projectile, SkillElement.Physical, Tags("Projectile", "Physical", "Spear"), SkillStats(18f, 0f, 0.05f, 2f, 0f, 1.1f, 1f, 1f, 1f, 0f), CreateAttack("Attack_SpearVolley.asset", "spear_volley", "Залп копий", 1.1f, nearest, spearVolley));
        SkillData armageddon = CreateSkill(runtime, "Skill_Armageddon.asset", "armageddon", "Армагеддон", "С неба падает огромный булыжник и наносит мощный урон по области.", SkillHitType.RandomStrike, SkillElement.Fire, Tags("Area", "Earth", "Fire", "Meteor"), SkillStats(95f, 0f, 0.04f, 2f, 0.5f, 3.8f, 1f, 1f, 1f, 0f), CreateAttack("Attack_Armageddon.asset", "armageddon", "Армагеддон", 3.8f, random, armageddonPulse));

        SkillData[] commonSkills = { boulderThrow, lightningSeries, doubleCleave, spearVolleySkill, armageddon };
        SkillData[] magicCommonSkills = { boulderThrow, lightningSeries, cursedGroundSkill, fireRainSkill, armageddon };
        SkillData[] martialCommonSkills = { boulderThrow, doubleCleave, spearVolleySkill, lightningSeries, armageddon };

        WeaponData vikingAxe = CreateWeapon("Weapon_VikingAxe.asset", "viking_axe", "Секира викинга", vikingWhirl, martialCommonSkills);
        WeaponData shadowDaggers = CreateWeapon("Weapon_AssassinBlades.asset", "shadow_daggers", "Клинки ворона", shadowKnives, martialCommonSkills);
        WeaponData knightSword = CreateWeapon("Weapon_KnightSword.asset", "knight_sword", "Рыцарский меч", knightArc, martialCommonSkills);
        WeaponData flameStaff = CreateWeapon("Weapon_FlameStaff.asset", "flame_staff", "Пламенный посох", fireRainSkill, magicCommonSkills);
        WeaponData rangerBow = CreateWeapon("Weapon_Longbow.asset", "longbow", "Длинный лук", arrowRainSkill, martialCommonSkills);
        WeaponData boneScepter = CreateWeapon("Weapon_BoneScepter.asset", "bone_scepter", "Костяной скипетр", cursedGroundSkill, magicCommonSkills);
        WeaponData holyHammerWeapon = CreateWeapon("Weapon_HolyHammer.asset", "holy_hammer", "Священный молот", holyHammerSkill, commonSkills);
        WeaponData runeHammer = CreateWeapon("Weapon_RuneHammer.asset", "rune_hammer", "Рунный молот", runeQuakeSkill, commonSkills);

        List<UpgradeData> upgrades = CreateUpgrades(new[]
        {
            vikingWhirl, shadowKnives, knightArc, fireRainSkill, arrowRainSkill, cursedGroundSkill, holyHammerSkill, runeQuakeSkill,
            boulderThrow, lightningSeries, doubleCleave, spearVolleySkill, armageddon
        });

        CharacterData viking = CreateCharacter("Hero_Viking.asset", "viking", "Викинг", vikingAxe, upgrades.ToArray(), Stats((StatType.MaxHealth, 145f), (StatType.MoveSpeed, 5.7f), (StatType.PickupRadius, 2.6f), (StatType.Defense, 18f), (StatType.ParryChance, 0.03f), (StatType.ExperienceMultiplier, 1f), (StatType.DashCharges, 2f), (StatType.HealthRegenPercent, 0.01f), (StatType.LifeTotemCount, 0f)));
        CharacterData shadowBlade = CreateCharacter("Hero_Assassin.asset", "shadow_blade", "Теневой клинок", shadowDaggers, upgrades.ToArray(), Stats((StatType.MaxHealth, 90f), (StatType.MoveSpeed, 7.4f), (StatType.PickupRadius, 2.8f), (StatType.Defense, 4f), (StatType.ParryChance, 0.08f), (StatType.ExperienceMultiplier, 1.05f), (StatType.DashCharges, 3f), (StatType.HealthRegenPercent, 0f), (StatType.LifeTotemCount, 0f)));
        CharacterData knight = CreateCharacter("Hero_Knight.asset", "knight", "Рыцарь", knightSword, upgrades.ToArray(), Stats((StatType.MaxHealth, 175f), (StatType.MoveSpeed, 5.2f), (StatType.PickupRadius, 2.5f), (StatType.Defense, 28f), (StatType.ParryChance, 0.12f), (StatType.ExperienceMultiplier, 1f), (StatType.DashCharges, 2f), (StatType.HealthRegenPercent, 0.012f), (StatType.LifeTotemCount, 0f)));
        CharacterData pyromancer = CreateCharacter("Hero_Pyromancer.asset", "pyromancer", "Пиромант", flameStaff, upgrades.ToArray(), Stats((StatType.MaxHealth, 82f), (StatType.MoveSpeed, 5.9f), (StatType.PickupRadius, 2.7f), (StatType.Defense, 3f), (StatType.ParryChance, 0.01f), (StatType.ExperienceMultiplier, 1f), (StatType.DashCharges, 2f), (StatType.HealthRegenPercent, 0f), (StatType.LifeTotemCount, 0f)));
        CharacterData ranger = CreateCharacter("Hero_Ranger.asset", "ranger", "Лесная следопытка", rangerBow, upgrades.ToArray(), Stats((StatType.MaxHealth, 105f), (StatType.MoveSpeed, 6.8f), (StatType.PickupRadius, 3.1f), (StatType.Defense, 7f), (StatType.ParryChance, 0.05f), (StatType.ExperienceMultiplier, 1.04f), (StatType.DashCharges, 3f), (StatType.HealthRegenPercent, 0f), (StatType.LifeTotemCount, 0f)));
        CharacterData necromancer = CreateCharacter("Hero_Necromancer.asset", "necromancer", "Некромант", boneScepter, upgrades.ToArray(), Stats((StatType.MaxHealth, 95f), (StatType.MoveSpeed, 5.4f), (StatType.PickupRadius, 2.9f), (StatType.Defense, 6f), (StatType.ParryChance, 0.02f), (StatType.ExperienceMultiplier, 1.08f), (StatType.DashCharges, 2f), (StatType.HealthRegenPercent, 0.004f), (StatType.LifeTotemCount, 1f)));
        CharacterData paladin = CreateCharacter("Hero_Paladin.asset", "paladin", "Паладин", holyHammerWeapon, upgrades.ToArray(), Stats((StatType.MaxHealth, 165f), (StatType.MoveSpeed, 5.3f), (StatType.PickupRadius, 2.6f), (StatType.Defense, 24f), (StatType.ParryChance, 0.09f), (StatType.ExperienceMultiplier, 1f), (StatType.DashCharges, 2f), (StatType.HealthRegenPercent, 0.018f), (StatType.LifeTotemCount, 0f)));
        CharacterData runeSmith = CreateCharacter("Hero_RuneSmith.asset", "rune_smith", "Рунный кузнец", runeHammer, upgrades.ToArray(), Stats((StatType.MaxHealth, 135f), (StatType.MoveSpeed, 5.5f), (StatType.PickupRadius, 2.7f), (StatType.Defense, 16f), (StatType.ParryChance, 0.04f), (StatType.ExperienceMultiplier, 1f), (StatType.DashCharges, 2f), (StatType.HealthRegenPercent, 0.006f), (StatType.LifeTotemCount, 0f)));

        AddCharactersToRoster(viking, shadowBlade, knight, pyromancer, ranger, necromancer, paladin, runeSmith);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static bool CoreAssetsExist()
    {
        return AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/Data/Characters/Hero_Pyromancer.asset") != null
            && AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/Data/Characters/Hero_Ranger.asset") != null
            && AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/Data/Characters/Hero_Necromancer.asset") != null
            && AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/Data/Characters/Hero_Paladin.asset") != null
            && AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/Data/Characters/Hero_RuneSmith.asset") != null
            && AssetDatabase.LoadAssetAtPath<SkillData>("Assets/Data/Skills/Skill_FireRain.asset") != null
            && AssetDatabase.LoadAssetAtPath<SkillData>("Assets/Data/Skills/Skill_RaiseDead.asset") != null
            && AssetDatabase.LoadAssetAtPath<CircularSweepAttackDeliveryDefinition>("Assets/Data/AttackModules/Delivery/Delivery_DoubleCleaveSector.asset") != null;
    }

    private static List<UpgradeData> CreateUpgrades(SkillData[] skills)
    {
        List<UpgradeData> upgrades = new()
        {
            CreateSkillUpgrade("Upgrade_AllSkillDamage.asset", "all_skill_damage", "Сила всех навыков", "Повышает урон всех активных навыков.", null, true, 7f, UpgradeRarity.Common)
        };

        foreach (SkillData skill in skills)
        {
            if (skill == null)
            {
                continue;
            }

            upgrades.Add(CreateSkillUpgrade($"Upgrade_{skill.name.Replace("Skill_", string.Empty)}Damage.asset",
                $"{skill.SkillId}_damage",
                $"Усиление: {skill.DisplayName}",
                $"Повышает урон навыка {skill.DisplayName}.",
                skill,
                false,
                GetSkillSpecificDamageBonus(skill),
                UpgradeRarity.Uncommon));
        }

        return upgrades;
    }

    private static float GetSkillSpecificDamageBonus(SkillData skill)
    {
        return skill.HitType switch
        {
            SkillHitType.RandomStrike => 18f,
            SkillHitType.LobProjectile => 14f,
            SkillHitType.Chain => 10f,
            SkillHitType.LastingArea => 5f,
            SkillHitType.Projectile => 7f,
            _ => 9f
        };
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

    private static AttackDefinition CreateAttack(string fileName, string id, string displayName, float cooldown, AttackTargetingDefinition targeting, AttackDeliveryDefinition delivery)
    {
        AttackDefinition attack = LoadOrCreateAsset<AttackDefinition>($"Assets/Data/AttackModules/Definitions/{fileName}");
        DamageAttackImpactDefinition impact = CreateDamage($"Impact_Damage_{fileName.Replace("Attack_", string.Empty)}", 0f, 0f, 1f);
        SerializedObject so = new(attack);
        so.FindProperty("attackId").stringValue = id;
        so.FindProperty("displayName").stringValue = displayName;
        so.FindProperty("baseCooldown").floatValue = cooldown;
        so.FindProperty("cooldownReductionPerRank").floatValue = 0f;
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

    private static WeaponData CreateWeapon(string fileName, string id, string displayName, SkillData startingSkill, SkillData[] skillPool)
    {
        WeaponData weapon = LoadOrCreateAsset<WeaponData>($"Assets/Data/Weapons/{fileName}");
        List<SkillData> pool = new() { startingSkill };
        pool.AddRange(skillPool);

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

    private static void EnsureFolders()
    {
        EnsureFolder("Assets/Data/Characters");
        EnsureFolder("Assets/Data/Weapons");
        EnsureFolder("Assets/Data/Skills");
        EnsureFolder("Assets/Data/Upgrades");
        EnsureFolder("Assets/Data/AttackModules/Targeting");
        EnsureFolder("Assets/Data/AttackModules/Delivery");
        EnsureFolder("Assets/Data/AttackModules/Impact");
        EnsureFolder("Assets/Data/AttackModules/Definitions");
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

    private static void ConfigureProjectile(ProjectileAttackDeliveryDefinition asset, float speed, float lifetime, float spawnHeight) => SetSerialized(asset, ("projectileSpeed", speed), ("projectileLifetime", lifetime), ("spawnHeight", spawnHeight));

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

    private static void ConfigureCircularSweep(CircularSweepAttackDeliveryDefinition asset, float radius, float duration, float arcWidth, bool clockwise)
    {
        SerializedObject so = new(asset);
        so.FindProperty("radius").floatValue = radius;
        so.FindProperty("sweepDuration").floatValue = duration;
        so.FindProperty("arcWidth").floatValue = arcWidth;
        so.FindProperty("clockwise").boolValue = clockwise;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(asset);
    }

    private static void ConfigureCone(FrontalConeAttackDeliveryDefinition asset, float radius, float angle, float visualLifetime, float visualHeight) => SetSerialized(asset, ("radius", radius), ("angle", angle), ("visualLifetime", visualLifetime), ("visualHeight", visualHeight));

    private static void ConfigureChain(ChainAttackDeliveryDefinition asset, int maxJumps, float jumpRadius, float falloff)
    {
        SerializedObject so = new(asset);
        so.FindProperty("maxJumps").intValue = maxJumps;
        so.FindProperty("jumpRadius").floatValue = jumpRadius;
        so.FindProperty("damageFalloffPerJump").floatValue = falloff;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(asset);
    }

    private static void ConfigureLastingArea(LastingAreaAttackDeliveryDefinition asset, float radius, float duration, float tickInterval, float visualHeight) => SetSerialized(asset, ("radius", radius), ("duration", duration), ("tickInterval", tickInterval), ("visualHeight", visualHeight));
    private static void ConfigureArea(AreaPulseAttackDeliveryDefinition asset, float radius, float visualLifetime, float visualHeight) => SetSerialized(asset, ("radius", radius), ("visualLifetime", visualLifetime), ("visualHeight", visualHeight));

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

    private static void SetSerialized(Object asset, params (string propertyName, float value)[] values)
    {
        SerializedObject so = new(asset);

        foreach ((string propertyName, float value) in values)
        {
            so.FindProperty(propertyName).floatValue = value;
        }

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

    private static SkillCombatConfig SkillStats(float baseDamage, float flatDamageBonus, float critChance, float critMultiplier, float areaBonus, float cooldown, float cooldownCoefficient, float cooldownDivider, float areaMultiplier, float repeatChance)
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
            result[i] = new StatValue { statType = values[i].statType, value = values[i].value };
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
