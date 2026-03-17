using UnityEngine;

public class CounterBehaviour : MonoBehaviour
{
    [Tooltip("Coins awarded when a shopper leaves the counter")]
    public int coinsPerShopper = 1;

    // Global bonus applied to newly spawned counters (incremented by ads / trader rewards)
    public static int GlobalCoinsBonus = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Apply any global bonus to this instance so newly spawned counters receive prior rewards
        if (GlobalCoinsBonus != 0)
        {
            coinsPerShopper += GlobalCoinsBonus;
        }

        // Optionally verify this is attached to a Drop-off object
        if (!IsDropOff())
        {
            Debug.LogWarning($"CounterBehaviour on '{gameObject.name}' is not a Drop-off object. This script should be attached to the Drop-off trigger.");
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    // Make sure the counter has a trigger collider. When a shopper leaves the counter area
    // they are considered finished and award coins. This script only awards when attached to a Drop-off object.
    private void OnTriggerExit(Collider other)
    {
        if (!IsDropOff())
            return;

        if (other == null)
            return;

        // Detect shopper by component (flexible if prefab name/tag changes)
        var shopper = other.GetComponent<Shopper_behaviour>();
        if (shopper != null && !shopper.HasPaid)
        {
            CoinManager.AddCoins(coinsPerShopper);
            shopper.MarkAsPaid();
            Debug.Log($"Drop-off: awarded {coinsPerShopper} coins. Total: {CoinManager.Coins}");
        }
    }

    private bool IsDropOff()
    {
        return gameObject.name.ToLowerInvariant().Contains("drop");
    }
}
