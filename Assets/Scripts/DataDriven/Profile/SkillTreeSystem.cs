using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class SkillTreeSystem
{
    private const string ModifierSourceId = "account_skill_tree";
    private static SkillTreeData activeTree;

    public static event Action SkillTreeChanged;

    public static SkillTreeData ActiveTree => activeTree;

    public static void SetActiveTree(SkillTreeData tree)
    {
        activeTree = tree;
    }

    public static int GetNodeLevel(string nodeId)
    {
        SkillTreeNodeProgress progress = GetProgress(nodeId, createIfMissing: false);
        return progress != null ? Mathf.Max(0, progress.level) : 0;
    }

    public static bool IsOwned(SkillTreeNodeData node)
    {
        return node != null && GetNodeLevel(node.NodeId) >= node.MaxLevel;
    }

    public static bool IsAvailable(SkillTreeData tree, SkillTreeNodeData node)
    {
        if (node == null)
        {
            return false;
        }

        if (GetNodeLevel(node.NodeId) >= node.MaxLevel)
        {
            return false;
        }

        string[] requiredNodeIds = node.RequiredNodeIds;

        if (requiredNodeIds == null || requiredNodeIds.Length == 0)
        {
            return IsStartingNode(tree, node);
        }

        for (int i = 0; i < requiredNodeIds.Length; i++)
        {
            string requiredId = requiredNodeIds[i];

            if (string.IsNullOrWhiteSpace(requiredId))
            {
                continue;
            }

            SkillTreeNodeData requiredNode = tree != null ? tree.GetNode(requiredId) : null;
            int requiredLevel = GetNodeLevel(requiredId);
            int requiredMaxLevel = requiredNode != null ? requiredNode.MaxLevel : 1;

            if (requiredLevel < requiredMaxLevel)
            {
                return false;
            }
        }

        return true;
    }

    public static bool CanImprove(SkillTreeData tree, SkillTreeNodeData node)
    {
        if (!IsAvailable(tree, node))
        {
            return false;
        }

        CurrencyAmount cost = node.GetCostForLevel(GetNodeLevel(node.NodeId));
        return cost == null || cost.amount <= 0 || SaveSystem.Wallet.CanAfford(cost.currencyType, cost.amount);
    }

    public static bool TryImprove(SkillTreeData tree, SkillTreeNodeData node)
    {
        if (!CanImprove(tree, node))
        {
            return false;
        }

        SkillTreeNodeProgress progress = GetProgress(node.NodeId, createIfMissing: true);

        if (progress == null)
        {
            return false;
        }

        CurrencyAmount cost = node.GetCostForLevel(progress.level);

        if (cost != null && cost.amount > 0 && !SaveSystem.Wallet.Spend(cost.currencyType, cost.amount, save: false))
        {
            return false;
        }

        progress.level = Mathf.Clamp(progress.level + 1, 0, node.MaxLevel);
        SaveSystem.Save();
        SkillTreeChanged?.Invoke();
        return true;
    }

    public static void ApplyGlobalBonuses(RuntimeStats runtimeStats, SkillTreeData tree = null)
    {
        if (runtimeStats == null)
        {
            return;
        }

        StatModifierData[] modifiers = GetGlobalStatModifiers(tree);

        if (modifiers.Length == 0)
        {
            runtimeStats.RemoveModifiers(ModifierSourceId);
            return;
        }

        runtimeStats.AddModifiers(ModifierSourceId, modifiers);
    }

    public static StatModifierData[] GetGlobalStatModifiers(SkillTreeData tree = null)
    {
        List<StatModifierData> result = new();
        SkillTreeNodeData[] nodes = GetNodes(tree);

        for (int i = 0; i < nodes.Length; i++)
        {
            SkillTreeNodeData node = nodes[i];

            if (node == null || node.StatModifiers == null)
            {
                continue;
            }

            int level = GetNodeLevel(node.NodeId);

            if (level <= 0)
            {
                continue;
            }

            for (int j = 0; j < node.StatModifiers.Length; j++)
            {
                StatModifierData modifier = node.StatModifiers[j];
                result.Add(new StatModifierData
                {
                    statType = modifier.statType,
                    additive = modifier.additive * level,
                    multiplier = modifier.multiplier * level
                });
            }
        }

        return result.ToArray();
    }

    public static int GetStartingLevelBonus(SkillTreeData tree = null)
    {
        return Mathf.FloorToInt(GetUnlockEffectTotal(SkillTreeUnlockEffectType.StartingLevelBonus, tree));
    }

    public static int GetActiveSkillSlotBonus(SkillTreeData tree = null)
    {
        return Mathf.FloorToInt(GetUnlockEffectTotal(SkillTreeUnlockEffectType.ActiveSkillSlotBonus, tree));
    }

    public static float GetRewardMultiplier(SkillTreeData tree = null)
    {
        return 1f + Mathf.Max(0f, GetUnlockEffectTotal(SkillTreeUnlockEffectType.RewardMultiplier, tree));
    }

    public static float GetRarityWeightMultiplier(UpgradeRarity rarity, SkillTreeData tree = null)
    {
        float bonus = 0f;
        SkillTreeNodeData[] nodes = GetNodes(tree);

        for (int i = 0; i < nodes.Length; i++)
        {
            SkillTreeNodeData node = nodes[i];

            if (node == null || node.UnlockEffects == null)
            {
                continue;
            }

            int level = GetNodeLevel(node.NodeId);

            if (level <= 0)
            {
                continue;
            }

            for (int j = 0; j < node.UnlockEffects.Length; j++)
            {
                SkillTreeUnlockEffectData effect = node.UnlockEffects[j];

                if (effect.effectType == SkillTreeUnlockEffectType.UpgradeRarityChance && effect.rarity == rarity)
                {
                    bonus += effect.value * level;
                }
            }
        }

        return 1f + Mathf.Max(0f, bonus);
    }

    public static string BuildNodeDescription(SkillTreeData tree, SkillTreeNodeData node)
    {
        if (node == null)
        {
            return "Выбери узел дерева навыков.";
        }

        int level = GetNodeLevel(node.NodeId);
        CurrencyAmount cost = node.GetCostForLevel(level);
        StringBuilder sb = new();
        sb.AppendLine(node.DisplayName);
        sb.Append("Уровень: ");
        sb.Append(level);
        sb.Append('/');
        sb.AppendLine(node.MaxLevel.ToString());
        sb.AppendLine();

        if (!string.IsNullOrWhiteSpace(node.Description))
        {
            sb.AppendLine(node.Description);
            sb.AppendLine();
        }

        AppendStatModifiers(sb, node);
        AppendUnlockEffects(sb, node);

        if (level >= node.MaxLevel)
        {
            sb.AppendLine("Статус: изучено полностью.");
        }
        else if (!IsAvailable(tree, node))
        {
            sb.AppendLine("Статус: закрыто.");
            sb.AppendLine(BuildRequirementText(tree, node));
        }
        else if (cost != null && cost.amount > 0)
        {
            sb.Append("Стоимость: ");
            sb.Append(GetCurrencyDisplayName(cost.currencyType));
            sb.Append(' ');
            sb.AppendLine(cost.amount.ToString());
        }
        else
        {
            sb.AppendLine("Стоимость: бесплатно.");
        }

        return sb.ToString();
    }

    public static string BuildRequirementText(SkillTreeData tree, SkillTreeNodeData node)
    {
        if (node == null)
        {
            return string.Empty;
        }

        string[] requiredNodeIds = node.RequiredNodeIds;

        if (requiredNodeIds == null || requiredNodeIds.Length == 0)
        {
            return IsStartingNode(tree, node) ? "Доступно с начала." : "Нет связи со стартовыми узлами.";
        }

        StringBuilder sb = new("Требуется: ");
        bool hasRequirement = false;

        for (int i = 0; i < requiredNodeIds.Length; i++)
        {
            string requiredId = requiredNodeIds[i];

            if (string.IsNullOrWhiteSpace(requiredId))
            {
                continue;
            }

            SkillTreeNodeData requiredNode = tree != null ? tree.GetNode(requiredId) : null;

            if (hasRequirement)
            {
                sb.Append(", ");
            }

            sb.Append(requiredNode != null ? requiredNode.DisplayName : requiredId);
            hasRequirement = true;
        }

        return hasRequirement ? sb.ToString() : "Нет требований.";
    }

    private static float GetUnlockEffectTotal(SkillTreeUnlockEffectType effectType, SkillTreeData tree)
    {
        float total = 0f;
        SkillTreeNodeData[] nodes = GetNodes(tree);

        for (int i = 0; i < nodes.Length; i++)
        {
            SkillTreeNodeData node = nodes[i];

            if (node == null || node.UnlockEffects == null)
            {
                continue;
            }

            int level = GetNodeLevel(node.NodeId);

            if (level <= 0)
            {
                continue;
            }

            for (int j = 0; j < node.UnlockEffects.Length; j++)
            {
                SkillTreeUnlockEffectData effect = node.UnlockEffects[j];

                if (effect.effectType == effectType)
                {
                    total += effect.value * level;
                }
            }
        }

        return total;
    }

    private static SkillTreeNodeData[] GetNodes(SkillTreeData tree)
    {
        tree ??= activeTree;
        return tree != null && tree.Nodes != null ? tree.Nodes : Array.Empty<SkillTreeNodeData>();
    }

    private static SkillTreeNodeProgress GetProgress(string nodeId, bool createIfMissing)
    {
        if (string.IsNullOrWhiteSpace(nodeId))
        {
            return null;
        }

        PlayerProfileData profile = SaveSystem.CurrentProfile;

        if (profile.skillTreeProgress == null)
        {
            profile.skillTreeProgress = new List<SkillTreeNodeProgress>();
        }

        for (int i = 0; i < profile.skillTreeProgress.Count; i++)
        {
            SkillTreeNodeProgress progress = profile.skillTreeProgress[i];

            if (progress != null && string.Equals(progress.nodeId, nodeId, StringComparison.OrdinalIgnoreCase))
            {
                progress.level = Mathf.Max(0, progress.level);
                return progress;
            }
        }

        if (!createIfMissing)
        {
            return null;
        }

        SkillTreeNodeProgress created = new()
        {
            nodeId = nodeId,
            level = 0
        };
        profile.skillTreeProgress.Add(created);
        return created;
    }

    private static bool IsStartingNode(SkillTreeData tree, SkillTreeNodeData node)
    {
        if (tree == null || node == null)
        {
            return true;
        }

        string[] startingIds = tree.StartingAvailableNodeIds;

        if (startingIds == null || startingIds.Length == 0)
        {
            return node.RequiredNodeIds == null || node.RequiredNodeIds.Length == 0;
        }

        for (int i = 0; i < startingIds.Length; i++)
        {
            if (string.Equals(startingIds[i], node.NodeId, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static void AppendStatModifiers(StringBuilder sb, SkillTreeNodeData node)
    {
        StatModifierData[] modifiers = node.StatModifiers;

        if (modifiers == null || modifiers.Length == 0)
        {
            return;
        }

        sb.AppendLine("Бонусы за уровень:");

        for (int i = 0; i < modifiers.Length; i++)
        {
            sb.Append("- ");
            sb.Append(modifiers[i].statType);

            if (Mathf.Abs(modifiers[i].additive) > 0.0001f)
            {
                sb.Append(" +");
                sb.Append(modifiers[i].additive);
            }

            if (Mathf.Abs(modifiers[i].multiplier) > 0.0001f)
            {
                sb.Append(" +");
                sb.Append(Mathf.RoundToInt(modifiers[i].multiplier * 100f));
                sb.Append('%');
            }

            sb.AppendLine();
        }

        sb.AppendLine();
    }

    private static void AppendUnlockEffects(StringBuilder sb, SkillTreeNodeData node)
    {
        SkillTreeUnlockEffectData[] effects = node.UnlockEffects;

        if (effects == null || effects.Length == 0)
        {
            return;
        }

        sb.AppendLine("Особые эффекты:");

        for (int i = 0; i < effects.Length; i++)
        {
            sb.Append("- ");
            sb.Append(effects[i].effectType);

            if (effects[i].effectType == SkillTreeUnlockEffectType.UpgradeRarityChance)
            {
                sb.Append(" ");
                sb.Append(effects[i].rarity);
            }

            sb.Append(" +");
            sb.Append(effects[i].value);
            sb.AppendLine();
        }

        sb.AppendLine();
    }

    public static string GetCurrencyDisplayName(CurrencyType currencyType)
    {
        return currencyType switch
        {
            CurrencyType.SoulShards => "осколки душ",
            CurrencyType.RedCrystal => "красные кристаллы",
            CurrencyType.BlueCrystal => "синие кристаллы",
            CurrencyType.AmberCrystal => "янтарные кристаллы",
            CurrencyType.GreenCrystal => "зелёные кристаллы",
            CurrencyType.VoidCrystal => "кристаллы бездны",
            CurrencyType.RuneDust => "руническая пыль",
            CurrencyType.WeaponOre => "материалы оружия",
            _ => currencyType.ToString()
        };
    }
}
