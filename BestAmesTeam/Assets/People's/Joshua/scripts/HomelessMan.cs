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

    public bool isBegger = false;
    public bool isTheif = false;

    public Transform Pcamera;


    // Nav and player tracking
    public NavMeshAgent agent;
    public Transform player;
    public Transform exitPoint;

    [Tooltip("Distance at which the homeless man will stop following and attempt interaction")]
    public float interactionDistance = 2f;

    // thief state
    ItemSpot thiefTargetSpot;
    bool thiefHasTarget = false;
    bool thiefInteracting = false;
    bool isLeaving = false;

    // BEGGER STATE
    public Canvas beggerUICanvas;
    public bool beggerIsPaid = false;
    public bool beggerIsShunned = false;



    void Start()
    {
        getType();

        Pcamera = GetComponent<PlayerMovement>().playerCamera;
        beggerUICanvas.enabled = false;
        

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


        if (isTheif)
            gameObject.GetComponent<NPCController>().ChooseItem();
    }

    public void getType()
    {
        // Choose a role at random on startup: 0 = Theif, 1 = begger, 
        int choice = Random.Range(0, 2);
        //Debug.Log(choice);
        isBegger = false;
        isTheif = false;

        for (int i = 0; i < 10; i++)
        {
            int choice1 = Random.Range(0, 2);
            print("This Guy:" + choice1);
        }

        switch (choice)
        {
            case 0:
                isTheif = true;
                Debug.Log("This NPC is a thief.");
                break;
            case 1:
                isBegger = true;
                Debug.Log("This NPC is a begger.");
                break;

        }
    }

    void Update()
    {
        if (isBegger)
            HandleBeggerBehavior();

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
        float dist = Vector3.Distance(transform.position, player.position);

        if (agent == null || player == null) return;

        if (!beggerIsPaid && !beggerIsShunned)
        {


            agent.SetDestination(player.position);


            if (dist <= interactionDistance && !isInteracting)
            {
                isInteracting = true;
                beggerUICanvas.enabled = true;
                agent.isStopped = true;
                PlayerMovement.Instance.cursorLock = false;
                beggerUICanvas.enabled = true;
                Pcamera.LookAt(this.transform.position);



            }
            else if (isInteracting && dist > interactionDistance + 0.5f)
            {
                isInteracting = false;
                beggerUICanvas.enabled = false;
                agent.isStopped = false;
                PlayerMovement.Instance.cursorLock = true;
                beggerUICanvas.enabled = false;


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
                    if (distanceToExit <= 3f)
                        Destroy(gameObject);
                }
            }
            else
            {
                agent.isStopped = true;
            }
        }
    }
    public void HandleBeggerPayment()
    {
        int cost = Random.Range(25, 50);
        PlayerMovement.Instance.cursorLock = true;
        Currency.Instance.amount -= cost;
        beggerIsPaid = true;
        isInteracting = false;
        beggerUICanvas.enabled = false;
        if (agent != null)
            agent.isStopped = false;
        if (beggerIsPaid)
        {
            agent.SetDestination(exitPoint.position);
        }
    }

    public void HandleBeggerShunning()
    {
        beggerIsShunned = true;
        isInteracting = false;
        PlayerMovement.Instance.cursorLock = true;
        beggerUICanvas.enabled = false;
        if (agent != null)
            agent.isStopped = false;

        int choice = Random.Range(0, 4);

        switch (choice)
        {
            case 1:
                agent.SetDestination(exitPoint.position);
                break;
            case 2:
                agent.SetDestination(exitPoint.position);
                break;
            case 3:
                isTheif = true;
                isBegger = false;
                break;
            case 4:
                agent.SetDestination(exitPoint.position);
                break;
        }

    }

    public void ReceivePayment()
    {
        beggerIsPaid = true;
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

    }
}
