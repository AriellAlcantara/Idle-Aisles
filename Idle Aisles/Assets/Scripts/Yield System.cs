using UnityEngine;
using System.Collections;

public class YieldSystem : MonoBehaviour
{
    [Tooltip("Seconds between each replenish tick")]
    public float replenishInterval = 10f;

    [Tooltip("Amount to restore to each aisle per tick (if 0 and fullReplenish true, restocks to max)")]
    public float replenishAmount = 1f; // changed to float to support fractional increases

    [Tooltip("If true, replenish to maxProductCount instead of adding replenishAmount")]
    public bool fullReplenish = false;

    [Tooltip("If true, replenish will run on Start (use false to control externally)")]
    public bool autoStart = true;

    [Tooltip("If true, replenish will fully restock when replenishing (overrides replenishAmount)")]
    public bool fullOnReplenish = false;

    private void Start()
    {
        if (!autoStart) return;

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
            if (fullReplenish || fullOnReplenish)
            {
                int toAdd = Mathf.CeilToInt(s.maxProductCount - s.productCount);
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
