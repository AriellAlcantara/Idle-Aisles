using UnityEngine;

public class AislesStorage : MonoBehaviour
{
    [Tooltip("Current number of products available at this pickup")]
    public float productCount = 5f;

    [Tooltip("Maximum capacity of products this pickup can hold")]
    public float maxProductCount = 5f;

    // Optional components to toggle when out of stock
    private Collider pickupCollider;
    private Renderer[] renderers;

    // Whether there are any products left
    public bool HasProducts => productCount > 0f;

    void Awake()
    {
        pickupCollider = GetComponent<Collider>();
        renderers = GetComponentsInChildren<Renderer>(includeInactive: true);

        // Clamp values
        if (maxProductCount < 0f) maxProductCount = 0f;
        productCount = Mathf.Clamp(productCount, 0f, maxProductCount);

        if (!HasProducts)
            SetUnavailable();
        else
            SetAvailable();
    }

    // Attempt to take a product. Returns true if successful, false if none left.
    public bool TryTakeProduct()
    {
        if (productCount <= 0f)
            return false;

        productCount -= 1f;

        if (productCount <= 0f)
        {
            OnOutOfProducts();
        }

        return true;
    }

    // Replenish products by amount (can be fractional). Returns true if any product was added.
    public bool Replenish(float amount)
    {
        if (amount <= 0f) return false;

        float before = productCount;
        productCount = Mathf.Clamp(productCount + amount, 0f, maxProductCount);
        if (productCount > 0f && before <= 0f)
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
