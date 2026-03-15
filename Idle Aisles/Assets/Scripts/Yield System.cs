using UnityEngine;
using System.Collections;

public class YieldSystem : MonoBehaviour
{
    [Tooltip("Seconds between each replenish tick")]
    public float replenishInterval = 10f;

    [Tooltip("Amount to restore to each aisle per tick (if 0 and fullReplenish true, restocks to max)")]
    public int replenishAmount = 1;

    [Tooltip("If true, replenish to maxProductCount instead of adding replenishAmount")]
    public bool fullReplenish = false;

    private void Start()
    {
        if (replenishInterval <= 0f)
            replenishInterval = 1f;

        StartCoroutine(ReplenishLoop());
    }

    private IEnumerator ReplenishLoop()
    {
        var wait = new WaitForSeconds(replenishInterval);
        while (true)
        {
            ReplenishAllAisles();
            yield return wait;
        }
    }

    private void ReplenishAllAisles()
    {
        var storages = GameObject.FindObjectsOfType<AislesStorage>();
        foreach (var s in storages)
        {
            if (s == null) continue;
            if (fullReplenish)
            {
                int toAdd = s.maxProductCount - s.productCount;
                if (toAdd > 0)
                    s.Replenish(toAdd);
            }
            else
            {
                s.Replenish(replenishAmount);
            }
        }
    }
}
