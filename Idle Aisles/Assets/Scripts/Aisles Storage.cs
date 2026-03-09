using UnityEngine;

public class AislesStorage : MonoBehaviour
{
    [Tooltip("Number of products available at this pickup")]
    public int productCount = 5;

    // Whether there are any products left
    public bool HasProducts => productCount > 0;

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

    // Called when storage runs out of products
    private void OnOutOfProducts()
    {
        // Disable the pickup so shoppers won't target it anymore
        // You can instead disable only the collider or visual elements if preferred.
        gameObject.SetActive(false);
        Debug.Log($"AislesStorage '{gameObject.name}' is out of products and has been disabled.");
    }
}
