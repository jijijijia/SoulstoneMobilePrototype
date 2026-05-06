using System.IO;
using UnityEditor;
using UnityEngine;

public static class SkillTreeContentCreator
{
    public const string SkillTreeFolder = "Assets/Data/SkillTree";
    public const string DefaultTreePath = SkillTreeFolder + "/SkillTree_Default.asset";

    [MenuItem("Soulstone/Content/Create Default Skill Tree")]
    public static void CreateDefaultSkillTree()
    {
        Directory.CreateDirectory(SkillTreeFolder);

        SkillTreeNodeData vitality = CreateNode(
            "node_vitality",
            "Живучесть",
            "Повышает максимальное здоровье героя.",
            maxLevel: 5,
            cost: 100,
            statType: StatType.MaxHealth,
            additive: 10f,
            multiplier: 0f,
            requiredIds: null);

        SkillTreeNodeData swiftness = CreateNode(
            "node_swiftness",
            "Быстрый шаг",
            "Повышает скорость передвижения.",
            maxLevel: 5,
            cost: 120,
            statType: StatType.MoveSpeed,
            additive: 0.15f,
            multiplier: 0f,
            requiredIds: new[] { vitality.NodeId });

        SkillTreeNodeData scholar = CreateNode(
            "node_scholar",
            "Ученик душ",
            "Повышает количество получаемого опыта.",
            maxLevel: 4,
            cost: 140,
            statType: StatType.ExperienceMultiplier,
            additive: 0f,
            multiplier: 0.05f,
            requiredIds: new[] { vitality.NodeId });

        SkillTreeNodeData veteran = CreateEffectNode(
            "node_veteran",
            "Ветеран",
            "Герой начинает забег с дополнительным уровнем.",
            maxLevel: 3,
            cost: 250,
            effectType: SkillTreeUnlockEffectType.StartingLevelBonus,
            value: 1f,
            requiredIds: new[] { scholar.NodeId });

        SkillTreeNodeData arsenal = CreateEffectNode(
            "node_arsenal",
            "Расширенный арсенал",
            "Увеличивает лимит активных навыков.",
            maxLevel: 2,
            cost: 350,
            effectType: SkillTreeUnlockEffectType.ActiveSkillSlotBonus,
            value: 1f,
            requiredIds: new[] { swiftness.NodeId });

        SkillTreeNodeData fortune = CreateEffectNode(
            "node_fortune",
            "Зов редкости",
            "Повышает вес редких улучшений при повышении уровня.",
            maxLevel: 5,
            cost: 180,
            effectType: SkillTreeUnlockEffectType.UpgradeRarityChance,
            value: 0.08f,
            requiredIds: new[] { scholar.NodeId },
            rarity: UpgradeRarity.Rare);

        SkillTreeNodeData treasure = CreateEffectNode(
            "node_treasure",
            "Охотник за реликвиями",
            "Повышает награды после забега.",
            maxLevel: 5,
            cost: 200,
            effectType: SkillTreeUnlockEffectType.RewardMultiplier,
            value: 0.05f,
            requiredIds: new[] { fortune.NodeId });

        SkillTreeData tree = AssetDatabase.LoadAssetAtPath<SkillTreeData>(DefaultTreePath);

        if (tree == null)
        {
            tree = ScriptableObject.CreateInstance<SkillTreeData>();
            AssetDatabase.CreateAsset(tree, DefaultTreePath);
        }

        SerializedObject serializedTree = new(tree);
        SetObjectArray(serializedTree.FindProperty("nodes"), new Object[] { vitality, swiftness, scholar, veteran, arsenal, fortune, treasure });
        SetStringArray(serializedTree.FindProperty("startingAvailableNodeIds"), new[] { vitality.NodeId });
        serializedTree.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(tree);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Selection.activeObject = tree;

        Debug.Log("Default account skill tree created at " + DefaultTreePath);
    }

    private static SkillTreeNodeData CreateNode(string nodeId, string displayName, string description, int maxLevel, int cost, StatType statType, float additive, float multiplier, string[] requiredIds)
    {
        SkillTreeNodeData node = LoadOrCreateNode(nodeId);
        SerializedObject serialized = new(node);
        serialized.FindProperty("nodeId").stringValue = nodeId;
        serialized.FindProperty("displayName").stringValue = displayName;
        serialized.FindProperty("description").stringValue = description;
        serialized.FindProperty("maxLevel").intValue = maxLevel;
        serialized.FindProperty("currencyType").enumValueIndex = (int)CurrencyType.SoulShards;
        SetCosts(serialized.FindProperty("costPerLevel"), maxLevel, cost);
        SetStringArray(serialized.FindProperty("requiredNodeIds"), requiredIds);

        SerializedProperty modifiers = serialized.FindProperty("statModifiers");
        modifiers.arraySize = 1;
        SerializedProperty modifier = modifiers.GetArrayElementAtIndex(0);
        modifier.FindPropertyRelative("statType").enumValueIndex = (int)statType;
        modifier.FindPropertyRelative("additive").floatValue = additive;
        modifier.FindPropertyRelative("multiplier").floatValue = multiplier;

        serialized.FindProperty("unlockEffects").arraySize = 0;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(node);
        return node;
    }

    private static SkillTreeNodeData CreateEffectNode(string nodeId, string displayName, string description, int maxLevel, int cost, SkillTreeUnlockEffectType effectType, float value, string[] requiredIds, UpgradeRarity rarity = UpgradeRarity.Common)
    {
        SkillTreeNodeData node = LoadOrCreateNode(nodeId);
        SerializedObject serialized = new(node);
        serialized.FindProperty("nodeId").stringValue = nodeId;
        serialized.FindProperty("displayName").stringValue = displayName;
        serialized.FindProperty("description").stringValue = description;
        serialized.FindProperty("maxLevel").intValue = maxLevel;
        serialized.FindProperty("currencyType").enumValueIndex = (int)CurrencyType.SoulShards;
        SetCosts(serialized.FindProperty("costPerLevel"), maxLevel, cost);
        SetStringArray(serialized.FindProperty("requiredNodeIds"), requiredIds);
        serialized.FindProperty("statModifiers").arraySize = 0;

        SerializedProperty effects = serialized.FindProperty("unlockEffects");
        effects.arraySize = 1;
        SerializedProperty effect = effects.GetArrayElementAtIndex(0);
        effect.FindPropertyRelative("effectType").enumValueIndex = (int)effectType;
        effect.FindPropertyRelative("value").floatValue = value;
        effect.FindPropertyRelative("rarity").enumValueIndex = (int)rarity;

        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(node);
        return node;
    }

    private static SkillTreeNodeData LoadOrCreateNode(string nodeId)
    {
        string path = $"{SkillTreeFolder}/SkillTreeNode_{nodeId}.asset";
        SkillTreeNodeData node = AssetDatabase.LoadAssetAtPath<SkillTreeNodeData>(path);

        if (node != null)
        {
            return node;
        }

        node = ScriptableObject.CreateInstance<SkillTreeNodeData>();
        AssetDatabase.CreateAsset(node, path);
        return node;
    }

    private static void SetCosts(SerializedProperty costs, int maxLevel, int baseCost)
    {
        costs.arraySize = maxLevel;

        for (int i = 0; i < maxLevel; i++)
        {
            SerializedProperty entry = costs.GetArrayElementAtIndex(i);
            entry.FindPropertyRelative("currencyType").enumValueIndex = (int)CurrencyType.SoulShards;
            entry.FindPropertyRelative("amount").intValue = baseCost * (i + 1);
        }
    }

    private static void SetStringArray(SerializedProperty property, string[] values)
    {
        values ??= new string[0];
        property.arraySize = values.Length;

        for (int i = 0; i < values.Length; i++)
        {
            property.GetArrayElementAtIndex(i).stringValue = values[i];
        }
    }

    private static void SetObjectArray(SerializedProperty property, Object[] values)
    {
        values ??= new Object[0];
        property.arraySize = values.Length;

        for (int i = 0; i < values.Length; i++)
        {
            property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }
    }
}
