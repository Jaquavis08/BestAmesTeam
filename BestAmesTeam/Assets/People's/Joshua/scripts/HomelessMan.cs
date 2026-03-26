using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class HomelessMan : MonoBehaviour
{
    public NavMeshSurface navMeshSurface;

    public GameObject Shelf;

    public bool isBrowsing = false;
    public bool isInteracting = false;

    public bool isSleeper = false;
    public bool isBegger = false;
    public bool isTheif = false;

    // Nav and player tracking
    public NavMeshAgent agent;
    public Transform player;
    public Transform exitPoint;

    [Tooltip("Distance at which the homeless man will stop following and attempt interaction")]
    public float interactionDistance = 2f;

    [Tooltip("Optional transform used as the center of the shop area. If null, this object position is used.")]
    public Transform shopCenter;
    [Tooltip("Radius around the shop center where sleepers can choose a resting point.")]
    public float shopRadius = 8f;

    public bool isPaid = false;

    // sleeper state
    Vector3 sleeperTarget;
    bool sleeperHasTarget = false;
    bool sleeperAtTarget = false;

    // thief state
    ItemSpot thiefTargetSpot;
    bool thiefHasTarget = false;
    bool thiefInteracting = false;
    bool isLeaving = false;

    void Start()
    {
        getType();

        isInteracting = false;

        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        // Try to find player by tag if not assigned
        if (player == null)
        {
            var playerGo = GameObject.FindWithTag("Player");
            if (playerGo != null)
                player = playerGo.transform;
        }



        if (navMeshSurface != null)
            navMeshSurface.BuildNavMesh();

        if (isSleeper)
            ChooseSleeperDestination();

        if (isTheif)
            gameObject.GetComponent<NPCController>().ChooseItem();
    }
    
    public void getType()
    {
        // Choose a role at random on startup: 0 = sleeper, 1 = begger, 2 = theif
        int choice = Random.Range(0, 3);

        isSleeper = false;
        isBegger = false;
        isTheif = true;

        //switch(choice)
        //{
        //    case 1:
        //        isSleeper = true;
        //        break;
        //    case 2:
        //        isBegger = true;
        //        break;
        //    case 3:
        //        isTheif = true;
        //        break;
        //}
    }

    void Update()
    {
        if (isBegger)
            HandleBeggerBehavior();

        if (isSleeper)
            HandleSleeperBehavior();
    
        if (isTheif && Input.GetKeyDown(KeyCode.G))
        {
            Debug.LogWarning("Simulating thief caught condition for testing.");
            ThiefCaught();
        }

        if (isLeaving)
        {
            agent.SetDestination(CheckoutManager.Instance.exitPoint.position);
        }
    }

    void ThiefCaught()
    {
        // Plan (pseudocode):
        // 1. Log start.
        // 2. Get NPCController and its cart; bail out if missing or empty.
        // 3. Build two dictionaries:
        //    a) counts: maps item name -> total quantity
        //    b) representative: maps item name -> representative GameObject prefab (if any)
        //    For each entry in cart:
        //      - If entry is CartItem, use ci.item.name (or "Unknown") for key and ci.quantity (min 1) for qty.
        //        Attempt to discover a prefab/model GameObject on the item via common field/property names (safe reflect).
        //      - Otherwise, try to resolve a human key: GameObject.name, or "name" prop via reflection, or entry.ToString()
        //      - Accumulate counts.
        // 4. If no counts, log and return.
        // 5. Safely obtain ShelfManager box prefab and item dictionary if available.
        // 6. For each distinct item in counts:
        //      - Compute spawn position
        //      - Instantiate boxPrefab if available, otherwise create a small cube fallback
        //      - Get ItemBox component; if present assign itemType (if found) and itemCount; otherwise log warning
        //      - Parent box to a sensible object
        //      - If we have a representative GameObject, instantiate a child copy, strip physics/colliders/agents and scale/position it
        //      - Create a simple TextMesh child showing the count and orient towards main camera
        // 7. After creating all boxes, clear the NPC's cart once (outside the loop).
        // 8. Mark thief as leaving and log completion.
        //
        // Implementation notes:
        // - All external calls are guarded (ShelfManager, ItemDictionary, ItemBox).
        // - Clear the cart once after processing to avoid losing data mid-processing (bug fix).
        // - Defensive null-checking prevents exceptions that could cause "sometimes fails" behavior.

        print("Thief caught! Attempting to return stolen items.");

        var npc = gameObject.GetComponent<NPCController>();
        if (npc == null)
        {
            Debug.LogWarning("NPCController not found on thief.");
            return;
        }

        var cart = npc.cart;
        if (cart == null || cart.Count == 0)
        {
            Debug.Log("Cart is null or empty, nothing to return.");
            return;
        }

        var counts = new Dictionary<string, int>();
        var representative = new Dictionary<string, GameObject>(System.StringComparer.Ordinal);

        foreach (var entry in cart)
        {
            if (entry == null) continue;

            // If the cart stores a CartItem structure
            if (entry is CartItem ci)
            {
                string key = ci.item != null ? (ci.item.name ?? "Unknown") : "Unknown";
                int qty = Mathf.Max(1, ci.quantity);

                if (!counts.ContainsKey(key))
                    counts[key] = 0;

                // Try to find a representative GameObject on the item (safe reflection)
                if (!representative.ContainsKey(key) && ci.item != null)
                {
                    try
                    {
                        var t = ci.item.GetType();
                        var field = t.GetField("prefab") ?? t.GetField("Prefab") ?? t.GetField("worldPrefab") ?? t.GetField("model") ?? t.GetField("Model");
                        GameObject rep = null;
                        if (field != null && typeof(GameObject).IsAssignableFrom(field.FieldType))
                            rep = field.GetValue(ci.item) as GameObject;
                        else
                        {
                            var prop = t.GetProperty("prefab") ?? t.GetProperty("Prefab") ?? t.GetProperty("worldPrefab") ?? t.GetProperty("model") ?? t.GetProperty("Model");
                            if (prop != null && typeof(GameObject).IsAssignableFrom(prop.PropertyType))
                                rep = prop.GetValue(ci.item, null) as GameObject;
                        }

                        if (rep != null)
                            representative[key] = rep;
                    }
                    catch
                    {
                        // swallow reflection exceptions; representative simply won't be set
                    }
                }

                counts[key] += qty;
                continue;
            }

            // Generic fallback for non-CartItem entries
            string fallbackKey = "Unknown";
            try
            {
                // With this corrected version:
                print("NIL");
            }
            catch
            {
                fallbackKey = entry.ToString() ?? "Unknown";
            }

            if (!counts.ContainsKey(fallbackKey))
                counts[fallbackKey] = 0;

            counts[fallbackKey] += 1;
        }

        if (counts.Count == 0)
        {
            Debug.Log("No items in cart to display.");
            return;
        }

        // Safely get box prefab and item dictionary
        GameObject boxPrefab = null;
        var itemArray = (ShelfManager.Instance != null && ShelfManager.Instance.ItemDictionary != null)
            ? ShelfManager.Instance.ItemDictionary.items
            : null;

        try { boxPrefab = ShelfManager.Instance?.BoxPrefab; } catch { boxPrefab = null; }

        Vector3 basePos = transform.position;
        int index = 0;
        float spacing = 1.2f;
        int total = counts.Count;

        foreach (var kv in counts)
        {
            string itemKey = kv.Key;
            int itemCount = kv.Value;
            print($"{itemKey} -> {itemCount}");

            Vector3 offset = transform.forward * 1.0f + transform.right * (index * spacing - (total - 1) * spacing / 2f);
            Vector3 spawnPos = basePos + offset + Vector3.up * 1f;

            GameObject box = null;
            if (boxPrefab != null)
            {
                try
                {
                    box = GameObject.Instantiate(boxPrefab, spawnPos, Quaternion.identity);
                }
                catch
                {
                    box = null;
                }
            }

            if (box == null)
            {
                box = GameObject.CreatePrimitive(PrimitiveType.Cube);
                box.transform.position = spawnPos;
                box.transform.localScale = new Vector3(0.5f, 0.3f, 0.5f);
                var col = box.GetComponent<Collider>();
                if (col != null) GameObject.Destroy(col);
            }

            var itemBox = box.GetComponent<ItemBox>();
            if (itemBox != null)
            {
                if (itemArray != null)
                {
                    try
                    {
                        itemBox.itemType = System.Array.Find(itemArray, i => i != null && i.name == itemKey);
                    }
                    catch
                    {
                        itemBox.itemType = null;
                    }
                }

                itemBox.itemCount = itemCount;
            }
            else
            {
                Debug.LogWarning($"Returned box prefab missing ItemBox component for item '{itemKey}'.");
            }

            box.name = $"ReturnBox_{itemKey}";
            

            if (representative.TryGetValue(itemKey, out GameObject repGo) && repGo != null)
            {
                GameObject itemCopy = null;
                try
                {
                    itemCopy = GameObject.Instantiate(repGo, box.transform);
                }
                catch
                {
                    itemCopy = null;
                }

                if (itemCopy != null)
                {
                    itemCopy.transform.localPosition = Vector3.zero;
                    itemCopy.transform.localRotation = Quaternion.identity;
                    itemCopy.transform.localScale = Vector3.one * 0.25f;

                    foreach (var col in itemCopy.GetComponentsInChildren<Collider>())
                        GameObject.Destroy(col);

                    var agentComp = itemCopy.GetComponent<NavMeshAgent>();
                    if (agentComp != null) GameObject.Destroy(agentComp);

                    foreach (var rb in itemCopy.GetComponentsInChildren<Rigidbody>())
                        GameObject.Destroy(rb);
                }
            }

            GameObject textObj = new GameObject("CountText");
            textObj.transform.SetParent(box.transform, false);
            textObj.transform.localPosition = new Vector3(0f, 0.4f, 0f);
            textObj.transform.localRotation = Quaternion.identity;

            var textMesh = textObj.AddComponent<TextMesh>();
            textMesh.text = itemCount.ToString();
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.characterSize = 0.12f;
            textMesh.fontSize = 64;
            textMesh.color = Color.black;

            var cam = Camera.main;
            if (cam != null)
                textObj.transform.rotation = Quaternion.LookRotation(textObj.transform.position - cam.transform.position);

            index++;
        }

        // Clear the cart once after processing to avoid losing data mid-processing (fixes intermittent missing returns)
        try
        {
            npc.cart.Clear();
        }
        catch
        {
            // ignore failures clearing cart
        }

        print("Running thief caught logic: created return boxes for items.");
        isLeaving = true;
    }

    void HandleBeggerBehavior()
    {
        if (agent == null || player == null) return;

        if (!isPaid)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);

            float dist = Vector3.Distance(transform.position, player.position);
            if (dist <= interactionDistance && !isInteracting)
            {
                isInteracting = true;
                // TODO: beg dialogue/animation
            }
        }
        else
        {
            if (exitPoint != null)
            {
                agent.isStopped = false;
                agent.SetDestination(exitPoint.position);

                if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
                {
                    float distanceToExit = Vector3.Distance(transform.position, exitPoint.position);
                    if (distanceToExit <= 1f)
                        Destroy(gameObject);
                }
            }
            else
            {
                agent.isStopped = true;
            }
        }
    }

    void HandleSleeperBehavior()
    {
        if (agent == null) return;

        if (player == null)
        {
            var playerGo = GameObject.FindWithTag("Player");
            if (playerGo != null)
                player = playerGo.transform;
        }

        if (!sleeperHasTarget)
            ChooseSleeperDestination();

        if (sleeperHasTarget && !sleeperAtTarget)
        {
            agent.isStopped = false;
            agent.SetDestination(sleeperTarget);

            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                sleeperAtTarget = true;
                agent.isStopped = true;
            }
        }
        else if (sleeperAtTarget)
        {
            if (player != null && !isInteracting)
            {
                float dist = Vector3.Distance(transform.position, player.position);
                if (dist <= interactionDistance)
                {
                    isInteracting = true;
                    // TODO: trigger sleeper-specific interaction/animation
                }
            }
        }
    }

    void PickThiefShelfTarget()
    {
        // safe access to ShelfManager
        if (ShelfManager.Instance == null)
        {
            //BeginLeaving();
            return;
        }

        Shelf shelf = ShelfManager.Instance.GetRandomShelfWithItems();

        if (shelf == null)
        {
            //BeginLeaving();
            return;
        }

        ItemSpot spot = shelf.GetRandomSpotWithItem();
        if (spot == null)
        {
            // no items on this shelf -> leave
            //BeginLeaving();
            return;
        }

        thiefTargetSpot = spot;
        thiefHasTarget = true;

        Vector3 offset = new Vector3(
            Random.Range(-0.3f, 0.3f),
            0f,
            Random.Range(-0.3f, 0.3f)
        );

        Collider[] hits = Physics.OverlapSphere(spot.standPoint.position, 0.6f);
        bool crowded = false;
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("NPC"))
            {
                crowded = true;
                break;
            }
        }

        if (!crowded && agent != null)
        {
            agent.SetDestination(spot.standPoint.position + offset);
        }
    }

    public void ChooseSleeperDestination()
    {
        Vector3 center = shopCenter != null ? shopCenter.position : transform.position;
        const int maxAttempts = 8;
        for (int i = 0; i < maxAttempts; i++)
        {
            Vector3 randomLocal = Random.insideUnitSphere * shopRadius;
            randomLocal.y = 0f;
            Vector3 candidate = center + randomLocal;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(candidate, out hit, 2f, NavMesh.AllAreas))
            {
                sleeperTarget = hit.position;
                sleeperHasTarget = true;
                sleeperAtTarget = false;
                if (agent != null)
                {
                    agent.isStopped = false;
                    agent.SetDestination(sleeperTarget);
                }
                return;
            }
        }

        // Fallback
        sleeperTarget = center;
        sleeperHasTarget = true;
        sleeperAtTarget = false;
        if (agent != null)
        {
            agent.isStopped = false;
            agent.SetDestination(sleeperTarget);
        }
    }

    public void ReceivePayment()
    {
        isPaid = true;
        isInteracting = false;

        if (agent != null)
            agent.isStopped = false;

        if (exitPoint == null)
        {
            var exitGo = GameObject.FindWithTag("Exit");
            if (exitGo != null)
                exitPoint = exitGo.transform;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);

        Gizmos.color = Color.cyan;
        Vector3 center = shopCenter != null ? shopCenter.position : transform.position;
        Gizmos.DrawWireSphere(center, shopRadius);
    }
}
