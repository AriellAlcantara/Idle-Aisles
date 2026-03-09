using UnityEngine;

public static class CoinManager
{
    private static int coins = 0;

    public static int Coins => coins;

    public static void AddCoins(int amount)
    {
        if (amount <= 0) return;
        coins += amount;
    }

    public static bool SpendCoins(int amount)
    {
        if (amount <= 0) return false;
        if (coins < amount) return false;
        coins -= amount;
        return true;
    }
}
