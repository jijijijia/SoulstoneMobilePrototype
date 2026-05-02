using UnityEngine;

public class AttackRuntimeContext
{
    public CharacterSystem Owner { get; set; }
    public SkillRuntimeContext SkillContext { get; set; }
    public AttackDefinition AttackDefinition { get; set; }
    public int Rank { get; set; }
    public float Cooldown { get; set; }
}
