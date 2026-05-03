using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCController : MonoBehaviour
{
    public NavMeshAgent agent;

    ItemSpot targetSpot;

    public List<CartItem> cart = new List<CartItem>();

    public int maxItems;
    public int itemsCollected = 0;
    public float queueSnapSpeed = 8f;

    [HideInInspector]
    public NPCSpawner npcSpawner;

    private bool isLeaving = false;
    bool isBrowsing = false;

    public Vector3 queueTargetPosition;
    public bool inQueue = false;

    public Animator animator;

    bool isIdlePlaying = false;

    void Start()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        //// 🔥 FORCE PLACE ON NAVMESH
        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 2f, NavMesh.AllAreas))
        {
            transform.position = hit.position;
        }
        else
        {
            Debug.LogError("NPC not placed on NavMesh! Name: " + gameObject.name);
            return;
        }

        agent.Warp(transform.position); // 🔥 IMPORTANT

        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
        agent.avoidancePriority = Random.Range(30, 90);
        agent.radius = 0.5f; // 🔥 increased space
        agent.autoRepath = true;

        maxItems = Random.Range(1, 6);

        if (!gameObject.GetComponent<HomelessMan>() || gameObject.GetComponent<HomelessMan>().isTheif)
        {
            ChooseItem();
        }
    }

    void Update()
    {
        //animator = GetComponent<Animator>();
        float speed = agent.velocity.magnitude;
        if (animator != null)
            animator.SetFloat("Speed", speed);

        if (!GetComponent<HomelessMan>() && animator.GetFloat("Speed") < 0.1f)
        {
            if (!isIdlePlaying)
            {
                isIdlePlaying = true;

                float rand = Random.value; // gives 0.0 → 1.0

                if (rand < 0.75f)
                {
                    animator.SetTrigger("Idle1"); // 75% chance
                }
                else
                {
                    animator.SetTrigger("Idle2"); // 25% chance
                }
            }
        }
        else
        {
            // Reset when moving again
            isIdlePlaying = false;
        }

        if (!isBrowsing && !inQueue && agent.isStopped)
        {
            agent.isStopped = false;
        }

        // NORMAL SHOPPING
        if (targetSpot != null && !isBrowsing)
        {
            if (!agent.pathPending && targetSpot != null)
            {
                float dist = Vector3.Distance(transform.position, targetSpot.standPoint.position);

                if (dist <= 1.2f && itemsCollected < maxItems) // same range as your grab check
                {
                    isBrowsing = true;
                    StartCoroutine(GrabItemRoutine());
                }
            }
        }

        if (inQueue)
        {
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;

            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                // 🔥 HARD SNAP (THIS IS THE MAGIC)
                transform.position = Vector3.Lerp(
                transform.position,
                queueTargetPosition,
                Time.deltaTime * queueSnapSpeed
                );
                transform.rotation = Quaternion.LookRotation(-CheckoutManager.Instance.checkoutSpot.forward);

                agent.velocity = Vector3.zero;
                agent.isStopped = true;
            }
        }
        else
        {
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
        }

        CheckIfExited();
    }

    bool IsAgentValid()
    {
        return agent != null && agent.isOnNavMesh;
    }

    public void SetNPCPosition(Vector3 Position)
    {
        if (IsAgentValid())
        {
            agent.SetDestination(Position);
        }
    }

    public void ChooseItem()
    {
        if (itemsCollected >= maxItems)
        {
            Debug.LogError("NPC " + gameObject.name + " has already collected max items! Items collected: " + itemsCollected);
            return;
        }
        Debug.LogWarning("NPC " + gameObject.name + " is choosing an item. Items collected: " + itemsCollected);

        bool check = ShelfCheck();
        Shelf shelf = ShelfManager.Instance.GetRandomShelfWithItems();
        //print(check);
        if (check == false)
        {
            print("No shelves with items found!");

            GoToCheckout();
            return;
        }

        targetSpot = shelf.GetRandomSpotWithItem();


        //print($"NPC {gameObject.name} is targeting shelf {shelf.name} at spot {targetSpot.name}");

        if (targetSpot == null)
        {
            GoToCheckout();
            return;
        }

        Vector3 offset = new Vector3(
            Random.Range(-0.3f, 0.3f),
            0f,
            Random.Range(-0.3f, 0.3f)
        );

        Collider[] hits = Physics.OverlapSphere(targetSpot.standPoint.position, 0.6f);
        bool crowded = false;

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("NPC"))
            {
                crowded = true;
                break;
            }
        }

        if (!crowded && !inQueue)
        {
            agent.isStopped = false; // 🔥 IMPORTANT
            SetNPCPosition(targetSpot.standPoint.position + offset);
        }
        else
        {
            StartCoroutine(WaitThenChooseAnotherSpot());
        }
    }

    public bool ShelfCheck()
    {
        Shelf shelf = ShelfManager.Instance.GetRandomShelfWithItems();
        return shelf != null;
    }

    IEnumerator WaitThenChooseAnotherSpot()
    {
        yield return new WaitForSeconds(Random.Range(0.25f, 0.5f));
        ChooseItem();
    }

    public void AddItemToCart(ItemData item)
    {
        CartItem existing = cart.Find(c => c.item == item);

        if (existing != null)
            existing.quantity++;
        else
            cart.Add(new CartItem { item = item, quantity = 1 });
    }

    void GoToCheckout()
    {
        isBrowsing = false;
        targetSpot = null;

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.ResetPath();
        }

        if (!transform.GetComponent<HomelessMan>() && itemsCollected > 0)
        {
            CheckoutManager.Instance.JoinQueue(this);
        }
        else
        {
            isLeaving = true;
            SetNPCPosition(CheckoutManager.Instance.exitPoint.position);
        }
    }

    public IEnumerator GrabItemRoutine()
    {
        if (inQueue) yield break; // safety check

        animator.SetTrigger("Grab");
        yield return new WaitForSeconds(Random.Range(1f, 2f));

        if (targetSpot == null)
        {
            isBrowsing = false;
            yield break;
        }

        ItemData grabbedItem = targetSpot.TakeItem();

        if (grabbedItem != null)
        {
            AddItemToCart(grabbedItem);
            itemsCollected++;
        }

        targetSpot = null;

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.ResetPath();
        }

        if (itemsCollected < maxItems)
        {
            ChooseItem();
        }
        else
        {
            GoToCheckout();
        }

        isBrowsing = false;
    }

    public bool AtCheckoutSpot()
    {
        if (this == null || agent == null)
            return true; // treat as "done" so queue doesn't break

        if (CheckoutManager.Instance == null || CheckoutManager.Instance.checkoutSpot == null)
            return true;

        return !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;
    }

    public void CompleteCheckout(bool ispaying)
    {
        inQueue = false;

        // 🔥 MAKE A COPY
        List<CartItem> cartCopy = new List<CartItem>(cart);

        PrintCart(ispaying, cartCopy);
        cart.Clear();

        if (CheckoutManager.Instance.exitPoint != null)
        {
            isLeaving = true;
            agent.isStopped = false; // 🔥 IMPORTANT
            SetNPCPosition(CheckoutManager.Instance.exitPoint.position);
        }
    }

    void CheckIfExited()
    {
        if (!isLeaving) return;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            print($"NPC {gameObject.name} has exited the store.");
            NPCSpawner.Instance.CustomerLeft();
            Destroy(gameObject);
        }
    }

    void PrintCart(bool ispaying, List<CartItem> cartData)
    {
        if (!ispaying) return;

        if (CheckoutManager.Instance != null)
        {
            CheckoutManager.Instance.StartCoroutine(
                CheckoutManager.Instance.CheckoutRoutine(cartData)
            );
        }
    }

    //IEnumerator CheckoutRoutine(List<CartItem> cartData)
    //{
    //    print($"Cart COPY has {cartData.Count} items.");

    //    if (CheckoutManager.Instance.checkoutUIParent != null)
    //    {
    //        foreach (Transform child in CheckoutManager.Instance.checkoutUIParent)
    //        {
    //            Destroy(child.gameObject);
    //        }
    //    }

    //    yield return new WaitForSeconds(0.1f);

    //    foreach (var entry in cartData)
    //    {
    //        if (entry.item == null) continue;

    //        float totalPrice = entry.item.price * entry.quantity;

    //        print($"{entry.quantity} x {entry.item.itemName} - ${totalPrice}");

    //        if (Currency.Instance != null)
    //        {
    //            Currency.Instance.AddCurrency((int)totalPrice);
    //        }

    //        if (CheckoutManager.Instance.checkoutItemUIPrefab != null &&
    //            CheckoutManager.Instance.checkoutUIParent != null)
    //        {
    //            GameObject ui = Instantiate(
    //                CheckoutManager.Instance.checkoutItemUIPrefab,
    //                CheckoutManager.Instance.checkoutUIParent
    //            );

    //            CheckoutItemUI uiScript = ui.GetComponent<CheckoutItemUI>();
    //            if (uiScript != null)
    //            {
    //                uiScript.Setup(entry.item.itemName, entry.quantity, totalPrice);
    //            }
    //        }

    //        yield return new WaitForSeconds(0.1f);
    //    }
    //}
}

    [System.Serializable]
public class CartItem
{
    public ItemData item;
    public int quantity;
}