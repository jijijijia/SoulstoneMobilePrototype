using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillCooldownSlotView : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Image cooldownFillImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text rankText;
    [SerializeField] private TMP_Text cooldownText;
    [SerializeField] private GameObject emptyStateRoot;
    [SerializeField] private GameObject filledStateRoot;
    [Header("Ready Pulse")]
    [SerializeField] private float pulseScale = 1.18f;
    [SerializeField] private float pulseDuration = 0.22f;

    private SkillInstance skillInstance;
    private ISkillCooldownSource cooldownSource;
    private bool wasReady;
    private Coroutine readyPulseCoroutine;
    private Vector3 filledRootBaseScale;

    private void Awake()
    {
        ResolveReferences();

        if (filledStateRoot != null)
        {
            filledRootBaseScale = filledStateRoot.transform.localScale;
        }

        Refresh();
    }

    public void Bind(SkillInstance instance)
    {
        skillInstance = instance;
        cooldownSource = instance?.Runtime as ISkillCooldownSource;
        wasReady = cooldownSource == null || cooldownSource.IsReady;
        RefreshStaticData();
        Refresh();
    }

    public void Refresh()
    {
        bool hasSkill = skillInstance?.Data != null;

        if (emptyStateRoot != null)
        {
            emptyStateRoot.SetActive(!hasSkill);
        }

        if (filledStateRoot != null)
        {
            filledStateRoot.SetActive(hasSkill);
        }

        if (!hasSkill)
        {
            SetCooldownVisual(0f, string.Empty);
            return;
        }

        bool isReady = cooldownSource == null || cooldownSource.IsReady;

        if (isReady && !wasReady)
        {
            TriggerReadyPulse();
        }

        wasReady = isReady;

        if (isReady)
        {
            SetCooldownVisual(1f, "Готов");
            return;
        }

        SetCooldownVisual(cooldownSource.CooldownNormalized, $"{cooldownSource.CooldownRemaining:0.0}s");
    }

    private void TriggerReadyPulse()
    {
        if (filledStateRoot == null)
        {
            return;
        }

        if (readyPulseCoroutine != null)
        {
            StopCoroutine(readyPulseCoroutine);
            filledStateRoot.transform.localScale = filledRootBaseScale;
        }

        readyPulseCoroutine = StartCoroutine(ReadyPulseRoutine());
    }

    private IEnumerator ReadyPulseRoutine()
    {
        float elapsed = 0f;

        while (elapsed < pulseDuration)
        {
            float t = elapsed / pulseDuration;
            float scale = 1f + (pulseScale - 1f) * Mathf.Sin(t * Mathf.PI);
            filledStateRoot.transform.localScale = filledRootBaseScale * scale;
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        filledStateRoot.transform.localScale = filledRootBaseScale;
        readyPulseCoroutine = null;
    }

    private void RefreshStaticData()
    {
        ResolveReferences();

        SkillData skillData = skillInstance?.Data;

        if (iconImage != null)
        {
            iconImage.sprite = skillData != null ? skillData.Icon : null;
            iconImage.enabled = skillData != null && skillData.Icon != null;
        }

        if (nameText != null)
        {
            nameText.text = skillData != null ? skillData.DisplayName : string.Empty;
        }

        if (rankText != null)
        {
            rankText.text = skillInstance != null ? $"R{skillInstance.Rank}" : string.Empty;
        }
    }

    private void SetCooldownVisual(float fillAmount, string label)
    {
        if (cooldownFillImage != null)
        {
            cooldownFillImage.fillAmount = Mathf.Clamp01(fillAmount);
        }

        if (cooldownText != null)
        {
            cooldownText.text = label;
        }
    }

    private void ResolveReferences()
    {
        iconImage = iconImage != null ? iconImage : FindChild<Image>("Icon");
        cooldownFillImage = cooldownFillImage != null ? cooldownFillImage : FindChild<Image>("CooldownFill");
        nameText = nameText != null ? nameText : FindChild<TMP_Text>("Name");
        rankText = rankText != null ? rankText : FindChild<TMP_Text>("Rank");
        cooldownText = cooldownText != null ? cooldownText : FindChild<TMP_Text>("Cooldown");
    }

    private T FindChild<T>(string childName) where T : Component
    {
        Transform child = transform.Find(childName);
        return child != null ? child.GetComponent<T>() : null;
    }
}
