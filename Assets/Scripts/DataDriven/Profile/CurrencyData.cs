using UnityEngine;

[CreateAssetMenu(menuName = "Soulstone/Profile/Currency", fileName = "CurrencyData")]
public class CurrencyData : ScriptableObject
{
    [SerializeField] private CurrencyType currencyType;
    [SerializeField] private string displayName;
    [SerializeField] private Sprite icon;

    public CurrencyType CurrencyType => currencyType;
    public string DisplayName => displayName;
    public Sprite Icon => icon;
}
