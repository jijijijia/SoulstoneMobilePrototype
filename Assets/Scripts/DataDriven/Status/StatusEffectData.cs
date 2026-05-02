using System;
using UnityEngine;

[Serializable]
public struct StatusEffectData
{
    public StatusEffectType effectType;
    public float duration;
    public float potency;
    public int maxStacks;
    public float tickInterval;
    public bool refreshDuration;
}
