using UnityEngine;

[CreateAssetMenu(fileName = "SkillTree_Default", menuName = "Soulstone/Skill Tree/Tree")]
public class SkillTreeData : ScriptableObject
{
    [SerializeField] private SkillTreeNodeData[] nodes;
    [SerializeField] private string[] startingAvailableNodeIds;

    public SkillTreeNodeData[] Nodes => nodes;
    public string[] StartingAvailableNodeIds => startingAvailableNodeIds;

    public SkillTreeNodeData GetNode(string nodeId)
    {
        if (string.IsNullOrWhiteSpace(nodeId) || nodes == null)
        {
            return null;
        }

        for (int i = 0; i < nodes.Length; i++)
        {
            SkillTreeNodeData node = nodes[i];

            if (node != null && string.Equals(node.NodeId, nodeId, System.StringComparison.OrdinalIgnoreCase))
            {
                return node;
            }
        }

        return null;
    }
}
