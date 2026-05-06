using UnityEngine;

public sealed class MapBoundsResolver
{
    private Vector2 fallbackMin = new(-20f, -20f);
    private Vector2 fallbackMax = new(20f, 20f);
    private Bounds groundBounds;
    private bool hasGroundBounds;
    private bool hasMapBoundsOverride;
    private Vector2 mapBoundsOverrideMin;
    private Vector2 mapBoundsOverrideMax;

    public Vector2 Min => GetMin();
    public Vector2 Max => GetMax();

    public void ConfigureFallback(Vector2 min, Vector2 max)
    {
        fallbackMin = min;
        fallbackMax = max;
    }

    public void ApplyMapData(MapData mapData)
    {
        if (mapData == null)
        {
            hasMapBoundsOverride = false;
            return;
        }

        hasMapBoundsOverride = mapData.OverrideMapBounds;
        mapBoundsOverrideMin = mapData.MapMin;
        mapBoundsOverrideMax = mapData.MapMax;
    }

    public void CacheGroundBounds(Transform groundTransform)
    {
        if (groundTransform == null)
        {
            hasGroundBounds = false;
            return;
        }

        if (groundTransform.TryGetComponent(out Collider groundCollider))
        {
            groundBounds = groundCollider.bounds;
            hasGroundBounds = true;
            return;
        }

        if (groundTransform.TryGetComponent(out Renderer groundRenderer))
        {
            groundBounds = groundRenderer.bounds;
            hasGroundBounds = true;
            return;
        }

        hasGroundBounds = false;
    }

    public Vector3 Clamp(Vector3 position)
    {
        Vector2 min = GetMin();
        Vector2 max = GetMax();
        position.x = Mathf.Clamp(position.x, min.x, max.x);
        position.z = Mathf.Clamp(position.z, min.y, max.y);
        return position;
    }

    public bool Contains(Vector3 position, float padding)
    {
        Vector2 min = GetMin();
        Vector2 max = GetMax();

        return position.x >= min.x + padding &&
               position.x <= max.x - padding &&
               position.z >= min.y + padding &&
               position.z <= max.y - padding;
    }

    private Vector2 GetMin()
    {
        if (hasMapBoundsOverride)
        {
            return mapBoundsOverrideMin;
        }

        return hasGroundBounds
            ? new Vector2(groundBounds.min.x, groundBounds.min.z)
            : fallbackMin;
    }

    private Vector2 GetMax()
    {
        if (hasMapBoundsOverride)
        {
            return mapBoundsOverrideMax;
        }

        return hasGroundBounds
            ? new Vector2(groundBounds.max.x, groundBounds.max.z)
            : fallbackMax;
    }
}
