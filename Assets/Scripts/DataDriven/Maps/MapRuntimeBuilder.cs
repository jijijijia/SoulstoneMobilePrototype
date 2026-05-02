using System;
using UnityEngine;

public class MapRuntimeBuilder : MonoBehaviour
{
    [Serializable]
    public struct ObstacleDefinition
    {
        public string name;
        public Vector3 position;
        public Vector3 scale;
        public Color color;
    }

    [SerializeField] private Vector2 mapSize = new(40f, 40f);
    [SerializeField] private float groundHeight = 0.2f;
    [SerializeField] private Color groundColor = new(0.45f, 0.43f, 0.4f, 1f);
    [SerializeField] private Color obstacleColor = new(0.58f, 0.56f, 0.52f, 1f);
    [SerializeField] private bool clearLegacySceneMapObjects = true;
    [SerializeField] private ObstacleDefinition[] obstacles;

    private void Awake()
    {
        if (clearLegacySceneMapObjects)
        {
            ClearLegacySceneMapObjects();
        }

        BuildGround();
        BuildObstacles();
    }

    private void BuildGround()
    {
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ground.name = "Ground";
        ground.transform.SetParent(transform, false);
        ground.transform.localPosition = new Vector3(0f, -groundHeight * 0.5f, 0f);
        ground.transform.localScale = new Vector3(Mathf.Max(1f, mapSize.x), groundHeight, Mathf.Max(1f, mapSize.y));
        ApplyColor(ground, groundColor);
    }

    private void BuildObstacles()
    {
        if (obstacles == null)
        {
            return;
        }

        for (int i = 0; i < obstacles.Length; i++)
        {
            ObstacleDefinition obstacle = obstacles[i];
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = string.IsNullOrWhiteSpace(obstacle.name) ? $"Obstacle_{i + 1}" : obstacle.name;
            cube.transform.SetParent(transform, false);
            cube.transform.localPosition = obstacle.position;
            cube.transform.localScale = new Vector3(
                Mathf.Max(0.1f, obstacle.scale.x),
                Mathf.Max(0.1f, obstacle.scale.y),
                Mathf.Max(0.1f, obstacle.scale.z));

            Color color = obstacle.color.a > 0f ? obstacle.color : obstacleColor;
            ApplyColor(cube, color);
        }
    }

    private static void ApplyColor(GameObject target, Color color)
    {
        if (target.TryGetComponent(out Renderer rendererComponent))
        {
            rendererComponent.material.color = color;
        }
    }

    private void ClearLegacySceneMapObjects()
    {
        GameObject[] roots = gameObject.scene.GetRootGameObjects();

        for (int i = 0; i < roots.Length; i++)
        {
            GameObject root = roots[i];

            if (root == null || root == gameObject || root.transform.IsChildOf(transform))
            {
                continue;
            }

            if (root.name == "Ground" || root.name.StartsWith("Cube", StringComparison.OrdinalIgnoreCase))
            {
                Destroy(root);
            }
        }
    }
}
