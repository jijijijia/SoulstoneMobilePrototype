using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class CombatFeedbackSystem : MonoBehaviour
{
    private const int BasePoolSize = 32;

    [SerializeField] private bool showFloatingText = true;
    [SerializeField] private bool flashTargets = true;
    [SerializeField] private float textLifetime = 0.65f;
    [SerializeField] private float enemyTextSize = 3.8f;
    [SerializeField] private float playerTextSize = 4.2f;
    [SerializeField] private float flashDuration = 0.08f;
    [SerializeField] private Color enemyDamageColor = new(1f, 0.78f, 0.22f, 1f);
    [SerializeField] private Color playerDamageColor = new(1f, 0.18f, 0.12f, 1f);
    [SerializeField] private Color criticalDamageColor = new(1f, 0.36f, 0.06f, 1f);
    [SerializeField] private Color flashColor = Color.white;
    [Header("Camera Shake")]
    [SerializeField] private bool shakeOnPlayerHit = true;
    [SerializeField] private float playerHitShakeIntensity = 0.22f;
    [SerializeField] private float criticalHitShakeIntensity = 0.42f;

    private readonly List<FloatingCombatText> textPool = new();
    private readonly Dictionary<Transform, Renderer[]> rendererCache = new();
    private MaterialPropertyBlock propertyBlock;
    private int baseColorId;
    private int colorId;

    private void Awake()
    {
        propertyBlock = new MaterialPropertyBlock();
        baseColorId = Shader.PropertyToID("_BaseColor");
        colorId = Shader.PropertyToID("_Color");
        WarmupTextPool();
        SceneManager.activeSceneChanged += OnSceneChanged;
    }

    private void OnDestroy()
    {
        SceneManager.activeSceneChanged -= OnSceneChanged;
    }

    private void OnEnable()
    {
        CombatFeedbackEvents.DamageTaken += HandleDamageTaken;
    }

    private void OnDisable()
    {
        CombatFeedbackEvents.DamageTaken -= HandleDamageTaken;
    }

    private void OnSceneChanged(Scene _, Scene __)
    {
        rendererCache.Clear();
    }

    private void HandleDamageTaken(CombatFeedbackEvent feedbackEvent)
    {
        if (feedbackEvent.DamageAmount <= 0f || feedbackEvent.Target == null)
        {
            return;
        }

        if (showFloatingText)
        {
            SpawnFloatingText(feedbackEvent);
        }

        if (flashTargets)
        {
            StartCoroutine(FlashTarget(feedbackEvent.Target));
        }

        if (shakeOnPlayerHit && feedbackEvent.IsPlayerTarget)
        {
            float intensity = feedbackEvent.IsCritical ? criticalHitShakeIntensity : playerHitShakeIntensity;
            CameraFollow.Shake(intensity);
        }
    }

    private void SpawnFloatingText(CombatFeedbackEvent feedbackEvent)
    {
        FloatingCombatText text = GetTextFromPool();
        int roundedDamage = Mathf.Max(1, Mathf.RoundToInt(feedbackEvent.DamageAmount));
        string label = feedbackEvent.IsCritical ? $"{roundedDamage}!" : roundedDamage.ToString();
        Color color = feedbackEvent.IsCritical
            ? criticalDamageColor
            : (feedbackEvent.IsPlayerTarget ? playerDamageColor : enemyDamageColor);
        float size = feedbackEvent.IsPlayerTarget ? playerTextSize : enemyTextSize;
        Vector3 position = feedbackEvent.WorldPosition + Vector3.up * 1.25f;
        text.Play(position, label, color, size, textLifetime);
    }

    private IEnumerator FlashTarget(Transform target)
    {
        Renderer[] renderers = GetCachedRenderers(target);
        bool[] hadBlock = new bool[renderers.Length];
        Color[] savedBaseColors = new Color[renderers.Length];
        Color[] savedColors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null)
            {
                continue;
            }

            renderers[i].GetPropertyBlock(propertyBlock);
            hadBlock[i] = !propertyBlock.isEmpty;

            if (hadBlock[i])
            {
                savedBaseColors[i] = propertyBlock.GetColor(baseColorId);
                savedColors[i] = propertyBlock.GetColor(colorId);
            }

            propertyBlock.SetColor(baseColorId, flashColor);
            propertyBlock.SetColor(colorId, flashColor);
            renderers[i].SetPropertyBlock(propertyBlock);
        }

        yield return new WaitForSecondsRealtime(flashDuration);

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null)
            {
                continue;
            }

            if (!hadBlock[i])
            {
                renderers[i].SetPropertyBlock(null);
            }
            else
            {
                propertyBlock.Clear();
                propertyBlock.SetColor(baseColorId, savedBaseColors[i]);
                propertyBlock.SetColor(colorId, savedColors[i]);
                renderers[i].SetPropertyBlock(propertyBlock);
            }
        }
    }

    private Renderer[] GetCachedRenderers(Transform target)
    {
        if (!rendererCache.TryGetValue(target, out Renderer[] cached))
        {
            cached = target.GetComponentsInChildren<Renderer>(true);
            rendererCache[target] = cached;
        }

        return cached;
    }

    private void WarmupTextPool()
    {
        for (int i = 0; i < BasePoolSize; i++)
        {
            CreateTextObject();
        }
    }

    private FloatingCombatText GetTextFromPool()
    {
        for (int i = 0; i < textPool.Count; i++)
        {
            if (!textPool[i].IsActive)
            {
                return textPool[i];
            }
        }

        return CreateTextObject();
    }

    private FloatingCombatText CreateTextObject()
    {
        GameObject textObject = new("FloatingCombatText", typeof(TextMeshPro), typeof(FloatingCombatText));
        textObject.transform.SetParent(transform, false);
        textObject.SetActive(false);
        FloatingCombatText floatingText = textObject.GetComponent<FloatingCombatText>();
        textPool.Add(floatingText);
        return floatingText;
    }
}
