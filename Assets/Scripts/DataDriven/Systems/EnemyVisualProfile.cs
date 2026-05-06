using UnityEngine;

public sealed class EnemyVisualProfile
{
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");

    private readonly Transform ownerTransform;
    private readonly Renderer[] renderers;
    private readonly Vector3 initialScale;
    private readonly MaterialPropertyBlock propertyBlock;

    public EnemyVisualProfile(Transform ownerTransform, Renderer[] renderers)
    {
        this.ownerTransform = ownerTransform;
        this.renderers = renderers ?? new Renderer[0];
        initialScale = ownerTransform != null ? ownerTransform.localScale : Vector3.one;
        propertyBlock = new MaterialPropertyBlock();
    }

    public void Apply(EnemyData enemyData)
    {
        if (ownerTransform == null || enemyData == null)
        {
            return;
        }

        ownerTransform.localScale = initialScale * enemyData.VisualScaleMultiplier;

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer targetRenderer = renderers[i];

            if (targetRenderer == null)
            {
                continue;
            }

            targetRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(BaseColorId, enemyData.TintColor);
            propertyBlock.SetColor(ColorId, enemyData.TintColor);
            targetRenderer.SetPropertyBlock(propertyBlock);
        }
    }

    public void Reset()
    {
        if (ownerTransform != null)
        {
            ownerTransform.localScale = initialScale;
        }

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                renderers[i].SetPropertyBlock(null);
            }
        }
    }
}
