using UnityEngine;

public abstract class SkillRuntimeDefinition : ScriptableObject
{
    public abstract SkillBehaviourBase CreateRuntime(CharacterSystem owner, SkillData skillData, int rank);
}
