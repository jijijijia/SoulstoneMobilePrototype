using System.Collections.Generic;
using UnityEngine;

public static class RunRewardSystem
{
    private const int BaseCompletionSoulShards = 10;
    private const int SoulShardsPerMinute = 8;
    private const int SoulShardsPerKill = 2;
    private const int SoulShardsPerLevel = 5;
    private const int SoulShardsPerElite = 20;
    private const int SoulShardsPerMiniBoss = 60;
    private const int SoulShardsPerBoss = 150;
    private const int VictorySoulShardsBonus = 100;

    public static RunRewardBreakdown Calculate(
        bool isVictory,
        float runTimeSeconds,
        int playerLevel,
        int killCount,
        int eliteKills,
        int miniBossKills,
        int bossKills,
        MapData mapData,
        float rewardMultiplier)
    {
        float safeMultiplier = Mathf.Max(0.1f, rewardMultiplier * SkillTreeSystem.GetRewardMultiplier());
        int minutesSurvived = Mathf.FloorToInt(Mathf.Max(0f, runTimeSeconds) / 60f);
        int safeLevel = Mathf.Max(1, playerLevel);
        int safeKills = Mathf.Max(0, killCount);
        int safeEliteKills = Mathf.Max(0, eliteKills);
        int safeMiniBossKills = Mathf.Max(0, miniBossKills);
        int safeBossKills = Mathf.Max(0, bossKills);

        RunRewardBreakdown breakdown = new()
        {
            isVictory = isVictory,
            runTimeSeconds = Mathf.Max(0f, runTimeSeconds),
            playerLevel = safeLevel,
            killCount = safeKills,
            eliteKills = safeEliteKills,
            miniBossKills = safeMiniBossKills,
            bossKills = safeBossKills,
            mapId = mapData != null ? mapData.MapId : string.Empty,
            mapName = mapData != null ? mapData.DisplayName : string.Empty,
            rewardMultiplier = safeMultiplier
        };

        int soulShards = BaseCompletionSoulShards
            + minutesSurvived * SoulShardsPerMinute
            + safeKills * SoulShardsPerKill
            + safeLevel * SoulShardsPerLevel
            + safeEliteKills * SoulShardsPerElite
            + safeMiniBossKills * SoulShardsPerMiniBoss
            + safeBossKills * SoulShardsPerBoss
            + (isVictory ? VictorySoulShardsBonus : 0);

        int rareCrystals = Mathf.FloorToInt(minutesSurvived / 5f)
            + safeEliteKills
            + safeMiniBossKills * 2
            + safeBossKills * 5
            + (isVictory ? 2 : 0);

        int weaponOre = Mathf.FloorToInt(safeKills / 20f)
            + safeEliteKills * 2
            + safeMiniBossKills * 4
            + safeBossKills * 8;

        int runeDust = Mathf.FloorToInt(safeLevel / 3f)
            + safeMiniBossKills
            + safeBossKills * 6
            + (isVictory ? 5 : 0);

        AddReward(breakdown.rewards, CurrencyType.SoulShards, ApplyMultiplier(soulShards, safeMultiplier, minimumWhenPositive: 1));
        AddReward(breakdown.rewards, CurrencyType.RedCrystal, ApplyMultiplier(rareCrystals, safeMultiplier, minimumWhenPositive: 0));
        AddReward(breakdown.rewards, CurrencyType.WeaponOre, ApplyMultiplier(weaponOre, safeMultiplier, minimumWhenPositive: 0));
        AddReward(breakdown.rewards, CurrencyType.RuneDust, ApplyMultiplier(runeDust, safeMultiplier, minimumWhenPositive: 0));

        return breakdown;
    }

    public static void AwardToProfile(RunRewardBreakdown breakdown)
    {
        if (breakdown == null || breakdown.rewards == null)
        {
            return;
        }

        CurrencyWallet wallet = SaveSystem.Wallet;

        for (int i = 0; i < breakdown.rewards.Count; i++)
        {
            RunRewardAmount reward = breakdown.rewards[i];

            if (reward == null || reward.amount <= 0)
            {
                continue;
            }

            wallet.Add(reward.currencyType, reward.amount, save: false);
        }

        SaveSystem.Save();
    }

    private static int ApplyMultiplier(int amount, float multiplier, int minimumWhenPositive)
    {
        if (amount <= 0)
        {
            return 0;
        }

        return Mathf.Max(minimumWhenPositive, Mathf.RoundToInt(amount * multiplier));
    }

    private static void AddReward(List<RunRewardAmount> rewards, CurrencyType currencyType, int amount)
    {
        if (rewards == null || amount <= 0)
        {
            return;
        }

        rewards.Add(new RunRewardAmount
        {
            currencyType = currencyType,
            amount = amount
        });
    }
}
