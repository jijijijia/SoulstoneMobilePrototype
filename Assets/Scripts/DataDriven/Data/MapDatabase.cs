using UnityEngine;

[CreateAssetMenu(menuName = "Soulstone/Data/Map Database", fileName = "MapDatabase")]
public class MapDatabase : ScriptableObject
{
    [SerializeField] private MapData[] maps;

    public MapData[] Maps => maps;

    public MapData GetFirstMap()
    {
        if (maps == null)
        {
            return null;
        }

        for (int i = 0; i < maps.Length; i++)
        {
            if (maps[i] != null)
            {
                return maps[i];
            }
        }

        return null;
    }

    public MapData FindById(string mapId)
    {
        if (string.IsNullOrWhiteSpace(mapId) || maps == null)
        {
            return null;
        }

        for (int i = 0; i < maps.Length; i++)
        {
            MapData map = maps[i];

            if (map != null && string.Equals(map.MapId, mapId, System.StringComparison.OrdinalIgnoreCase))
            {
                return map;
            }
        }

        return null;
    }
}
