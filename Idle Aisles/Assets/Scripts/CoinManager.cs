using System;
using UnityEngine;

public static class CoinManager
{
    private static int coins = 0;

    public static int Coins => coins;

    // Event fired when coin total changes. Subscribers receive the new total.
    public static event Action<int> OnCoinsChanged;

    // Extra coins awarded per shopper from bonuses (e.g., trader3 quest)
    public static int ExtraPerShopper { get; set; } = 0;

    public static void AddCoins(int amount)
    {
        if (amount <= 0) return;
        coins += amount;
        OnCoinsChanged?.Invoke(coins);
    }

    public static bool SpendCoins(int amount)
    {
        if (amount <= 0) return false;
        if (coins < amount) return false;
        coins -= amount;
        OnCoinsChanged?.Invoke(coins);
        return true;
    }
}
