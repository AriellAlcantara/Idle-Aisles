using UnityEngine;

public class AislesStorage : MonoBehaviour
{
    [Tooltip("Current number of products available at this pickup")]
    public int productCount = 5;

    [Tooltip("Maximum capacity of products this pickup can hold")]
    public int maxProductCount = 5;

    // Optional components to toggle when out of stock
    private Collider pickupCollider;
    private Renderer[] renderers;

    // Whether there are any products left
    public bool HasProducts => productCount > 0;

    void Awake()
    {
        pickupCollider = GetComponent<Collider>();
        renderers = GetComponentsInChildren<Renderer>(includeInactive: true);

        // Clamp values
        if (maxProductCount < 0) maxProductCount = 0;
        productCount = Mathf.Clamp(productCount, 0, maxProductCount);

        if (!HasProducts)
            SetUnavailable();
        else
            SetAvailable();
    }

    // Attempt to take a product. Returns true if successful, false if none left.
    public bool TryTakeProduct()
    {
        if (productCount <= 0)
            return false;

        productCount--;

        if (productCount <= 0)
        {
            OnOutOfProducts();
        }

        return true;
    }

    // Replenish products by amount (clamped to max). Returns true if any product was added.
    public bool Replenish(int amount)
    {
        if (amount <= 0) return false;

        int before = productCount;
        productCount = Mathf.Clamp(productCount + amount, 0, maxProductCount);
        if (productCount > 0 && before <= 0)
        {
            OnRestocked();
        }

        return productCount > before;
    }

    // Called when storage runs out of products
    private void OnOutOfProducts()
    {
        SetUnavailable();
        Debug.Log($"AislesStorage '{gameObject.name}' is out of products and has been marked unavailable.");
    }

    private void OnRestocked()
    {
        SetAvailable();
        Debug.Log($"AislesStorage '{gameObject.name}' was restocked ({productCount}/{maxProductCount}).");
    }

    private void SetUnavailable()
    {
        if (pickupCollider != null)
            pickupCollider.enabled = false;

        if (renderers != null)
        {
            foreach (var r in renderers)
                if (r != null)
                    r.enabled = false;
        }
    }

    private void SetAvailable()
    {
        if (pickupCollider != null)
            pickupCollider.enabled = true;

        if (renderers != null)
        {
            foreach (var r in renderers)
                if (r != null)
                    r.enabled = true;
        }
    }
}
