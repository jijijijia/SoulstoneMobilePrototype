using UnityEngine.Events;

public class MapCardView : MenuCardViewBase
{
    public void Configure(string mapName, string biome, string difficulty, UnityAction onClick)
    {
        ConfigureBase(mapName, $"{biome}\nСложность: {difficulty}", onClick);
    }
}
