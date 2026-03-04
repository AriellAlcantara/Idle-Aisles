using UnityEngine;
using System.Collections;

public class Shopper_behaviour : MonoBehaviour
{
    [Tooltip("Movement speed in units per second")]
    public float speed = 2f;

    [Tooltip("Seconds to wait at pick-up")]
    public float pickupWait = 2f;

    [Tooltip("Seconds to wait at drop-off")]
    public float dropoffWait = 3f;

    // current movement target
    private Transform target;

    // small distance to consider "arrived"
    private const float arriveThreshold = 0.2f;

    void Start()
    {
        // Start by finding the closest Path object
        target = FindClosestByNameSubstring("Path");

        if (target == null)
        {
            Debug.LogWarning("Shopper_behaviour: No Path object found in the scene.");
        }
    }

    void Update()
    {
        if (target == null)
            return;

        // Move towards the current target
        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        // Arrived?
        if (Vector3.Distance(transform.position, target.position) <= arriveThreshold)
        {
            OnReachedTarget();
        }
    }

    private void OnReachedTarget()
    {
        if (target == null)
            return;

        string tname = target.name.ToLowerInvariant();

        if (tname.Contains("path"))
        {
            // Arrived at the path -> choose a random pickup
            Transform pickup = FindRandomByNameSubstring("Pick-up", "Pickup", "Pickup");
            if (pickup != null)
            {
                target = pickup;
            }
            else
            {
                Debug.LogWarning("Shopper_behaviour: No Pick-up object found.");
            }
        }
        else if (tname.Contains("pick")) // covers pick-up / pickup
        {
            // At pickup: wait pickupWait then go to a drop-off
            StartCoroutine(WaitThenGoToNext(dropoffWait: dropoffWait, nextNameSubstrings: new string[] { "Drop-off", "Dropoff", "Drop" }));
        }
        else if (tname.Contains("drop"))
        {
            // At drop-off: wait dropoffWait then go to exit
            StartCoroutine(WaitThenGoToNext(exitWait: dropoffWait, nextNameSubstrings: new string[] { "exit" }));
        }
        else if (tname == "exit" || tname.Contains("exit"))
        {
            // Reached exit: destroy shopper to allow spawner to replace it
            Destroy(gameObject);
        }
        else
        {
            // Unknown target type — try to progress to an exit as a fallback
            Transform exit = FindClosestByNameSubstring("exit");
            if (exit != null)
                target = exit;
        }
    }

    private IEnumerator WaitThenGoToNext(float pickupWait = 0f, float dropoffWait = 0f, float exitWait = 0f, string[] nextNameSubstrings = null)
    {
        // Decide which wait to use (only one will be > 0 when called appropriately)
        float wait = pickupWait;
        if (wait <= 0f) wait = dropoffWait;
        if (wait <= 0f) wait = exitWait;

        if (wait > 0f)
            yield return new WaitForSeconds(wait);

        // Find next target by provided substrings
        if (nextNameSubstrings != null && nextNameSubstrings.Length > 0)
        {
            // If next is exit, prefer closest; otherwise pick random matching
            bool isExit = false;
            foreach (var s in nextNameSubstrings)
                if (s.ToLowerInvariant().Contains("exit")) isExit = true;

            Transform next = null;
            if (isExit)
                next = FindClosestByNameSubstring(nextNameSubstrings);
            else
                next = FindRandomByNameSubstring(nextNameSubstrings);

            if (next != null)
            {
                target = next;
            }
            else
            {
                Debug.LogWarning($"Shopper_behaviour: Could not find next target matching: {string.Join(",", nextNameSubstrings)}");
            }
        }
    }

    // Find the closest transform whose name contains any of the provided substrings (case-insensitive)
    private Transform FindClosestByNameSubstring(params string[] nameSubstrings)
    {
        Transform closest = null;
        float closestDist = float.MaxValue;
        var all = GameObject.FindObjectsOfType<Transform>();
        foreach (var tr in all)
        {
            if (tr == null)
                continue;

            string lname = tr.name.ToLowerInvariant();
            bool match = false;
            foreach (var s in nameSubstrings)
            {
                if (lname.Contains(s.ToLowerInvariant()))
                {
                    match = true;
                    break;
                }
            }

            if (!match)
                continue;

            float d = Vector3.Distance(transform.position, tr.position);
            if (d < closestDist)
            {
                closestDist = d;
                closest = tr;
            }
        }

        return closest;
    }

    // Find a random transform whose name contains any of the provided substrings (case-insensitive)
    private Transform FindRandomByNameSubstring(params string[] nameSubstrings)
    {
        var all = GameObject.FindObjectsOfType<Transform>();
        var matches = new System.Collections.Generic.List<Transform>();
        foreach (var tr in all)
        {
            if (tr == null)
                continue;

            string lname = tr.name.ToLowerInvariant();
            foreach (var s in nameSubstrings)
            {
                if (lname.Contains(s.ToLowerInvariant()))
                {
                    matches.Add(tr);
                    break;
                }
            }
        }

        if (matches.Count == 0)
            return null;

        int idx = Random.Range(0, matches.Count);
        return matches[idx];
    }
}
