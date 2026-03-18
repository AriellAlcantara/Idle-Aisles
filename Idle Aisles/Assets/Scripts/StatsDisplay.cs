using UnityEngine;
using TMPro;

// Simple UI helper to display current game stats: max shoppers, coins per shopper, resupply speed.
public class StatsDisplay : MonoBehaviour
{
    [Header("UI Targets - TextMeshPro (required)")]
    public TMP_Text maxShoppersText;
    public TMP_Text coinsPerShopperText;
    public TMP_Text resupplySpeedText;

    [Header("Additional UI")]
    public TMP_Text coinsTotalText; // new: show current coin total

    [Header("References (auto-find if empty)")]
    public ShopperSpawner shopperSpawner;
    public YieldSystem yieldSystem;

    private void Reset()
    {
        // Attempt to auto-wire on Reset (when component is first added)
        AutoAssignTextFields();
    }

    void Awake()
    {
        // Auto-find runtime references if null
        if (shopperSpawner == null)
            shopperSpawner = FindObjectOfType<ShopperSpawner>();
        if (yieldSystem == null)
            yieldSystem = FindObjectOfType<YieldSystem>();

        AutoAssignTextFields();

        UpdateUI();
    }

    void OnValidate()
    {
        // Attempt to auto-assign in editor when something changes
        AutoAssignTextFields();
    }

    void Update()
    {
        // Keep UI in sync in play mode
        UpdateUI();
    }

    private void AutoAssignTextFields()
    {
        // If already assigned, nothing to do
        if (maxShoppersText != null && coinsPerShopperText != null && resupplySpeedText != null && coinsTotalText != null)
            return;

        // Search TMP_Text components under this GameObject first
        var tmps = GetComponentsInChildren<TMP_Text>(true);
        if (tmps == null || tmps.Length == 0)
            return;

        // Helper function to find by keywords in name or current text
        TMP_Text FindByKeywords(string[] keywords)
        {
            foreach (var t in tmps)
            {
                var nm = t.gameObject.name.ToLowerInvariant();
                var txt = (t.text ?? "").ToLowerInvariant();
                foreach (var k in keywords)
                {
                    if (nm.Contains(k) || txt.Contains(k))
                        return t;
                }
            }
            return null;
        }

        // Try to assign maxShoppersText (keywords: max, shopper)
        if (maxShoppersText == null)
        {
            var candidate = FindByKeywords(new string[] { "max shopper", "shopper max", "max", "shopper" });
            if (candidate == null && tmps.Length >= 1) candidate = tmps[0];
            maxShoppersText = candidate;
        }

        // Try to assign coinsPerShopperText (prefer explicit 'per shopper' phrasing)
        if (coinsPerShopperText == null)
        {
            var candidate = FindByKeywords(new string[] { "per shopper", "coins per", "coin per", "coins per shopper" });
            if (candidate == null)
            {
                // fallback to coins keyword
                candidate = FindByKeywords(new string[] { "coin", "coins" });
            }

            if (candidate == null)
            {
                // pick the next unmatched TMP
                foreach (var t in tmps)
                {
                    if (t == maxShoppersText) continue;
                    candidate = t; break;
                }
            }
            coinsPerShopperText = candidate;
        }

        // Try to assign resupplySpeedText (keywords: resupply, speed, replenish)
        if (resupplySpeedText == null)
        {
            var candidate = FindByKeywords(new string[] { "resupply speed", "resupply", "replenish", "speed" });
            if (candidate == null)
            {
                // pick the next unmatched TMP
                foreach (var t in tmps)
                {
                    if (t == maxShoppersText || t == coinsPerShopperText) continue;
                    candidate = t; break;
                }
            }
            resupplySpeedText = candidate;
        }

        // Try to assign coinsTotalText (keywords: total, balance, wallet, coins total)
        if (coinsTotalText == null)
        {
            var candidate = FindByKeywords(new string[] { "coins total", "coin total", "total coins", "total", "balance", "wallet" });
            if (candidate == null)
            {
                // pick the next unmatched TMP
                foreach (var t in tmps)
                {
                    if (t == maxShoppersText || t == coinsPerShopperText || t == resupplySpeedText) continue;
                    candidate = t; break;
                }
            }
            coinsTotalText = candidate;
        }
    }

    private void UpdateUI()
    {
        int maxSpawns = shopperSpawner != null ? shopperSpawner.maxSpawns : 0;

        // Determine a representative "coins per shopper" value.
        int coinsPerShopper = 0;
        var counters = FindObjectsOfType<CounterBehaviour>();
        if (counters != null && counters.Length > 0)
        {
            int sum = 0;
            for (int i = 0; i < counters.Length; i++)
                sum += counters[i].coinsPerShopper;
            coinsPerShopper = Mathf.RoundToInt((float)sum / counters.Length);
        }
        else
        {
            // Fallback: show base + global bonus
            coinsPerShopper = 1 + CounterBehaviour.GlobalCoinsBonus;
        }

        float replenishInterval = yieldSystem != null ? yieldSystem.replenishInterval : 0f;

        if (maxShoppersText != null)
            maxShoppersText.text = $"Max Shopper: {maxSpawns}";
        if (coinsPerShopperText != null)
            coinsPerShopperText.text = $"Coins per shopper: {coinsPerShopper}";
        if (resupplySpeedText != null)
            resupplySpeedText.text = $"Resupply Speed: {replenishInterval:0.00}s";

        if (coinsTotalText != null)
            coinsTotalText.text = $"Coins: {CoinManager.Coins}";
    }
}
