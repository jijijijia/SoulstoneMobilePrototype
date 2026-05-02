using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public class CreateCharacterContentWizard : EditorWindow
{
    private const string RuntimeDefinitionPath = "Assets/Data/AttackModules/Runtime/ConfigurableSkillRuntimeDefinition.asset";
    private const string DefaultHeroPath = "Assets/Data/Characters/Hero_Default.asset";
    private const string RosterPath = "Assets/Resources/CharacterRoster.asset";

    private string characterName = "Storm Sentinel";
    private string weaponName = "Storm Sigil";
    private string skillName = "Storm Javelin";
    private string skillDescription = "Launch charged javelins that spear the closest enemies.";
    private string tagList = "Projectile, Lightning, Magic";
    private GameObject modelPrefab;

    private float maxHealth = 90f;
    private float moveSpeed = 6.8f;
    private float flatDamageBonus = 0f;
    private float attackPower = 1.12f;
    private float critChance = 0.1f;
    private float critMultiplier = 2f;
    private float skillFrequency = 1.08f;
    private float areaBonus = 0f;
    private float areaMultiplier = 1f;
    private float doubleAttackChance = 0.08f;
    private float defense = 0f;
    private float parryChance = 0f;
    private float experienceMultiplier = 1f;
    private float pickupRadius = 2.8f;
    private float attackRange = 1.5f;
    private float projectileSpeedBonus = 2f;
    private float projectileLifetimeBonus = 0.35f;
    private int dashCharges = 2;
    private float healthRegenPercent = 0f;
    private int lifeTotemCount = 0;

    private ConfigurableSkillTargetingMode targetingMode = ConfigurableSkillTargetingMode.NearestEnemy;
    private SkillHitType skillHitType = SkillHitType.Projectile;
    private SkillElement skillElement = SkillElement.Lightning;
    private int targetCount = 2;
    private float maxDistance = 14f;
    private ConfigurableSkillDeliveryMode deliveryMode = ConfigurableSkillDeliveryMode.Projectile;
    private float skillCooldown = 0.68f;
    private float cooldownReductionPerRank = 0.04f;
    private float projectileSpeed = 16f;
    private float projectileLifetime = 2.6f;
    private float spawnHeight = 0.85f;
    private float areaRadius = 3f;
    private float areaVisualLifetime = 0.25f;
    private float baseDamage = 16f;
    private float damagePerRank = 5f;
    private float ownerDamageMultiplier = 1.15f;

    [MenuItem("Soulstone/Create Character Content Wizard")]
    public static void ShowWindow()
    {
        CreateCharacterContentWizard window = GetWindow<CreateCharacterContentWizard>("Character Content");
        window.minSize = new Vector2(460f, 720f);
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Create Character Content", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Creates a CharacterData, WeaponData, configurable SkillData and adds the hero to the Character Roster.", MessageType.Info);

        EditorGUILayout.Space(8f);
        DrawIdentitySection();
        EditorGUILayout.Space(8f);
        DrawStatsSection();
        EditorGUILayout.Space(8f);
        DrawSkillSection();
        EditorGUILayout.Space(12f);

        GUI.enabled = !EditorApplication.isCompiling && !EditorApplication.isUpdating;

        if (GUILayout.Button("Create Character Pack", GUILayout.Height(40f)))
        {
            CreateContentPack();
        }

        GUI.enabled = true;
    }

    private void DrawIdentitySection()
    {
        EditorGUILayout.LabelField("Identity", EditorStyles.boldLabel);
        characterName = EditorGUILayout.TextField("Character Name", characterName);
        weaponName = EditorGUILayout.TextField("Weapon Name", weaponName);
        skillName = EditorGUILayout.TextField("Skill Name", skillName);
        skillDescription = EditorGUILayout.TextField("Skill Description", skillDescription);
        tagList = EditorGUILayout.TextField("Skill Tags", tagList);
        modelPrefab = (GameObject)EditorGUILayout.ObjectField("Model Prefab", modelPrefab, typeof(GameObject), false);
    }

    private void DrawStatsSection()
    {
        EditorGUILayout.LabelField("Character Stats", EditorStyles.boldLabel);
        maxHealth = EditorGUILayout.FloatField("Max Health", maxHealth);
        moveSpeed = EditorGUILayout.FloatField("Move Speed", moveSpeed);
        flatDamageBonus = EditorGUILayout.FloatField("Flat Damage Bonus", flatDamageBonus);
        attackPower = EditorGUILayout.FloatField("Attack Power", attackPower);
        critChance = EditorGUILayout.Slider("Crit Chance", critChance, 0f, 1f);
        critMultiplier = EditorGUILayout.FloatField("Crit Multiplier", critMultiplier);
        skillFrequency = EditorGUILayout.FloatField("Skill Frequency", skillFrequency);
        areaBonus = EditorGUILayout.FloatField("Area Bonus", areaBonus);
        areaMultiplier = EditorGUILayout.FloatField("Area Multiplier", areaMultiplier);
        doubleAttackChance = EditorGUILayout.Slider("Double Attack Chance", doubleAttackChance, 0f, 1f);
        defense = EditorGUILayout.FloatField("Defense", defense);
        parryChance = EditorGUILayout.Slider("Parry Chance", parryChance, 0f, 1f);
        experienceMultiplier = EditorGUILayout.FloatField("Experience Multiplier", experienceMultiplier);
        pickupRadius = EditorGUILayout.FloatField("Pickup Radius", pickupRadius);
        attackRange = EditorGUILayout.FloatField("Attack Range", attackRange);
        projectileSpeedBonus = EditorGUILayout.FloatField("Projectile Speed Bonus", projectileSpeedBonus);
        projectileLifetimeBonus = EditorGUILayout.FloatField("Projectile Lifetime Bonus", projectileLifetimeBonus);
        dashCharges = EditorGUILayout.IntField("Dash Charges", dashCharges);
        healthRegenPercent = EditorGUILayout.FloatField("Health Regen Percent", healthRegenPercent);
        lifeTotemCount = EditorGUILayout.IntField("Life Totem Count", lifeTotemCount);
    }

    private void DrawSkillSection()
    {
        EditorGUILayout.LabelField("Starting Skill", EditorStyles.boldLabel);
        skillHitType = (SkillHitType)EditorGUILayout.EnumPopup("Hit Type", skillHitType);
        skillElement = (SkillElement)EditorGUILayout.EnumPopup("Element", skillElement);
        targetingMode = (ConfigurableSkillTargetingMode)EditorGUILayout.EnumPopup("Targeting", targetingMode);
        targetCount = EditorGUILayout.IntField("Target Count", targetCount);
        maxDistance = EditorGUILayout.FloatField("Max Distance", maxDistance);
        deliveryMode = (ConfigurableSkillDeliveryMode)EditorGUILayout.EnumPopup("Delivery", deliveryMode);
        skillCooldown = EditorGUILayout.FloatField("Base Cooldown", skillCooldown);
        cooldownReductionPerRank = EditorGUILayout.FloatField("Cooldown Reduction / Rank", cooldownReductionPerRank);
        projectileSpeed = EditorGUILayout.FloatField("Projectile Speed", projectileSpeed);
        projectileLifetime = EditorGUILayout.FloatField("Projectile Lifetime", projectileLifetime);
        spawnHeight = EditorGUILayout.FloatField("Spawn Height", spawnHeight);
        areaRadius = EditorGUILayout.FloatField("Area Radius", areaRadius);
        areaVisualLifetime = EditorGUILayout.FloatField("Area Visual Lifetime", areaVisualLifetime);
        baseDamage = EditorGUILayout.FloatField("Base Damage", baseDamage);
        damagePerRank = EditorGUILayout.FloatField("Damage / Rank", damagePerRank);
        ownerDamageMultiplier = EditorGUILayout.FloatField("Owner Damage Multiplier", ownerDamageMultiplier);
    }

    private void CreateContentPack()
    {
        string slug = Slugify(characterName);

        if (string.IsNullOrWhiteSpace(slug))
        {
            EditorUtility.DisplayDialog("Invalid Name", "Character name must contain at least one letter or number.", "OK");
            return;
        }

        SkillRuntimeDefinition runtimeDefinition = AssetDatabase.LoadAssetAtPath<SkillRuntimeDefinition>(RuntimeDefinitionPath);

        if (runtimeDefinition == null)
        {
            EditorUtility.DisplayDialog("Missing Runtime Definition", $"Could not find {RuntimeDefinitionPath}.", "OK");
            return;
        }

        CharacterData defaultHero = AssetDatabase.LoadAssetAtPath<CharacterData>(DefaultHeroPath);

        if (defaultHero == null)
        {
            EditorUtility.DisplayDialog("Missing Default Hero", $"Could not find {DefaultHeroPath}.", "OK");
            return;
        }

        CharacterRoster roster = LoadOrCreateRoster();

        string characterFolder = $"Assets/Data/Characters/{slug}";
        string weaponFolder = $"Assets/Data/Weapons/{slug}";
        string skillFolder = $"Assets/Data/Skills/{slug}";

        EnsureFolder(characterFolder);
        EnsureFolder(weaponFolder);
        EnsureFolder(skillFolder);

        SkillData skillAsset = CreateAsset<SkillData>($"{skillFolder}/Skill_{slug}.asset");
        WeaponData weaponAsset = CreateAsset<WeaponData>($"{weaponFolder}/Weapon_{slug}.asset");
        CharacterData characterAsset = CreateAsset<CharacterData>($"{characterFolder}/Hero_{slug}.asset");

        ConfigureSkill(skillAsset, slug, runtimeDefinition);
        ConfigureWeapon(weaponAsset, slug, skillAsset);
        ConfigureCharacter(characterAsset, slug, weaponAsset, defaultHero);
        AddCharacterToRoster(roster, characterAsset);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.SetDirty(roster);
        Selection.activeObject = characterAsset;
        EditorGUIUtility.PingObject(characterAsset);

        EditorUtility.DisplayDialog("Character Pack Created", $"Created {characterAsset.DisplayName}, {weaponAsset.DisplayName}, and {skillAsset.DisplayName}.", "OK");
    }

    private CharacterRoster LoadOrCreateRoster()
    {
        EnsureFolder("Assets/Resources");

        CharacterRoster roster = AssetDatabase.LoadAssetAtPath<CharacterRoster>(RosterPath);

        if (roster != null)
        {
            return roster;
        }

        roster = ScriptableObject.CreateInstance<CharacterRoster>();
        AssetDatabase.CreateAsset(roster, RosterPath);
        return roster;
    }

    private void ConfigureSkill(SkillData skillAsset, string slug, SkillRuntimeDefinition runtimeDefinition)
    {
        SerializedObject serializedObject = new(skillAsset);
        serializedObject.FindProperty("skillId").stringValue = $"{slug}_skill";
        serializedObject.FindProperty("displayName").stringValue = skillName;
        serializedObject.FindProperty("description").stringValue = skillDescription;
        serializedObject.FindProperty("hitType").enumValueIndex = (int)skillHitType;
        serializedObject.FindProperty("element").enumValueIndex = (int)skillElement;
        serializedObject.FindProperty("runtimeDefinition").objectReferenceValue = runtimeDefinition;
        serializedObject.FindProperty("icon").objectReferenceValue = null;
        serializedObject.FindProperty("visualPrefab").objectReferenceValue = null;
        serializedObject.FindProperty("baseDamage").floatValue = baseDamage;
        serializedObject.FindProperty("flatDamageBonus").floatValue = flatDamageBonus;
        serializedObject.FindProperty("critChance").floatValue = critChance;
        serializedObject.FindProperty("critMultiplier").floatValue = critMultiplier;
        serializedObject.FindProperty("areaBonus").floatValue = areaBonus;
        serializedObject.FindProperty("cooldown").floatValue = skillCooldown;
        serializedObject.FindProperty("cooldownCoefficient").floatValue = 1f;
        serializedObject.FindProperty("cooldownDivider").floatValue = skillFrequency;
        serializedObject.FindProperty("areaMultiplier").floatValue = areaMultiplier;
        serializedObject.FindProperty("repeatChance").floatValue = doubleAttackChance;
        serializedObject.FindProperty("globalSkill").boolValue = false;
        serializedObject.FindProperty("maxRank").intValue = 1;

        ApplyStringArray(serializedObject.FindProperty("tags"), ParseTags(tagList));
        serializedObject.FindProperty("appliedStatuses").arraySize = 0;
        serializedObject.FindProperty("attackDefinitions").arraySize = 0;
        serializedObject.FindProperty("parameters").arraySize = 0;

        SerializedProperty configurableAttacks = serializedObject.FindProperty("configurableAttacks");
        configurableAttacks.arraySize = 1;
        SerializedProperty attack = configurableAttacks.GetArrayElementAtIndex(0);
        attack.FindPropertyRelative("attackId").stringValue = $"{slug}_attack";
        attack.FindPropertyRelative("baseCooldown").floatValue = skillCooldown;
        attack.FindPropertyRelative("cooldownReductionPerRank").floatValue = cooldownReductionPerRank;
        attack.FindPropertyRelative("targetingMode").enumValueIndex = (int)targetingMode;
        attack.FindPropertyRelative("targetCount").intValue = Mathf.Max(1, targetCount);
        attack.FindPropertyRelative("maxDistance").floatValue = Mathf.Max(1f, maxDistance);
        attack.FindPropertyRelative("deliveryMode").enumValueIndex = (int)deliveryMode;
        attack.FindPropertyRelative("projectileSpeed").floatValue = projectileSpeed;
        attack.FindPropertyRelative("projectileLifetime").floatValue = projectileLifetime;
        attack.FindPropertyRelative("spawnHeight").floatValue = spawnHeight;
        attack.FindPropertyRelative("areaRadius").floatValue = areaRadius;
        attack.FindPropertyRelative("areaVisualLifetime").floatValue = areaVisualLifetime;
        attack.FindPropertyRelative("baseDamage").floatValue = 0f;
        attack.FindPropertyRelative("damagePerRank").floatValue = 0f;
        attack.FindPropertyRelative("ownerDamageMultiplier").floatValue = 1f;

        serializedObject.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(skillAsset);
    }

    private void ConfigureWeapon(WeaponData weaponAsset, string slug, SkillData skillAsset)
    {
        SerializedObject serializedObject = new(weaponAsset);
        serializedObject.FindProperty("weaponId").stringValue = $"{slug}_weapon";
        serializedObject.FindProperty("displayName").stringValue = weaponName;
        serializedObject.FindProperty("icon").objectReferenceValue = null;
        serializedObject.FindProperty("startingSkill").objectReferenceValue = skillAsset;

        SerializedProperty uniqueSkillPool = serializedObject.FindProperty("uniqueSkillPool");
        uniqueSkillPool.arraySize = 1;
        uniqueSkillPool.GetArrayElementAtIndex(0).objectReferenceValue = skillAsset;

        serializedObject.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(weaponAsset);
    }

    private void ConfigureCharacter(CharacterData characterAsset, string slug, WeaponData weaponAsset, CharacterData defaultHero)
    {
        SerializedObject serializedObject = new(characterAsset);
        serializedObject.FindProperty("characterId").stringValue = slug.ToLowerInvariant();
        serializedObject.FindProperty("displayName").stringValue = characterName;
        serializedObject.FindProperty("modelPrefab").objectReferenceValue = modelPrefab;

        SerializedProperty baseStatsProperty = serializedObject.FindProperty("baseStats");
        baseStatsProperty.arraySize = 9;
        SetStat(baseStatsProperty, 0, StatType.MaxHealth, maxHealth);
        SetStat(baseStatsProperty, 1, StatType.MoveSpeed, moveSpeed);
        SetStat(baseStatsProperty, 2, StatType.PickupRadius, pickupRadius);
        SetStat(baseStatsProperty, 3, StatType.Defense, defense);
        SetStat(baseStatsProperty, 4, StatType.ParryChance, parryChance);
        SetStat(baseStatsProperty, 5, StatType.ExperienceMultiplier, experienceMultiplier);
        SetStat(baseStatsProperty, 6, StatType.DashCharges, dashCharges);
        SetStat(baseStatsProperty, 7, StatType.HealthRegenPercent, healthRegenPercent);
        SetStat(baseStatsProperty, 8, StatType.LifeTotemCount, lifeTotemCount);

        serializedObject.FindProperty("startingWeapon").objectReferenceValue = weaponAsset;
        SerializedProperty availableWeapons = serializedObject.FindProperty("availableWeapons");
        availableWeapons.arraySize = 1;
        availableWeapons.GetArrayElementAtIndex(0).objectReferenceValue = weaponAsset;

        SerializedProperty globalUpgradePool = serializedObject.FindProperty("globalUpgradePool");
        SerializedProperty defaultUpgradePool = new SerializedObject(defaultHero).FindProperty("globalUpgradePool");
        globalUpgradePool.arraySize = defaultUpgradePool.arraySize;

        for (int i = 0; i < defaultUpgradePool.arraySize; i++)
        {
            globalUpgradePool.GetArrayElementAtIndex(i).objectReferenceValue = defaultUpgradePool.GetArrayElementAtIndex(i).objectReferenceValue;
        }

        serializedObject.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(characterAsset);
    }

    private static void AddCharacterToRoster(CharacterRoster roster, CharacterData characterAsset)
    {
        SerializedObject serializedObject = new(roster);
        SerializedProperty characters = serializedObject.FindProperty("characters");

        for (int i = 0; i < characters.arraySize; i++)
        {
            if (characters.GetArrayElementAtIndex(i).objectReferenceValue == characterAsset)
            {
                return;
            }
        }

        int index = characters.arraySize;
        characters.arraySize += 1;
        characters.GetArrayElementAtIndex(index).objectReferenceValue = characterAsset;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void ApplyStringArray(SerializedProperty property, string[] values)
    {
        property.arraySize = values.Length;

        for (int i = 0; i < values.Length; i++)
        {
            property.GetArrayElementAtIndex(i).stringValue = values[i];
        }
    }

    private static void SetStat(SerializedProperty baseStatsProperty, int index, StatType statType, float value)
    {
        SerializedProperty element = baseStatsProperty.GetArrayElementAtIndex(index);
        element.FindPropertyRelative("statType").enumValueIndex = (int)statType;
        element.FindPropertyRelative("value").floatValue = value;
    }

    private static string[] ParseTags(string rawTags)
    {
        return rawTags
            .Split(',')
            .Select(tag => tag.Trim())
            .Where(tag => !string.IsNullOrWhiteSpace(tag))
            .Distinct()
            .ToArray();
    }

    private static T CreateAsset<T>(string assetPath) where T : ScriptableObject
    {
        string uniquePath = AssetDatabase.GenerateUniqueAssetPath(assetPath);
        T asset = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(asset, uniquePath);
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

    private static string Slugify(string value)
    {
        StringBuilder builder = new();

        foreach (char symbol in value)
        {
            if (char.IsLetterOrDigit(symbol))
            {
                builder.Append(symbol);
            }
        }

        return builder.ToString();
    }
}
