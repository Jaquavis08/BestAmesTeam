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

    void Start()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

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
        // NORMAL SHOPPING
        if (targetSpot != null && !isBrowsing)
        {
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                isBrowsing = true;
                StartCoroutine(GrabItemRoutine());
            }
        }

        // 🔥 QUEUE BEHAVIOR FIX
        if (CheckoutManager.Instance != null &&
            CheckoutManager.Instance.checkoutQueue.Contains(this))
        {
            // TURN OFF pushing
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;

            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                agent.velocity = Vector3.zero;
                agent.isStopped = true;
            }
        }
        else
        {
            // TURN IT BACK ON
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
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

    public void ChooseItem()
    {
        Shelf shelf = ShelfManager.Instance.GetRandomShelfWithItems();

        if (shelf == null)
        {
            if (itemsCollected == 0)
            {
                isLeaving = true;
                agent.isStopped = false;
                agent.SetDestination(CheckoutManager.Instance.exitPoint.position);
            }
            else
            {
                GoToCheckout();
            }
            return;
        }

        targetSpot = shelf.GetRandomSpotWithItem();

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

        if (!crowded)
        {
            agent.isStopped = false; // 🔥 IMPORTANT
            agent.SetDestination(targetSpot.standPoint.position + offset);
        }
        else
        {
            StartCoroutine(WaitThenChooseAnotherSpot());
        }
    }

    IEnumerator WaitThenChooseAnotherSpot()
    {
        yield return new WaitForSeconds(Random.Range(0.2f, 0.5f));
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
        agent.isStopped = false; // 🔥 IMPORTANT
        CheckoutManager.Instance.JoinQueue(this);
    }

    public IEnumerator GrabItemRoutine()
    {
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
        if (CheckoutManager.Instance.checkoutSpot == null)
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
            agent.SetDestination(CheckoutManager.Instance.exitPoint.position);
        }
    }

    void CheckIfExited()
    {
        if (!isLeaving) return;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            Destroy(gameObject);
        }
    }

    void PrintCart(bool ispaying, List<CartItem> cartData)
    {
        if (!ispaying) return;

        StartCoroutine(CheckoutRoutine(cartData));
    }

    IEnumerator CheckoutRoutine(List<CartItem> cartData)
    {
        print($"Cart COPY has {cartData.Count} items.");

        if (CheckoutManager.Instance.checkoutUIParent != null)
        {
            foreach (Transform child in CheckoutManager.Instance.checkoutUIParent)
            {
                Destroy(child.gameObject);
            }
        }

        yield return new WaitForSeconds(0.1f);

        foreach (var entry in cartData)
        {
            if (entry.item == null) continue;

            float totalPrice = entry.item.price * entry.quantity;

            print($"{entry.quantity} x {entry.item.itemName} - ${totalPrice}");

            if (Currency.Instance != null)
            {
                Currency.Instance.AddCurrency((int)totalPrice);
            }

            if (CheckoutManager.Instance.checkoutItemUIPrefab != null &&
                CheckoutManager.Instance.checkoutUIParent != null)
            {
                GameObject ui = Instantiate(
                    CheckoutManager.Instance.checkoutItemUIPrefab,
                    CheckoutManager.Instance.checkoutUIParent
                );

                CheckoutItemUI uiScript = ui.GetComponent<CheckoutItemUI>();
                if (uiScript != null)
                {
                    uiScript.Setup(entry.item.itemName, entry.quantity, totalPrice);
                }
            }

            yield return new WaitForSeconds(0.1f);
        }
    }
}

    [System.Serializable]
public class CartItem
{
    public ItemData item;
    public int quantity;
}