using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TraderSystem : MonoBehaviour
{
    public int[] upgradeCosts = new int[] { 5, 15, 40, 60, 100 };

    // Trader 1 UI (supports both Unity UI Text and TextMeshPro)
    public Text trader1Label; // e.g. "Trader 1: Upgrade"
    public TMP_Text trader1LabelTMP;
    public Button trader1Button;
    public Text trader1CostText;
    public TMP_Text trader1CostTextTMP;

    // Trader 2 UI
    public Text trader2Label;
    public TMP_Text trader2LabelTMP;
    public Button trader2Button;
    public Text trader2CostText;
    public TMP_Text trader2CostTextTMP;

    // Trader 3 UI (quest)
    public Text trader3Label;
    public TMP_Text trader3LabelTMP;
    public Button trader3Button; // optional, not required for auto-award
    public Text trader3ProgressText;
    public TMP_Text trader3ProgressTextTMP;

    // Coin display (supports UI Text or TextMeshPro)
    public Text coinText;
    public TMP_Text coinTextTMP;

    // Internal levels
    private int trader1Level = 0; // 0..5
    private int trader2Level = 0; // 0..5
    private const int maxLevel = 5;

    // Trader3 quest: milestones and progress
    private int[] trader3Milestones = new int[] { 30, 70, 120 };
    private int trader3Index = 0; // which milestone we're currently working toward
    private int trader3Progress = 0; // progress toward current milestone
    private bool trader3Maxed = false;

    // References to systems to apply upgrades
    public ShopperSpawner shopperSpawner;
    public YieldSystem yieldSystem;

    // cache last known coins to avoid unnecessary UpdateUI calls
    private int lastKnownCoins = -1;

    void Start()
    {
        // Wire up buttons
        if (trader1Button != null)
            trader1Button.onClick.AddListener(UpgradeTrader1);
        if (trader2Button != null)
            trader2Button.onClick.AddListener(UpgradeTrader2);
        if (trader3Button != null)
            trader3Button.onClick.AddListener(() => { /* optional: show details */ });

        // Try to find systems if not assigned
        if (shopperSpawner == null)
            shopperSpawner = FindObjectOfType<ShopperSpawner>();
        if (yieldSystem == null)
            yieldSystem = FindObjectOfType<YieldSystem>();

        UpdateUI(true);
    }

    void OnEnable()
    {
        CoinManager.OnCoinsChanged += OnCoinsChanged;
    }

    void OnDisable()
    {
        CoinManager.OnCoinsChanged -= OnCoinsChanged;
    }

    private void OnCoinsChanged(int newTotal)
    {
        // determine how many coins were earned since last update
        int delta = 0;
        if (lastKnownCoins < 0)
        {
            // first time, just set
            delta = 0;
        }
        else
        {
            delta = newTotal - lastKnownCoins;
        }

        if (delta > 0)
        {
            AddToTrader3Progress(delta);
        }

        lastKnownCoins = newTotal;
        UpdateUI(true);
    }

    private void AddToTrader3Progress(int coinsEarned)
    {
        if (trader3Maxed || trader3Index >= trader3Milestones.Length) return;

        int remaining = coinsEarned;
        while (remaining > 0 && trader3Index < trader3Milestones.Length)
        {
            int target = trader3Milestones[trader3Index];
            int needed = target - trader3Progress;
            int take = Mathf.Min(remaining, needed);
            trader3Progress += take;
            remaining -= take;

            if (trader3Progress >= target)
            {
                // Completed this milestone: increase global coins-per-shopper bonus
                CounterBehaviour.GlobalCoinsBonus += 1;

                // Also apply immediately to existing counters so effect is instant
                var counters = FindObjectsOfType<CounterBehaviour>();
                int applied = 0;
                foreach (var c in counters)
                {
                    if (c != null)
                    {
                        c.coinsPerShopper += 1;
                        applied++;
                    }
                }

                Debug.Log($"Trader3 quest: completed milestone {target}. GlobalCoinsBonus is now {CounterBehaviour.GlobalCoinsBonus}. Applied to {applied} existing counters.");

                // move to next milestone and reset progress to 0 (explicitly as requested)
                trader3Index++;
                trader3Progress = 0;

                if (trader3Index >= trader3Milestones.Length)
                {
                    trader3Maxed = true;
                    break;
                }
            }
        }
    }

    private void UpdateUI(bool force = false)
    {
        if (!force && CoinManager.Coins == lastKnownCoins) return;
        lastKnownCoins = CoinManager.Coins;

        // Trader 1
        string t1Label = $"Trader 1: Upgrade {trader1Level}/{maxLevel} (Max Shoppers Increase)";
        SetText(trader1Label, trader1LabelTMP, t1Label);

        if (trader1Level < maxLevel)
            SetText(trader1CostText, trader1CostTextTMP, $"Cost: {upgradeCosts[trader1Level]}");
        else
            SetText(trader1CostText, trader1CostTextTMP, "Max");

        if (trader1Button != null)
            trader1Button.interactable = trader1Level < maxLevel && CoinManager.Coins >= GetTrader1NextCost();

        // Trader 2
        string t2Label = $"Trader 2: Upgrade {trader2Level}/{maxLevel} (Yield/Resupply Increase)";
        SetText(trader2Label, trader2LabelTMP, t2Label);

        if (trader2Level < maxLevel)
            SetText(trader2CostText, trader2CostTextTMP, $"Cost: {upgradeCosts[trader2Level]}");
        else
            SetText(trader2CostText, trader2CostTextTMP, "Max");

        if (trader2Button != null)
            trader2Button.interactable = trader2Level < maxLevel && CoinManager.Coins >= GetTrader2NextCost();

        // Trader 3 (quest) - show coins earned progress. This tracks coins earned (not spent) and resets progress after each milestone.
        if (!trader3Maxed && trader3Index < trader3Milestones.Length)
        {
            int target = trader3Milestones[trader3Index];
            // Label indicates this is coins-earned progress so player knows they don't need to keep the coins
            string t3Label = $"Trader 3: Coins Earned {trader3Progress}/{target} (Coins earned increase)";
            if (CounterBehaviour.GlobalCoinsBonus > 0)
                t3Label += " (Coins earned increased)";

            SetText(trader3Label, trader3LabelTMP, t3Label);
            SetText(trader3ProgressText, trader3ProgressTextTMP, $"Progress: {trader3Progress}/{target}");
            if (trader3Button != null)
                trader3Button.interactable = true; // submit button can be used for details
        }
        else
        {
            SetText(trader3Label, trader3LabelTMP, "Trader 3: Completed (Coins earned increased)");
            SetText(trader3ProgressText, trader3ProgressTextTMP, "Max");
            if (trader3Button != null)
                trader3Button.interactable = false;
        }

        // Coin display
        SetText(coinText, coinTextTMP, $"Coins: {CoinManager.Coins}");
    }

    // Helper: write value to either TMP or UI Text
    private void SetText(Text uiText, TMP_Text tmpText, string value)
    {
        if (tmpText != null)
        {
            tmpText.text = value;
            return;
        }
        if (uiText != null)
        {
            uiText.text = value;
            return;
        }

        // Best-effort: try to find child TMP/Text
        var foundTmp = GetComponentInChildren<TMP_Text>(true);
        if (foundTmp != null)
        {
            foundTmp.text = value;
            return;
        }
        var foundUi = GetComponentInChildren<Text>(true);
        if (foundUi != null)
        {
            foundUi.text = value;
        }
    }

    private int GetTrader1NextCost()
    {
        if (trader1Level >= maxLevel) return int.MaxValue;
        return upgradeCosts[trader1Level];
    }

    private int GetTrader2NextCost()
    {
        if (trader2Level >= maxLevel) return int.MaxValue;
        return upgradeCosts[trader2Level];
    }

    public void UpgradeTrader1()
    {
        if (trader1Level >= maxLevel) return;
        int cost = GetTrader1NextCost();
        if (!CoinManager.SpendCoins(cost)) return;

        trader1Level++;

        // Effect: increase max shoppers by 2 per upgrade
        if (shopperSpawner != null)
        {
            shopperSpawner.maxSpawns += 2;
        }

        UpdateUI(true);
    }

    public void UpgradeTrader2()
    {
        if (trader2Level >= maxLevel) return;
        int cost = GetTrader2NextCost();
        if (!CoinManager.SpendCoins(cost)) return;

        trader2Level++;

        // Effect:
        // - For first 3 upgrades (levels 1..3) reduce replenish interval by 0.5s each
        // - For last 2 upgrades (levels 4..5) increase replenish amount by 0.5 each
        if (yieldSystem != null)
        {
            if (trader2Level <= 3)
            {
                yieldSystem.replenishInterval = Mathf.Max(0.1f, yieldSystem.replenishInterval - 0.5f);
            }
            else
            {
                // fractional increase
                yieldSystem.replenishAmount += 0.5f;
            }
        }

        UpdateUI(true);
    }

    void Update()
    {
        // Fallback: Update UI if coins changed but event might not be hooked (keeps UI responsive)
        UpdateUI();
    }
}
