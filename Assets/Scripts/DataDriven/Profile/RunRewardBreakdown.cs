using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class RunRewardBreakdown
{
    public bool isVictory;
    public float runTimeSeconds;
    public int playerLevel;
    public int killCount;
    public int eliteKills;
    public int miniBossKills;
    public int bossKills;
    public string mapId;
    public string mapName;
    public float rewardMultiplier = 1f;
    public List<RunRewardAmount> rewards = new();

    public bool HasRewards => rewards != null && rewards.Count > 0;

    public int GetAmount(CurrencyType currencyType)
    {
        if (rewards == null)
        {
            return 0;
        }

        for (int i = 0; i < rewards.Count; i++)
        {
            if (rewards[i] != null && rewards[i].currencyType == currencyType)
            {
                return Mathf.Max(0, rewards[i].amount);
            }
        }

        return 0;
    }
}

[Serializable]
public sealed class RunRewardAmount
{
    public CurrencyType currencyType;
    public int amount;
}
