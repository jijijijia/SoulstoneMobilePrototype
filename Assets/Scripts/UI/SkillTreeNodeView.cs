using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class SkillTreeNodeView : MonoBehaviour
{
    [SerializeField] private Image background;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private GameObject lockedState;
    [SerializeField] private GameObject availableState;
    [SerializeField] private GameObject ownedState;
    [SerializeField] private Button button;

    private SkillTreeNodeData nodeData;

    public SkillTreeNodeData NodeData => nodeData;

    private void Reset()
    {
        background = GetComponent<Image>();
        iconImage = transform.Find("Icon") != null ? transform.Find("Icon").GetComponent<Image>() : null;
        titleText = transform.Find("Title") != null ? transform.Find("Title").GetComponent<TMP_Text>() : null;
        levelText = transform.Find("Level") != null ? transform.Find("Level").GetComponent<TMP_Text>() : null;
        button = GetComponent<Button>();
    }

    public void Configure(SkillTreeData tree, SkillTreeNodeData node, System.Action<SkillTreeNodeData> onSelected)
    {
        nodeData = node;
        gameObject.SetActive(node != null);

        if (node == null)
        {
            return;
        }

        int level = SkillTreeSystem.GetNodeLevel(node.NodeId);
        bool owned = SkillTreeSystem.IsOwned(node);
        bool available = SkillTreeSystem.IsAvailable(tree, node);

        if (titleText != null)
        {
            titleText.text = node.DisplayName;
        }

        if (levelText != null)
        {
            levelText.text = $"{level}/{node.MaxLevel}";
        }

        if (iconImage != null)
        {
            iconImage.sprite = node.Icon;
            iconImage.enabled = node.Icon != null;
        }

        if (lockedState != null)
        {
            lockedState.SetActive(!available && !owned);
        }

        if (availableState != null)
        {
            availableState.SetActive(available && !owned);
        }

        if (ownedState != null)
        {
            ownedState.SetActive(owned);
        }

        if (background != null)
        {
            background.color = owned
                ? new Color(0.16f, 0.38f, 0.21f, 0.92f)
                : available
                    ? new Color(0.33f, 0.25f, 0.08f, 0.92f)
                    : new Color(0.08f, 0.09f, 0.12f, 0.82f);
        }

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.interactable = true;
            button.onClick.AddListener(() => onSelected?.Invoke(node));
        }
    }

    public void SetSelected(bool selected)
    {
        if (background == null || nodeData == null)
        {
            return;
        }

        if (selected)
        {
            background.color = new Color(0.9f, 0.55f, 0.05f, 0.95f);
        }
    }
}
