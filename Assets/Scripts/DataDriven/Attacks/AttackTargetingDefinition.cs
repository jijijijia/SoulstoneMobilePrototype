using System.Collections.Generic;
using UnityEngine;

public abstract class AttackTargetingDefinition : ScriptableObject
{
    public abstract void ResolveTargets(AttackRuntimeContext context, List<AttackTargetData> results);
}
