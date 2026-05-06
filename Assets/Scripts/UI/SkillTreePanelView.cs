using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class SkillTreePanelView : MonoBehaviour
{
    [SerializeField] private SkillTreeData skillTreeData;
    [SerializeField] private SkillTreeNodeView[] nodeViews;
    [SerializeField] private Image[] connectionLines;
    [SerializeField] private TMP_Text tooltipText;
    [SerializeField] private Button improveButton;

    private SkillTreeNodeData selectedNode;

    public SkillTreeData SkillTreeData => skillTreeData;

    private void Awake()
    {
        if (nodeViews == null || nodeViews.Length == 0)
        {
            nodeViews = GetComponentsInChildren<SkillTreeNodeView>(true);
        }
    }

    private void OnEnable()
    {
        SkillTreeSystem.SetActiveTree(skillTreeData);
        SkillTreeSystem.SkillTreeChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        SkillTreeSystem.SkillTreeChanged -= Refresh;
    }

    public void Refresh()
    {
        SkillTreeNodeData[] nodes = skillTreeData != null ? skillTreeData.Nodes : null;
        int nodeCount = nodes != null ? nodes.Length : 0;

        if (nodeViews != null)
        {
            for (int i = 0; i < nodeViews.Length; i++)
            {
                if (nodeViews[i] == null)
                {
                    continue;
                }

                SkillTreeNodeData node = i < nodeCount ? nodes[i] : null;
                nodeViews[i].Configure(skillTreeData, node, SelectNode);
                nodeViews[i].SetSelected(node != null && node == selectedNode);
            }
        }

        RefreshConnectionLines();
        RefreshTooltip();
    }

    public void SelectNode(SkillTreeNodeData node)
    {
        selectedNode = node;
        Refresh();
    }

    public void ImproveSelectedNode()
    {
        if (selectedNode != null && SkillTreeSystem.TryImprove(skillTreeData, selectedNode))
        {
            Refresh();
        }
    }

    private void RefreshTooltip()
    {
        if (tooltipText != null)
        {
            tooltipText.text = SkillTreeSystem.BuildNodeDescription(skillTreeData, selectedNode);
        }

        if (improveButton != null)
        {
            improveButton.onClick.RemoveAllListeners();
            improveButton.onClick.AddListener(ImproveSelectedNode);
            improveButton.interactable = selectedNode != null && SkillTreeSystem.CanImprove(skillTreeData, selectedNode);

            TMP_Text label = improveButton.GetComponentInChildren<TMP_Text>(true);

            if (label != null)
            {
                label.text = improveButton.interactable ? "УЛУЧШИТЬ" : "НЕДОСТУПНО";
            }
        }
    }

    private void RefreshConnectionLines()
    {
        if (connectionLines == null)
        {
            return;
        }

        for (int i = 0; i < connectionLines.Length; i++)
        {
            if (connectionLines[i] == null)
            {
                continue;
            }

            connectionLines[i].color = new Color(0.68f, 0.55f, 0.28f, 0.65f);
        }
    }
}
