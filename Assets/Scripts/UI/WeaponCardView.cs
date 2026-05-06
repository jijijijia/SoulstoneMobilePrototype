using UnityEngine.Events;

public class WeaponCardView : MenuCardViewBase
{
    public void Configure(WeaponData weapon, string description, UnityAction onClick)
    {
        string title = weapon != null ? weapon.DisplayName : "Unknown Weapon";
        ConfigureBase(title, description, weapon != null ? weapon.Icon : null, onClick);
    }
}
