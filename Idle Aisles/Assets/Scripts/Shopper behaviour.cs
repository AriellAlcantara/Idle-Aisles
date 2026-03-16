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

    [Tooltip("Degrees per second to rotate toward movement direction")]
    public float rotationSpeed = 720f;

    // Animator
    [Tooltip("Animator component used for shopper animations (optional, will be found on the GameObject or children)")]
    public Animator animator;

    [Tooltip("Bool parameter name to enable walking animation (Set true while moving)")]
    public string walkBoolName = "isWalking";

    [Tooltip("Trigger parameter name to play pick-up / interact animation")]
    public string pickTriggerName = "PickUp";

    [Tooltip("Animator state name for the pick-up interaction. Use exact state name or 'LayerName.StateName' if needed.")]
    public string pickStateName = "PickUp";

    [Tooltip("If > 0, override waiting for animator state by waiting this many seconds for the interaction to complete")]
    public float interactionDuration = 0f;

    // current movement target
    private Transform target;

    // small distance to consider "arrived"
    private const float arriveThreshold = 0.2f;

    // Track whether this shopper has already paid to prevent double payment
    private bool hasPaid = false;

    // Track whether this shopper is currently carrying a product (should only pick once)
    private bool hasPickedUp = false;

    // Public read-only accessors
    public bool HasPaid => hasPaid;
    public bool HasPickedUp => hasPickedUp;

    // Mark shopper as paid
    public void MarkAsPaid()
    {
        hasPaid = true;
    }

    // Prevent handling arrival repeatedly while waiting/processing
    private bool isProcessingTarget = false;

    void Awake()
    {
        if (animator == null)
        {
            // try to find an Animator on this object or its children
            animator = GetComponentInChildren<Animator>();
        }
    }

    void Start()
    {
        // Start by finding the closest Path object
        target = FindClosestByNameSubstring("Path");

        if (target == null)
        {
            Debug.LogWarning("Shopper_behaviour: No Path object found in the scene.");
        }

        UpdateWalkAnimation();
    }

    void Update()
    {
        if (target == null)
            return;

        // Move only when not processing an interaction so shopper waits at pickup/drop-off
        if (!isProcessingTarget)
        {
            transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
        }

        // Rotate to face movement direction (allow rotation even while waiting)
        UpdateRotation();

        // Update walking animation depending on movement; suppress walking while processing
        UpdateWalkAnimation();

        // Arrived? Only trigger arrival handling once until it sets next target.
        if (!isProcessingTarget && Vector3.Distance(transform.position, target.position) <= arriveThreshold)
        {
            isProcessingTarget = true;
            OnReachedTarget();
        }
    }

    private void UpdateRotation()
    {
        if (target == null)
            return;

        Vector3 dir = target.position - transform.position;
        dir.y = 0f; // keep only horizontal direction

        if (dir.sqrMagnitude < 0.0001f)
            return;

        Quaternion desired = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, desired, rotationSpeed * Time.deltaTime);
    }

    private void UpdateWalkAnimation()
    {
        if (animator == null) return;

        // Do not play walk animation while processing interaction/waiting
        if (isProcessingTarget)
        {
            SetWalk(false);
            return;
        }

        if (target == null)
        {
            SetWalk(false);
            return;
        }

        float dist = Vector3.Distance(transform.position, target.position);
        SetWalk(dist > arriveThreshold);
    }

    private void SetWalk(bool value)
    {
        if (animator == null || string.IsNullOrEmpty(walkBoolName)) return;
        animator.SetBool(walkBoolName, value);
    }

    private void TriggerPickAnimation()
    {
        if (animator == null || string.IsNullOrEmpty(pickTriggerName))
        {
            Debug.LogWarning($"Shopper_behaviour: Cannot trigger pick animation - animator or pickTriggerName missing on '{gameObject.name}'");
            return;
        }

        animator.SetTrigger(pickTriggerName);
        Debug.Log($"Shopper_behaviour: Triggered pick animation '{pickTriggerName}' on '{gameObject.name}'");
    }

    private void OnReachedTarget()
    {
        if (target == null)
        {
            isProcessingTarget = false;
            return;
        }

        string tname = target.name.ToLowerInvariant();

        if (tname.Contains("path"))
        {
            // Arrived at the path -> choose a random pickup that still has products
            Transform pickup = FindRandomPickupWithProducts();
            if (pickup != null)
            {
                target = pickup;
                // allow arrival handling again when shopper reaches the new pickup
                isProcessingTarget = false;
                UpdateWalkAnimation();
            }
            else
            {
                // No pickups available: go to exit
                Transform exit = FindClosestByNameSubstring("exit");
                if (exit != null)
                    target = exit;
                else
                    Debug.LogWarning("Shopper_behaviour: No Pick-up available and no exit found.");

                isProcessingTarget = false;
                UpdateWalkAnimation();
            }
        }
        else if (tname.Contains("pick")) // covers pick-up / pickup
        {
            // If shopper already has a product, go to drop-off instead of taking another
            if (hasPickedUp)
            {
                Transform drop = FindRandomByNameSubstring("Drop-off", "Dropoff", "Drop");
                if (drop != null)
                {
                    target = drop;
                }
                else
                {
                    Transform exit = FindClosestByNameSubstring("exit");
                    if (exit != null) target = exit;
                }

                isProcessingTarget = false;
                UpdateWalkAnimation();
                return;
            }

            // At pickup: attempt to take product. If successful, set hasPickedUp, determine next target but DO NOT assign it yet
            var storage = target.GetComponent<AislesStorage>();
            bool took = false;
            if (storage != null)
            {
                took = storage.TryTakeProduct();
            }

            if (took)
            {
                hasPickedUp = true;

                // Decide next target now but do not set it until after the wait
                Transform next = FindRandomByNameSubstring("Drop-off", "Dropoff", "Drop");
                if (next == null)
                    next = FindClosestByNameSubstring("exit");

                // play pick-up animation
                TriggerPickAnimation();

                // wait then assign next target
                StartCoroutine(WaitThenAssignNext(next, pickupWait, true));
            }
            else
            {
                // This pickup is empty (or has no storage) - try another pickup
                Transform nextPick = FindRandomPickupWithProducts(exclude: target);
                if (nextPick != null)
                {
                    target = nextPick;
                    // allow arrival handling again at the new pickup
                    isProcessingTarget = false;
                    UpdateWalkAnimation();
                }
                else
                {
                    // No pickups left - go to exit
                    Transform exit = FindClosestByNameSubstring("exit");
                    if (exit != null)
                        target = exit;

                    isProcessingTarget = false;
                    UpdateWalkAnimation();
                }
            }
        }
        else if (tname.Contains("drop"))
        {
            // At drop-off: determine exit but DO NOT assign it until after the interaction wait
            Transform next = FindClosestByNameSubstring("exit");

            TriggerPickAnimation();
            StartCoroutine(WaitThenAssignNext(next, dropoffWait, true));
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

            isProcessingTarget = false;
            UpdateWalkAnimation();
        }
    }

    // New coroutine: wait using same rules as before, then set the next target
    private IEnumerator WaitThenAssignNext(Transform nextTarget, float waitParam, bool waitForPickAnimation)
    {
        // Wait period selection mirrors previous logic
        if (waitForPickAnimation)
        {
            if (interactionDuration > 0f)
            {
                yield return new WaitForSeconds(interactionDuration);
            }
            else if (animator != null && !string.IsNullOrEmpty(pickStateName))
            {
                yield return new WaitWhile(() =>
                {
                    var st = animator.GetCurrentAnimatorStateInfo(0);
                    return st.IsName(pickStateName) || animator.IsInTransition(0);
                });
            }
            else
            {
                if (waitParam > 0f)
                    yield return new WaitForSeconds(waitParam);
            }
        }
        else
        {
            if (waitParam > 0f)
                yield return new WaitForSeconds(waitParam);
        }

        // assign next and allow processing again
        if (nextTarget != null)
            target = nextTarget;

        isProcessingTarget = false;
        UpdateWalkAnimation();
    }

    // Original WaitThenGoToNext preserved for compatibility (kept but not used by new flow)
    private IEnumerator WaitThenGoToNext(float pickupWait = 0f, float dropoffWait = 0f, float exitWait = 0f, bool waitForPickAnimation = false)
    {
        float wait = pickupWait;
        if (wait <= 0f) wait = dropoffWait;
        if (wait <= 0f) wait = exitWait;

        if (waitForPickAnimation)
        {
            // If an explicit interaction duration was provided, use it
            if (interactionDuration > 0f)
            {
                yield return new WaitForSeconds(interactionDuration);
            }
            else if (animator != null && !string.IsNullOrEmpty(pickStateName))
            {
                yield return new WaitWhile(() =>
                {
                    var st = animator.GetCurrentAnimatorStateInfo(0);
                    return st.IsName(pickStateName) || animator.IsInTransition(0);
                });
            }
            else
            {
                if (wait > 0f)
                    yield return new WaitForSeconds(wait);
            }
        }
        else
        {
            if (wait > 0f)
                yield return new WaitForSeconds(wait);
        }

        isProcessingTarget = false;
        UpdateWalkAnimation();
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

            // skip inactive objects
            if (!tr.gameObject.activeInHierarchy)
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

            // skip inactive objects
            if (!tr.gameObject.activeInHierarchy)
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

    // Find a random pickup GameObject that has AislesStorage and products available.
    // Optionally exclude a transform (for example, the one just tried).
    private Transform FindRandomPickupWithProducts(Transform exclude = null)
    {
        var all = GameObject.FindObjectsOfType<Transform>();
        var matches = new System.Collections.Generic.List<Transform>();
        foreach (var tr in all)
        {
            if (tr == null)
                continue;

            if (!tr.gameObject.activeInHierarchy)
                continue;

            if (exclude != null && tr == exclude)
                continue;

            string lname = tr.name.ToLowerInvariant();
            if (lname.Contains("pick") || lname.Contains("pick-up") || lname.Contains("pickup"))
            {
                var storage = tr.GetComponent<AislesStorage>();
                if (storage != null && storage.HasProducts)
                {
                    matches.Add(tr);
                }
            }
        }

        if (matches.Count == 0)
            return null;

        int idx = Random.Range(0, matches.Count);
        return matches[idx];
    }
}
