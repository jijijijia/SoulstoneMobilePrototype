using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class MedievalFantasyContentCurator
{
    private const string DataRoot = "Assets/Data";
    private const string VisualRoot = "Assets/_Project/Prefabs";
    private const string RuntimePath = "Assets/Data/AttackModules/Runtime/ModularAttackRuntimeDefinition.asset";
    private const string SpawnTimelinePath = "Assets/Data/SpawnTimeline_Default.asset";
    private const string RosterPath = "Assets/Resources/CharacterRoster.asset";
    private const string MapDatabasePath = "Assets/Resources/MapDatabase.asset";

    [MenuItem("Soulstone/Content/Curate Medieval Fantasy Content Pack")]
    public static void Curate()
    {
        EnsureFolders();
        BuildSkillVisualPrefabs();
        SkillData[] skills = CurateSkills();
        WeaponData[] weapons = CurateWeapons(skills);
        CharacterData[] heroes = CurateHeroes(weapons);
        EnemyData[] enemies = CurateEnemies();
        MapData[] maps = CurateMaps(enemies);

        SetObjectArray(new SerializedObject(LoadOrCreate<CharacterRoster>(RosterPath)).FindProperty("characters"), heroes);
        SetObjectArray(new SerializedObject(LoadOrCreate<MapDatabase>(MapDatabasePath)).FindProperty("maps"), maps);

        SaveAsset(RosterPath);
        SaveAsset(MapDatabasePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        ContentValidationTools.ValidateContent();

        Debug.Log($"Curated medieval fantasy content pack: {heroes.Length} heroes, {weapons.Length} weapons, {skills.Length} skills, {enemies.Length} enemies, {maps.Length} maps.");
    }

    public static void CurateBatch()
    {
        Curate();
        EditorApplication.Exit(0);
    }

    private static SkillData[] CurateSkills()
    {
        SkillRuntimeDefinition runtime = Load<SkillRuntimeDefinition>(RuntimePath);

        return new[]
        {
            Skill("Skill_VikingWhirl", "viking_whirl", "Вихрь секиры", "Быстрый круговой удар секирой вокруг героя. Хорош против плотной толпы.", SkillHitType.AreaPulse, SkillElement.Physical, 16f, 0.06f, 2f, 0.45f, 1f, 0.04f, runtime, "Attack_VikingWhirl", "VFX_Slash_Orange", "Melee", "Area", "Physical", "Axe"),
            Skill("Skill_AssassinKnifeStorm", "assassin_knife_storm", "Шторм клинков", "Ассасин выпускает короткий веер метательных клинков в ближайших врагов.", SkillHitType.Projectile, SkillElement.Physical, 12f, 0.18f, 2.3f, 0.72f, 1f, 0.12f, runtime, "Attack_AssassinKnifeStorm", "VFX_Blade_Silver", "Projectile", "Physical", "Blade"),
            Skill("Skill_KnightSwordArc", "knight_sword_arc", "Рыцарский размах", "Меч рассекает сектор перед героем, не задевая полный круг.", SkillHitType.MeleeArc, SkillElement.Physical, 24f, 0.04f, 1.9f, 0.68f, 1f, 0.03f, runtime, "Attack_KnightSwordArc", "VFX_Slash_Gold", "Melee", "Area", "Physical", "Sword"),
            Skill("Skill_FireRain", "fire_rain", "Огненный дождь", "С неба падают огненные вспышки по случайным врагам.", SkillHitType.RandomStrike, SkillElement.Fire, 34f, 0.08f, 2f, 1.45f, 1.2f, 0.02f, runtime, "Attack_FireRain", "VFX_Fire_Red", "Area", "Fire", "Magic"),
            Skill("Skill_ArrowRain", "arrow_rain", "Град стрел", "Следопыт выпускает плотный, но контролируемый залп стрел.", SkillHitType.Projectile, SkillElement.Physical, 15f, 0.12f, 2.1f, 0.82f, 1f, 0.06f, runtime, "Attack_ArrowRain", "VFX_Arrow_Green", "Projectile", "Physical", "Bow"),
            Skill("Skill_RaiseDead", "raise_dead", "Поднять мёртвых", "Некромант призывает скелетов-союзников, которые отвлекают и атакуют врагов.", SkillHitType.Summon, SkillElement.Shadow, 11f, 0.05f, 2f, 4.2f, 1f, 0f, runtime, "Attack_RaiseDead", "VFX_Shadow_Purple", "Summon", "Shadow", "Minion"),
            Skill("Skill_PoisonMire", "poison_mire", "Ядовитая топь", "Создаёт токсичную область, наносящую урон врагам со временем.", SkillHitType.LastingArea, SkillElement.Poison, 10f, 0.05f, 2f, 2.2f, 1.35f, 0f, runtime, "Attack_PoisonMire", "VFX_Poison_Green", "Area", "Poison", "Lasting"),
            Skill("Skill_HolyHammer", "holy_hammer", "Святой молот", "Паладин обрушивает освящённый молот по широкой дуге.", SkillHitType.MeleeArc, SkillElement.Holy, 28f, 0.05f, 2f, 0.95f, 1.05f, 0.03f, runtime, "Attack_HolyHammer", "VFX_Holy_Gold", "Melee", "Area", "Holy"),
            Skill("Skill_RuneQuake", "rune_quake", "Рунный раскол", "Рунный кузнец разбивает землю, создавая ударную волну.", SkillHitType.AreaPulse, SkillElement.Arcane, 38f, 0.04f, 2f, 1.35f, 1.25f, 0f, runtime, "Attack_RuneQuake", "VFX_Arcane_Blue", "Area", "Arcane", "Earth"),
            Skill("Skill_BoulderThrow", "boulder_throw", "Бросок булыжника", "Тяжёлый камень летит в ближайшего врага и пробивает строй.", SkillHitType.LobProjectile, SkillElement.Physical, 42f, 0.05f, 2f, 1.35f, 1f, 0f, runtime, "Attack_BoulderThrow", "VFX_Stone_Brown", "Projectile", "Earth", "Physical"),
            Skill("Skill_LightningSeries", "lightning_series", "Серия молний", "Молния поражает цель и перескакивает между врагами.", SkillHitType.Chain, SkillElement.Lightning, 30f, 0.08f, 2f, 1.55f, 1f, 0f, runtime, "Attack_LightningSeries", "VFX_Lightning_Cyan", "Chain", "Lightning", "Magic"),
            Skill("Skill_DoubleCleave", "double_cleave", "Двойной секущий удар", "Два секущих удара бьют по сектору перед героем.", SkillHitType.MeleeArc, SkillElement.Physical, 22f, 0.06f, 2f, 1.05f, 1.1f, 0f, runtime, "Attack_DoubleCleave", "VFX_Slash_Silver", "Melee", "Area", "Slash"),
            Skill("Skill_SpearVolley", "spear_volley", "Залп копий", "Несколько копий летят в направлении ближайшего врага.", SkillHitType.Projectile, SkillElement.Physical, 18f, 0.05f, 2f, 1.1f, 1f, 0f, runtime, "Attack_SpearVolley", "VFX_Spear_Silver", "Projectile", "Physical", "Spear"),
            Skill("Skill_Armageddon", "armageddon", "Армагеддон", "Огромный валун падает с неба и наносит мощный урон по области.", SkillHitType.RandomStrike, SkillElement.Fire, 95f, 0.04f, 2f, 3.8f, 1.5f, 0f, runtime, "Attack_Armageddon", "VFX_Meteor_Red", "Area", "Fire", "Meteor"),
            Skill("Skill_StormCall", "storm_call", "Зов бури", "Грозовые разряды бьют случайные группы врагов.", SkillHitType.RandomStrike, SkillElement.Lightning, 31f, 0.09f, 2f, 1.6f, 1.15f, 0.02f, runtime, "Attack_StormCall", "VFX_Lightning_Cyan", "Area", "Lightning", "Magic"),
            Skill("Skill_StoneSpear", "stone_spear", "Каменное копьё", "Острое каменное копьё вырывается из земли под врагом.", SkillHitType.GroundAoe, SkillElement.Physical, 36f, 0.05f, 2f, 1.3f, 1.2f, 0f, runtime, "Attack_StoneSpear", "VFX_Stone_Brown", "Area", "Earth", "Physical"),
            Skill("Skill_ChainLightning", "chain_lightning", "Цепная молния", "Быстрый электрический разряд перескакивает по цепочке целей.", SkillHitType.Chain, SkillElement.Lightning, 24f, 0.1f, 2f, 1.05f, 1f, 0.04f, runtime, "Attack_ChainLightning", "VFX_Lightning_Cyan", "Chain", "Lightning", "Magic"),
            Skill("Skill_AxeCleave", "axe_cleave", "Тяжёлый рубящий удар", "Медленный, но сильный удар топором по широкой дуге.", SkillHitType.MeleeArc, SkillElement.Physical, 42f, 0.04f, 2f, 1.2f, 1.15f, 0f, runtime, "Attack_AxeCleave", "VFX_Slash_Orange", "Melee", "Area", "Axe"),
            Skill("Skill_KnifeFan", "knife_fan", "Веер ножей", "Короткий веер клинков для ближней дистанции.", SkillHitType.Projectile, SkillElement.Physical, 9f, 0.16f, 2.2f, 0.55f, 1f, 0.1f, runtime, "Attack_KnifeFan", "VFX_Blade_Silver", "Projectile", "Blade", "Physical"),
            Skill("Skill_ToxicVolley", "toxic_volley", "Токсичный залп", "Ядовитые дротики отравляют ближайших врагов.", SkillHitType.Projectile, SkillElement.Poison, 13f, 0.06f, 2f, 0.9f, 1f, 0.03f, runtime, "Attack_PoisonMire", "VFX_Poison_Green", "Projectile", "Poison")
        };
    }

    private static WeaponData[] CurateWeapons(IReadOnlyList<SkillData> skills)
    {
        SkillData S(string id) => FindById(skills, id);
        SkillData[] martial = { S("boulder_throw"), S("double_cleave"), S("spear_volley"), S("lightning_series"), S("armageddon") };
        SkillData[] magic = { S("fire_rain"), S("poison_mire"), S("storm_call"), S("stone_spear"), S("armageddon") };
        SkillData[] bow = { S("arrow_rain"), S("spear_volley"), S("toxic_volley"), S("chain_lightning"), S("boulder_throw") };

        return new[]
        {
            Weapon("Weapon_VikingAxe", "viking_axe", "Секира викинга", S("viking_whirl"), martial, "WeaponVisual_Axe_2H", true, 0),
            Weapon("Weapon_AssassinBlades", "assassin_blades", "Клинки ворона", S("assassin_knife_storm"), martial, "WeaponVisual_Dagger", false, 250),
            Weapon("Weapon_KnightSword", "knight_sword", "Рыцарский меч", S("knight_sword_arc"), martial, "WeaponVisual_Sword_1H", false, 220),
            Weapon("Weapon_FlameStaff", "flame_staff", "Пламенный посох", S("fire_rain"), magic, "WeaponVisual_Staff", false, 350),
            Weapon("Weapon_Longbow", "longbow", "Длинный лук", S("arrow_rain"), bow, "WeaponVisual_Bow_WithString", false, 280),
            Weapon("Weapon_BoneScepter", "bone_scepter", "Костяной скипетр", S("raise_dead"), magic, "WeaponVisual_SkeletonStaff", false, 380),
            Weapon("Weapon_HolyHammer", "holy_hammer", "Священный молот", S("holy_hammer"), martial, "WeaponVisual_Sword_2H", false, 420),
            Weapon("Weapon_RuneHammer", "rune_hammer", "Рунный молот", S("rune_quake"), magic, "WeaponVisual_Axe_1H", false, 450),
            Weapon("Weapon_StormBow", "storm_bow", "Грозовой лук", S("storm_call"), bow, "WeaponVisual_Bow_WithString", false, 500),
            Weapon("Weapon_ThunderSpear", "thunder_spear", "Громовое копьё", S("chain_lightning"), new[] { S("spear_volley"), S("lightning_series"), S("stone_spear"), S("storm_call") }, "WeaponVisual_Staff", false, 520),
            Weapon("Weapon_SoulScythe", "soul_scythe", "Коса душ", S("poison_mire"), magic, "WeaponVisual_SkeletonStaff", false, 600),
            Weapon("Weapon_Ragnar_GreatAxe", "ragnar_greataxe", "Великая секира Рагнара", S("axe_cleave"), martial, "WeaponVisual_Axe_2H", false, 650),
            Weapon("Weapon_GhostWand", "ghost_wand", "Призрачный жезл", S("raise_dead"), magic, "WeaponVisual_Wand", false, 420),
            Weapon("Weapon_Starter", "starter_sword", "Учебный меч", S("knight_sword_arc"), martial, "WeaponVisual_Sword_1H", true, 0)
        };
    }

    private static CharacterData[] CurateHeroes(IReadOnlyList<WeaponData> weapons)
    {
        WeaponData W(string id) => FindById(weapons, id);
        UpgradeData[] upgrades = LoadAll<UpgradeData>("Assets/Data/Upgrades");

        return new[]
        {
            Hero("Hero_Viking", "viking", "Сигурд Железный", "HeroVisual_Barbarian", W("viking_axe"), new[] { W("viking_axe"), W("ragnar_greataxe"), W("holy_hammer") }, upgrades, true, 0, Stats((StatType.MaxHealth,145), (StatType.MoveSpeed,5.7f), (StatType.PickupRadius,2.6f), (StatType.Defense,18), (StatType.ParryChance,0.03f), (StatType.ExperienceMultiplier,1), (StatType.DashCharges,2), (StatType.HealthRegenPercent,0.01f), (StatType.LifeTotemCount,0))),
            Hero("Hero_Assassin", "shadow_blade", "Теневой клинок", "HeroVisual_RogueHooded", W("assassin_blades"), new[] { W("assassin_blades"), W("soul_scythe"), W("ghost_wand") }, upgrades, false, 350, Stats((StatType.MaxHealth,90), (StatType.MoveSpeed,7.4f), (StatType.PickupRadius,2.8f), (StatType.Defense,4), (StatType.ParryChance,0.08f), (StatType.ExperienceMultiplier,1.05f), (StatType.DashCharges,3), (StatType.HealthRegenPercent,0), (StatType.LifeTotemCount,0))),
            Hero("Hero_Knight", "knight", "Сэр Альрик", "HeroVisual_Knight", W("knight_sword"), new[] { W("knight_sword"), W("holy_hammer"), W("starter_sword") }, upgrades, false, 300, Stats((StatType.MaxHealth,175), (StatType.MoveSpeed,5.2f), (StatType.PickupRadius,2.5f), (StatType.Defense,28), (StatType.ParryChance,0.12f), (StatType.ExperienceMultiplier,1), (StatType.DashCharges,2), (StatType.HealthRegenPercent,0.012f), (StatType.LifeTotemCount,0))),
            Hero("Hero_Pyromancer", "pyromancer", "Исолда Пламенная", "HeroVisual_Mage", W("flame_staff"), new[] { W("flame_staff"), W("ghost_wand"), W("thunder_spear") }, upgrades, false, 450, Stats((StatType.MaxHealth,82), (StatType.MoveSpeed,5.9f), (StatType.PickupRadius,2.7f), (StatType.Defense,3), (StatType.ParryChance,0.01f), (StatType.ExperienceMultiplier,1), (StatType.DashCharges,2), (StatType.HealthRegenPercent,0), (StatType.LifeTotemCount,0))),
            Hero("Hero_Ranger", "ranger", "Лесная следопытка", "HeroVisual_Ranger", W("longbow"), new[] { W("longbow"), W("storm_bow"), W("thunder_spear") }, upgrades, false, 380, Stats((StatType.MaxHealth,105), (StatType.MoveSpeed,6.8f), (StatType.PickupRadius,3.1f), (StatType.Defense,7), (StatType.ParryChance,0.05f), (StatType.ExperienceMultiplier,1.04f), (StatType.DashCharges,3), (StatType.HealthRegenPercent,0), (StatType.LifeTotemCount,0))),
            Hero("Hero_Necromancer", "necromancer", "Морриган Костяная", "HeroVisual_Mage", W("bone_scepter"), new[] { W("bone_scepter"), W("soul_scythe"), W("ghost_wand") }, upgrades, false, 520, Stats((StatType.MaxHealth,95), (StatType.MoveSpeed,5.4f), (StatType.PickupRadius,2.9f), (StatType.Defense,6), (StatType.ParryChance,0.02f), (StatType.ExperienceMultiplier,1.08f), (StatType.DashCharges,2), (StatType.HealthRegenPercent,0.004f), (StatType.LifeTotemCount,1))),
            Hero("Hero_Paladin", "paladin", "Брат Альден", "HeroVisual_Knight", W("holy_hammer"), new[] { W("holy_hammer"), W("knight_sword"), W("rune_hammer") }, upgrades, false, 560, Stats((StatType.MaxHealth,165), (StatType.MoveSpeed,5.3f), (StatType.PickupRadius,2.6f), (StatType.Defense,24), (StatType.ParryChance,0.09f), (StatType.ExperienceMultiplier,1), (StatType.DashCharges,2), (StatType.HealthRegenPercent,0.018f), (StatType.LifeTotemCount,0))),
            Hero("Hero_RuneSmith", "rune_smith", "Дурн Рунный", "HeroVisual_Barbarian", W("rune_hammer"), new[] { W("rune_hammer"), W("thunder_spear"), W("holy_hammer") }, upgrades, false, 620, Stats((StatType.MaxHealth,135), (StatType.MoveSpeed,5.5f), (StatType.PickupRadius,2.7f), (StatType.Defense,16), (StatType.ParryChance,0.04f), (StatType.ExperienceMultiplier,1), (StatType.DashCharges,2), (StatType.HealthRegenPercent,0.006f), (StatType.LifeTotemCount,0))),
            Hero("Hero_RagnarIronfang", "ragnar_ironfang", "Рагнар Железный Клык", "HeroVisual_Barbarian", W("ragnar_greataxe"), new[] { W("ragnar_greataxe"), W("viking_axe"), W("holy_hammer") }, upgrades, false, 720, Stats((StatType.MaxHealth,190), (StatType.MoveSpeed,5.0f), (StatType.PickupRadius,2.4f), (StatType.Defense,20), (StatType.ParryChance,0.02f), (StatType.ExperienceMultiplier,1), (StatType.DashCharges,1), (StatType.HealthRegenPercent,0.02f), (StatType.LifeTotemCount,0))),
            Hero("Hero_StormSentinel", "storm_sentinel", "Страж бури", "HeroVisual_Ranger", W("storm_bow"), new[] { W("storm_bow"), W("thunder_spear"), W("longbow") }, upgrades, false, 700, Stats((StatType.MaxHealth,112), (StatType.MoveSpeed,6.2f), (StatType.PickupRadius,2.9f), (StatType.Defense,10), (StatType.ParryChance,0.04f), (StatType.ExperienceMultiplier,1.02f), (StatType.DashCharges,2), (StatType.HealthRegenPercent,0), (StatType.LifeTotemCount,0)))
        };
    }

    private static EnemyData[] CurateEnemies()
    {
        EnemyAbilityConfig claw = Ability("EnemyAbility_BasicClaw", "skeleton_claw", "Костяной удар", EnemyAbilityDeliveryType.MeleeContact, null, 1.25f, 0.85f, 1f, 0.3f, 0, 0, Color.white);
        EnemyAbilityConfig bite = Ability("EnemyAbility_FastBite", "fast_bite", "Рваный выпад", EnemyAbilityDeliveryType.DashStrike, null, 1.35f, 1.25f, 1.15f, 0.25f, 0, 0, Color.white);
        EnemyAbilityConfig poison = Ability("EnemyAbility_PoisonBolt", "poison_bolt", "Ядовитая стрела", EnemyAbilityDeliveryType.Projectile, null, 6.2f, 1.8f, 0.85f, 0.45f, 11f, 4f, Color.green);
        EnemyAbilityConfig frost = Ability("EnemyAbility_FrostBolt", "frost_bolt", "Ледяной болт", EnemyAbilityDeliveryType.SlowProjectile, null, 6f, 2.0f, 0.75f, 0.45f, 9f, 4f, Color.cyan);
        EnemyAbilityConfig slam = Ability("EnemyAbility_TankSlam", "tank_slam", "Тяжёлый удар", EnemyAbilityDeliveryType.AreaPulse, null, 1.7f, 2.1f, 1.35f, 0.45f, 0, 0, new Color(1f, 0.65f, 0.25f));
        EnemyAbilityConfig shock = Ability("EnemyAbility_EliteShockwave", "elite_shockwave", "Элитная волна", EnemyAbilityDeliveryType.DelayedAreaMarker, null, 3.2f, 2.4f, 1.6f, 0.55f, 0, 0, new Color(0.9f, 0.2f, 1f));
        EnemyAbilityConfig quake = Ability("EnemyAbility_MiniBossQuake", "miniboss_quake", "Раскол костей", EnemyAbilityDeliveryType.DelayedAreaMarker, null, 3.5f, 3.2f, 2.2f, 0.8f, 0, 0, new Color(1f, 0.3f, 0.1f));
        EnemyAbilityConfig summoner = Ability("EnemyAbility_BoneSummon", "bone_summon", "Призыв костей", EnemyAbilityDeliveryType.SummonMinions, null, 5.5f, 5f, 0.6f, 0.6f, 0, 0, Color.white);
        EnemyAbilityConfig buff = Ability("EnemyAbility_WarDrum", "war_drum", "Боевой рёв", EnemyAbilityDeliveryType.AllyBuff, null, 4f, 5.5f, 0.4f, 0.7f, 0, 0, Color.yellow);
        EnemyAbilityConfig death = Ability("EnemyAbility_DeathBurst", "death_burst", "Посмертный взрыв", EnemyAbilityDeliveryType.DeathExplosion, null, 1.7f, 2f, 1.6f, 0.4f, 0, 0, Color.magenta);

        EnemyData basic = Enemy("Enemy_Basic", "skeleton_minion", "Скелет-прислужник", EnemyCategory.Normal, "EnemyGameplay_BasicSkeletonMinion", claw, 1, 6.5f, 1, 9, 2, 1f, Stats((StatType.MaxHealth,30), (StatType.MoveSpeed,3.4f), (StatType.ContactDamage,6)));
        EnemyData fastEnemy = Enemy("Enemy_Fast", "bone_runner", "Костяной бегун", EnemyCategory.Normal, "EnemyGameplay_FastSkeletonArcher", bite, 1, 5.2f, 2, 8, 2, 0.92f, Stats((StatType.MaxHealth,24), (StatType.MoveSpeed,4.8f), (StatType.ContactDamage,5)));
        EnemyData tank = Enemy("Enemy_Tank", "crypt_guard", "Страж крипты", EnemyCategory.Empowered, "EnemyGameplay_TankSkeletonWarrior", slam, 3, 2.2f, 4, 16, 6, 1.12f, Stats((StatType.MaxHealth,85), (StatType.MoveSpeed,2.6f), (StatType.ContactDamage,12)));
        EnemyData poisoner = Enemy("Enemy_PoisonSpitter", "venom_archer", "Ядовитый лучник", EnemyCategory.Normal, "EnemyGameplay_FastSkeletonArcher", poison, 2, 3.7f, 3, 10, 4, 0.96f, Stats((StatType.MaxHealth,34), (StatType.MoveSpeed,3.1f), (StatType.ContactDamage,5)));
        EnemyData frostMage = Enemy("Enemy_FrostShaman", "frost_binder", "Ледяной заклинатель", EnemyCategory.Empowered, "EnemyGameplay_SkeletonMage", frost, 3, 3.1f, 4, 12, 5, 1f, Stats((StatType.MaxHealth,42), (StatType.MoveSpeed,2.9f), (StatType.ContactDamage,7)));
        EnemyData elite = Enemy("Enemy_Elite", "bone_champion", "Костяной чемпион", EnemyCategory.Elite, "EnemyGameplay_EliteSkeletonWarrior", shock, 5, 1.4f, 8, 30, 14, 1.22f, Stats((StatType.MaxHealth,180), (StatType.MoveSpeed,3.0f), (StatType.ContactDamage,18)));
        EnemyData miniBoss = Enemy("Enemy_MiniBoss", "ossuary_brute", "Огромный костолом", EnemyCategory.MiniBoss, "EnemyGameplay_MiniBossSkeletonWarriorBroken", quake, 8, 0.7f, 14, 80, 40, 1.55f, Stats((StatType.MaxHealth,520), (StatType.MoveSpeed,2.6f), (StatType.ContactDamage,26)));
        EnemyData summonerEnemy = Enemy("Enemy_BoneSummoner", "bone_summoner", "Костяной призыватель", EnemyCategory.Elite, "EnemyGameplay_SkeletonMage", summoner, 7, 0.9f, 10, 35, 18, 1.05f, Stats((StatType.MaxHealth,150), (StatType.MoveSpeed,2.7f), (StatType.ContactDamage,10)));
        EnemyData warDrummer = Enemy("Enemy_WarDrummer", "war_drummer", "Барабанщик орды", EnemyCategory.Empowered, "EnemyGameplay_TankSkeletonWarrior", buff, 6, 1.1f, 8, 28, 16, 1.08f, Stats((StatType.MaxHealth,160), (StatType.MoveSpeed,2.8f), (StatType.ContactDamage,9)));
        EnemyData deathPriest = Enemy("Enemy_DeathPriest", "death_priest", "Жрец распада", EnemyCategory.Elite, "EnemyGameplay_SkeletonMage", death, 9, 0.8f, 10, 45, 22, 1.1f, Stats((StatType.MaxHealth,170), (StatType.MoveSpeed,2.8f), (StatType.ContactDamage,12)));

        SetAbilitySummon(summoner, basic);

        return new[] { basic, fastEnemy, tank, poisoner, frostMage, elite, miniBoss, summonerEnemy, warDrummer, deathPriest };
    }

    private static MapData[] CurateMaps(IReadOnlyList<EnemyData> enemies)
    {
        EnemyData E(string id) => FindById(enemies, id);
        SpawnTimelineData timeline = Load<SpawnTimelineData>(SpawnTimelinePath);

        return new[]
        {
            Map("Map_ForgottenPlains", "elderroot_wilds", "Чаща Старого Корня", "Лёгкая", "Лес / руины", "Скелеты-разведчики и первые лучники среди деревьев.", "Осколки душ, зелёные кристаллы, базовая руда.", "Map_ElderrootWilds", timeline, new[] { E("skeleton_minion"), E("bone_runner"), E("venom_archer"), E("frost_binder") }, new[] { E("bone_champion") }, 1, 0.95f, 1f, 1f, 1.0f, 0, 0, 1f, true, 0, new Vector2(-22, -22), new Vector2(22, 22)),
            Map("Map_AshenQuarry", "ember_vault", "Пепельный свод", "Средняя", "Кузница / пепел", "Тяжёлые стражи крипты, чемпионы и огненный темп боя.", "Руда оружия, янтарные кристаллы, усиленные награды.", "Map_EmberVault", timeline, new[] { E("skeleton_minion"), E("crypt_guard"), E("bone_champion"), E("war_drummer") }, new[] { E("ossuary_brute") }, 3, 1f, 1.15f, 1.15f, 1.08f, 1, 6, 1.15f, false, 250, new Vector2(-22, -22), new Vector2(22, 22)),
            Map("Map_VenomMarsh", "rotfen_hollow", "Гнилая лощина", "Средняя", "Болото / яд", "Ядовитые лучники, жрецы распада и вязкие толпы.", "Зелёные кристаллы, редкие улучшения статусов.", "Map_RotfenHollow", timeline, new[] { E("skeleton_minion"), E("venom_archer"), E("death_priest"), E("bone_runner") }, new[] { E("ossuary_brute") }, 4, 1.08f, 1.12f, 1.18f, 1.12f, 1, 8, 1.2f, false, 320, new Vector2(-22, -22), new Vector2(22, 22)),
            Map("Map_StormSpire", "stormglass_ruins", "Руины Грозового Стекла", "Высокая", "Башня / молнии", "Быстрые бегуны, ледяные маги и элитные заклинатели.", "Синие кристаллы, руническая пыль, высокий опыт.", "Map_StormglassRuins", timeline, new[] { E("bone_runner"), E("frost_binder"), E("bone_champion"), E("bone_summoner") }, new[] { E("ossuary_brute"), E("death_priest") }, 6, 1.18f, 1.25f, 1.25f, 1.18f, 2, 12, 1.35f, false, 480, new Vector2(-24, -24), new Vector2(24, 24)),
            Map("Map_BoneCitadel", "iron_candle_crypt", "Костяная цитадель", "Очень высокая", "Некрополь / крепость", "Плотные толпы, призыватели, барабанщики и мини-боссы.", "Много руды, кристаллы пустоты, лучшие награды.", "Map_IronCandleCrypt", timeline, new[] { E("skeleton_minion"), E("crypt_guard"), E("bone_champion"), E("bone_summoner"), E("war_drummer"), E("death_priest") }, new[] { E("ossuary_brute") }, 8, 1.28f, 1.35f, 1.35f, 1.25f, 3, 18, 1.55f, false, 650, new Vector2(-24, -24), new Vector2(24, 24))
        };
    }

    private static SkillData Skill(string assetName, string id, string title, string description, SkillHitType hitType, SkillElement element, float damage, float crit, float critMult, float cooldown, float area, float repeat, SkillRuntimeDefinition runtime, string attackAssetName, string vfxName, params string[] tags)
    {
        SkillData skill = LoadOrCreate<SkillData>($"{DataRoot}/Skills/{assetName}.asset");
        SerializedObject so = new(skill);
        so.FindProperty("skillId").stringValue = id;
        so.FindProperty("displayName").stringValue = title;
        so.FindProperty("description").stringValue = description;
        so.FindProperty("hitType").enumValueIndex = (int)hitType;
        so.FindProperty("element").enumValueIndex = (int)element;
        so.FindProperty("runtimeDefinition").objectReferenceValue = runtime;
        so.FindProperty("visualPrefab").objectReferenceValue = Load<GameObject>($"{VisualRoot}/VFX/{vfxName}.prefab");
        so.FindProperty("baseDamage").floatValue = damage;
        so.FindProperty("flatDamageBonus").floatValue = 0f;
        so.FindProperty("critChance").floatValue = crit;
        so.FindProperty("critMultiplier").floatValue = critMult;
        so.FindProperty("areaBonus").floatValue = Mathf.Max(0f, area - 1f);
        so.FindProperty("cooldown").floatValue = cooldown;
        so.FindProperty("cooldownCoefficient").floatValue = 1f;
        so.FindProperty("cooldownDivider").floatValue = 1f;
        so.FindProperty("areaMultiplier").floatValue = area;
        so.FindProperty("repeatChance").floatValue = repeat;
        so.FindProperty("globalSkill").boolValue = !assetName.Contains("Viking") && !assetName.Contains("Assassin") && !assetName.Contains("Knight") && !assetName.Contains("FireRain") && !assetName.Contains("ArrowRain") && !assetName.Contains("RaiseDead") && !assetName.Contains("HolyHammer") && !assetName.Contains("RuneQuake");
        so.FindProperty("maxRank").intValue = 1;
        SetStringArray(so.FindProperty("tags"), tags);
        so.FindProperty("appliedStatuses").arraySize = 0;
        so.FindProperty("configurableAttacks").arraySize = 0;
        SetObjectArray(so.FindProperty("attackDefinitions"), Load<AttackDefinition>($"{DataRoot}/AttackModules/Definitions/{attackAssetName}.asset"));
        so.FindProperty("parameters").arraySize = 0;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(skill);
        return skill;
    }

    private static WeaponData Weapon(string assetName, string id, string title, SkillData starter, SkillData[] pool, string visualName, bool unlocked, int cost)
    {
        WeaponData weapon = LoadOrCreate<WeaponData>($"{DataRoot}/Weapons/{assetName}.asset");
        SerializedObject so = new(weapon);
        so.FindProperty("weaponId").stringValue = id;
        so.FindProperty("displayName").stringValue = title;
        so.FindProperty("visualPrefab").objectReferenceValue = Load<GameObject>($"{VisualRoot}/Weapons/{visualName}.prefab");
        so.FindProperty("forgeVisualPrefab").objectReferenceValue = Load<GameObject>($"{VisualRoot}/Weapons/{visualName}.prefab");
        so.FindProperty("startingSkill").objectReferenceValue = starter;
        SetObjectArray(so.FindProperty("uniqueSkillPool"), Unique(starter, pool));
        so.FindProperty("maxLevel").intValue = 10;
        SetForgeCost(so.FindProperty("forgeUpgradeCostByLevel"));
        SetStatModifiers(so.FindProperty("statGrowthPerLevel"), (StatType.Damage, 0f, 0.04f));
        SetStatModifiers(so.FindProperty("skillGrowthPerLevel"), (StatType.SkillFrequency, 0f, 0.025f), (StatType.AreaMultiplier, 0f, 0.02f));
        SetCurrencyArray(so.FindProperty("requiredMaterials"), (CurrencyType.WeaponOre, 45), (CurrencyType.SoulShards, 80));
        so.FindProperty("unlockId").stringValue = id;
        so.FindProperty("unlockedByDefault").boolValue = unlocked;
        so.FindProperty("unlockCost").intValue = cost;
        so.FindProperty("unlockCurrency").enumValueIndex = (int)CurrencyType.SoulShards;
        so.FindProperty("unlockRequirementText").stringValue = unlocked ? "Открыто по умолчанию." : $"Разблокировать за {cost} осколков душ.";
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(weapon);
        return weapon;
    }

    private static CharacterData Hero(string assetName, string id, string title, string visualName, WeaponData starter, WeaponData[] weapons, UpgradeData[] upgrades, bool unlocked, int cost, StatValue[] stats)
    {
        CharacterData hero = LoadOrCreate<CharacterData>($"{DataRoot}/Characters/{assetName}.asset");
        SerializedObject so = new(hero);
        so.FindProperty("characterId").stringValue = id;
        so.FindProperty("displayName").stringValue = title;
        so.FindProperty("modelPrefab").objectReferenceValue = Load<GameObject>($"{VisualRoot}/Characters/{visualName}.prefab");
        SetStats(so.FindProperty("baseStats"), stats);
        so.FindProperty("startingWeapon").objectReferenceValue = starter;
        SetObjectArray(so.FindProperty("availableWeapons"), Unique(starter, weapons));
        SetObjectArray(so.FindProperty("globalUpgradePool"), upgrades);
        so.FindProperty("unlockId").stringValue = id;
        so.FindProperty("unlockedByDefault").boolValue = unlocked;
        so.FindProperty("unlockCost").intValue = cost;
        so.FindProperty("unlockCurrency").enumValueIndex = (int)CurrencyType.SoulShards;
        so.FindProperty("unlockRequirementText").stringValue = unlocked ? "Открыт по умолчанию." : $"Разблокировать за {cost} осколков душ.";
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(hero);
        return hero;
    }

    private static EnemyAbilityConfig Ability(string assetName, string id, string title, EnemyAbilityDeliveryType delivery, AttackDefinition attack, float distance, float cooldown, float damage, float tolerance, float projectileSpeed, float projectileLifetime, Color color)
    {
        EnemyAbilityConfig ability = LoadOrCreate<EnemyAbilityConfig>($"{DataRoot}/EnemyAbilities/{assetName}.asset");
        SerializedObject so = new(ability);
        so.FindProperty("abilityId").stringValue = id;
        so.FindProperty("displayName").stringValue = title;
        so.FindProperty("deliveryType").enumValueIndex = (int)delivery;
        so.FindProperty("attackDefinition").objectReferenceValue = attack;
        so.FindProperty("preferredDistance").floatValue = distance;
        so.FindProperty("cooldown").floatValue = cooldown;
        so.FindProperty("damageMultiplier").floatValue = damage;
        so.FindProperty("rangeTolerance").floatValue = tolerance;
        so.FindProperty("projectileSpeed").floatValue = projectileSpeed > 0f ? projectileSpeed : 10f;
        so.FindProperty("projectileLifetime").floatValue = projectileLifetime > 0f ? projectileLifetime : 4f;
        so.FindProperty("projectileScale").floatValue = 0.4f;
        so.FindProperty("projectileColor").colorValue = color;
        so.FindProperty("areaRadius").floatValue = delivery == EnemyAbilityDeliveryType.AreaPulse ? 2.5f : 2.8f;
        so.FindProperty("areaVisualLifetime").floatValue = 0.35f;
        so.FindProperty("telegraphDuration").floatValue = 0.55f;
        so.FindProperty("delayDuration").floatValue = 1.15f;
        so.FindProperty("poolDuration").floatValue = 4f;
        so.FindProperty("poolTickInterval").floatValue = 0.5f;
        so.FindProperty("summonCount").intValue = 3;
        so.FindProperty("buffRadius").floatValue = 5.5f;
        so.FindProperty("buffStatType").enumValueIndex = (int)StatType.ContactDamage;
        so.FindProperty("buffMultiplier").floatValue = 0.25f;
        so.FindProperty("buffDuration").floatValue = 7f;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(ability);
        return ability;
    }

    private static EnemyData Enemy(string assetName, string id, string title, EnemyCategory category, string prefabName, EnemyAbilityConfig ability, int level, float weight, int cost, int xp, int killReq, float scale, StatValue[] stats)
    {
        EnemyData enemy = LoadOrCreate<EnemyData>($"{DataRoot}/Enemies/{assetName}.asset");
        SerializedObject so = new(enemy);
        so.FindProperty("enemyId").stringValue = id;
        so.FindProperty("displayName").stringValue = title;
        so.FindProperty("category").enumValueIndex = (int)category;
        so.FindProperty("prefab").objectReferenceValue = Load<GameObject>($"{VisualRoot}/Enemies/Gameplay/{prefabName}.prefab");
        so.FindProperty("abilityConfig").objectReferenceValue = ability;
        SetStats(so.FindProperty("baseStats"), stats);
        so.FindProperty("spawnCost").intValue = cost;
        so.FindProperty("spawnWeight").floatValue = weight;
        so.FindProperty("unlockPlayerLevel").intValue = level;
        so.FindProperty("killRequirement").intValue = killReq;
        so.FindProperty("experienceReward").intValue = xp;
        so.FindProperty("visualScaleMultiplier").floatValue = scale;
        so.FindProperty("tintColor").colorValue = Color.white;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(enemy);
        return enemy;
    }

    private static MapData Map(string assetName, string id, string title, string difficulty, string biome, string enemies, string rewards, string prefabName, SpawnTimelineData timeline, EnemyData[] enemyPool, EnemyData[] bosses, int level, float xp, float health, float damage, float spawnRate, int packBonus, int aliveBonus, float rewardMultiplier, bool unlocked, int cost, Vector2 min, Vector2 max)
    {
        MapData map = LoadOrCreate<MapData>($"{DataRoot}/Maps/{assetName}.asset");
        SerializedObject so = new(map);
        so.FindProperty("mapId").stringValue = id;
        so.FindProperty("displayName").stringValue = title;
        so.FindProperty("difficulty").stringValue = difficulty;
        so.FindProperty("biome").stringValue = biome;
        so.FindProperty("enemyDescription").stringValue = enemies;
        so.FindProperty("rewardsDescription").stringValue = rewards;
        so.FindProperty("recommendedLevel").intValue = level;
        so.FindProperty("experienceMultiplier").floatValue = xp;
        so.FindProperty("mapPrefab").objectReferenceValue = Load<GameObject>($"{VisualRoot}/Maps/{prefabName}.prefab");
        so.FindProperty("spawnTimeline").objectReferenceValue = timeline;
        SetObjectArray(so.FindProperty("enemyPool"), enemyPool);
        SetObjectArray(so.FindProperty("bossList"), bosses);
        so.FindProperty("enemyHealthMultiplier").floatValue = health;
        so.FindProperty("enemyDamageMultiplier").floatValue = damage;
        so.FindProperty("spawnRateMultiplier").floatValue = spawnRate;
        so.FindProperty("packSizeBonus").intValue = packBonus;
        so.FindProperty("maxAliveBonus").intValue = aliveBonus;
        so.FindProperty("rewardMultiplier").floatValue = rewardMultiplier;
        so.FindProperty("playerSpawnPosition").vector3Value = new Vector3(0f, 1.08f, 0f);
        so.FindProperty("playerSpawnClearRadius").floatValue = 2f;
        so.FindProperty("overrideMapBounds").boolValue = true;
        so.FindProperty("mapMin").vector2Value = min;
        so.FindProperty("mapMax").vector2Value = max;
        so.FindProperty("unlockId").stringValue = id;
        so.FindProperty("unlockedByDefault").boolValue = unlocked;
        so.FindProperty("unlockCost").intValue = cost;
        so.FindProperty("unlockCurrency").enumValueIndex = (int)CurrencyType.SoulShards;
        so.FindProperty("unlockRequirementText").stringValue = unlocked ? "Открыта по умолчанию." : $"Разблокировать карту за {cost} осколков душ.";
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(map);
        return map;
    }

    private static void BuildSkillVisualPrefabs()
    {
        EnsureFolder($"{VisualRoot}/VFX");
        CreateVfx("VFX_Slash_Orange", new Color(1f, 0.35f, 0.05f, 0.85f), new Vector3(1.8f, 0.04f, 0.35f));
        CreateVfx("VFX_Slash_Gold", new Color(1f, 0.75f, 0.15f, 0.85f), new Vector3(1.6f, 0.04f, 0.32f));
        CreateVfx("VFX_Slash_Silver", new Color(0.85f, 0.9f, 1f, 0.8f), new Vector3(1.7f, 0.04f, 0.3f));
        CreateVfx("VFX_Blade_Silver", new Color(0.8f, 0.85f, 0.95f, 0.9f), new Vector3(0.18f, 0.18f, 0.8f));
        CreateVfx("VFX_Arrow_Green", new Color(0.3f, 1f, 0.45f, 0.85f), new Vector3(0.14f, 0.14f, 0.9f));
        CreateVfx("VFX_Fire_Red", new Color(1f, 0.18f, 0.05f, 0.9f), new Vector3(0.8f, 0.12f, 0.8f));
        CreateVfx("VFX_Poison_Green", new Color(0.1f, 0.9f, 0.18f, 0.75f), new Vector3(1.1f, 0.05f, 1.1f));
        CreateVfx("VFX_Shadow_Purple", new Color(0.42f, 0.05f, 0.85f, 0.8f), new Vector3(0.9f, 0.08f, 0.9f));
        CreateVfx("VFX_Holy_Gold", new Color(1f, 0.85f, 0.22f, 0.9f), new Vector3(1.1f, 0.08f, 1.1f));
        CreateVfx("VFX_Arcane_Blue", new Color(0.1f, 0.45f, 1f, 0.85f), new Vector3(1.1f, 0.08f, 1.1f));
        CreateVfx("VFX_Stone_Brown", new Color(0.45f, 0.33f, 0.22f, 1f), new Vector3(0.7f, 0.7f, 0.7f));
        CreateVfx("VFX_Lightning_Cyan", new Color(0.2f, 0.9f, 1f, 0.9f), new Vector3(0.18f, 1.8f, 0.18f));
        CreateVfx("VFX_Spear_Silver", new Color(0.8f, 0.85f, 0.92f, 0.9f), new Vector3(0.16f, 0.16f, 1.2f));
        CreateVfx("VFX_Meteor_Red", new Color(1f, 0.28f, 0.06f, 0.9f), new Vector3(1.4f, 1.4f, 1.4f));
    }

    private static void CreateVfx(string name, Color color, Vector3 scale)
    {
        string path = $"{VisualRoot}/VFX/{name}.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
        {
            return;
        }

        GameObject root = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        root.name = name;
        root.transform.localScale = scale;
        Object.DestroyImmediate(root.GetComponent<Collider>());

        Material material = new(Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default"))
        {
            name = $"{name}_Mat",
            color = color
        };

        EnsureFolder("Assets/_Project/Materials/VFX");
        AssetDatabase.CreateAsset(material, $"Assets/_Project/Materials/VFX/{name}_Mat.mat");
        root.GetComponent<Renderer>().sharedMaterial = material;

        PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
    }

    private static void SetAbilitySummon(EnemyAbilityConfig ability, EnemyData summoned)
    {
        SerializedObject so = new(ability);
        so.FindProperty("summonEnemyData").objectReferenceValue = summoned;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(ability);
    }

    private static void EnsureFolders()
    {
        EnsureFolder("Assets/Data/Characters");
        EnsureFolder("Assets/Data/Weapons");
        EnsureFolder("Assets/Data/Skills");
        EnsureFolder("Assets/Data/Enemies");
        EnsureFolder("Assets/Data/EnemyAbilities");
        EnsureFolder("Assets/Data/Maps");
        EnsureFolder("Assets/Resources");
        EnsureFolder("Assets/_Project/Prefabs/VFX");
    }

    private static T LoadOrCreate<T>(string path) where T : ScriptableObject
    {
        T asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset != null)
        {
            return asset;
        }

        EnsureFolder(System.IO.Path.GetDirectoryName(path)?.Replace('\\', '/'));
        asset = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(asset, path);
        return asset;
    }

    private static T Load<T>(string path) where T : Object => AssetDatabase.LoadAssetAtPath<T>(path);

    private static T[] LoadAll<T>(string folder) where T : Object
    {
        string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { folder });
        List<T> assets = new();
        for (int i = 0; i < guids.Length; i++)
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guids[i]));
            if (asset != null)
            {
                assets.Add(asset);
            }
        }
        return assets.ToArray();
    }

    private static void SaveAsset(string path)
    {
        Object asset = AssetDatabase.LoadAssetAtPath<Object>(path);
        if (asset != null)
        {
            EditorUtility.SetDirty(asset);
        }
    }

    private static T FindById<T>(IReadOnlyList<T> values, string id) where T : Object
    {
        for (int i = 0; i < values.Count; i++)
        {
            if (values[i] is SkillData skill && skill.SkillId == id) return values[i];
            if (values[i] is WeaponData weapon && weapon.WeaponId == id) return values[i];
            if (values[i] is EnemyData enemy && enemy.EnemyId == id) return values[i];
        }
        return null;
    }

    private static T[] Unique<T>(T first, IEnumerable<T> rest) where T : Object
    {
        List<T> result = new();
        void Add(T item)
        {
            if (item != null && !result.Contains(item))
            {
                result.Add(item);
            }
        }

        Add(first);
        foreach (T item in rest)
        {
            Add(item);
        }
        return result.ToArray();
    }

    private static StatValue[] Stats(params (StatType type, float value)[] values)
    {
        StatValue[] stats = new StatValue[values.Length];
        for (int i = 0; i < values.Length; i++)
        {
            stats[i] = new StatValue { statType = values[i].type, value = values[i].value };
        }
        return stats;
    }

    private static void SetStats(SerializedProperty property, StatValue[] stats)
    {
        property.arraySize = stats?.Length ?? 0;
        for (int i = 0; i < property.arraySize; i++)
        {
            SerializedProperty item = property.GetArrayElementAtIndex(i);
            item.FindPropertyRelative("statType").enumValueIndex = (int)stats[i].statType;
            item.FindPropertyRelative("value").floatValue = stats[i].value;
        }
    }

    private static void SetStringArray(SerializedProperty property, string[] values)
    {
        property.arraySize = values?.Length ?? 0;
        for (int i = 0; i < property.arraySize; i++)
        {
            property.GetArrayElementAtIndex(i).stringValue = values[i];
        }
    }

    private static void SetObjectArray(SerializedProperty property, params Object[] values)
    {
        property.arraySize = values?.Length ?? 0;
        for (int i = 0; i < property.arraySize; i++)
        {
            property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }

        property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetCurrencyArray(SerializedProperty property, params (CurrencyType type, int amount)[] values)
    {
        property.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++)
        {
            SerializedProperty item = property.GetArrayElementAtIndex(i);
            item.FindPropertyRelative("currencyType").enumValueIndex = (int)values[i].type;
            item.FindPropertyRelative("amount").intValue = values[i].amount;
        }
    }

    private static void SetForgeCost(SerializedProperty property)
    {
        property.arraySize = 10;
        for (int i = 0; i < 10; i++)
        {
            SerializedProperty item = property.GetArrayElementAtIndex(i);
            item.FindPropertyRelative("currencyType").enumValueIndex = (int)CurrencyType.WeaponOre;
            item.FindPropertyRelative("amount").intValue = 35 + i * 25;
        }
    }

    private static void SetStatModifiers(SerializedProperty property, params (StatType type, float additive, float multiplier)[] values)
    {
        property.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++)
        {
            SerializedProperty item = property.GetArrayElementAtIndex(i);
            item.FindPropertyRelative("statType").enumValueIndex = (int)values[i].type;
            item.FindPropertyRelative("additive").floatValue = values[i].additive;
            item.FindPropertyRelative("multiplier").floatValue = values[i].multiplier;
        }
    }

    private static void EnsureFolder(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || AssetDatabase.IsValidFolder(path))
        {
            return;
        }

        string parent = System.IO.Path.GetDirectoryName(path)?.Replace('\\', '/');
        string name = System.IO.Path.GetFileName(path);
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, name);
    }
}
