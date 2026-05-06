using System;

public sealed class CurrencyWallet
{
    private readonly PlayerProfileData profile;

    public event Action<CurrencyType, int> CurrencyChanged;

    public CurrencyWallet(PlayerProfileData profile)
    {
        this.profile = profile;
        this.profile?.Normalize();
    }

    public int Get(CurrencyType currencyType)
    {
        return profile != null ? profile.GetCurrency(currencyType) : 0;
    }

    public bool CanAfford(CurrencyType currencyType, int amount)
    {
        return amount <= 0 || Get(currencyType) >= amount;
    }

    public void Add(CurrencyType currencyType, int amount, bool save = true)
    {
        if (profile == null || amount <= 0)
        {
            return;
        }

        profile.AddCurrency(currencyType, amount);
        CurrencyChanged?.Invoke(currencyType, profile.GetCurrency(currencyType));

        if (save)
        {
            SaveSystem.Save();
        }
    }

    public bool Spend(CurrencyType currencyType, int amount, bool save = true)
    {
        if (profile == null || !profile.SpendCurrency(currencyType, amount))
        {
            return false;
        }

        CurrencyChanged?.Invoke(currencyType, profile.GetCurrency(currencyType));

        if (save)
        {
            SaveSystem.Save();
        }

        return true;
    }
}
