using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class NPCController : MonoBehaviour
{
    public NavMeshAgent agent;

    ItemSpot targetSpot;

    public List<CartItem> cart = new List<CartItem>();

    public int maxItems;
    public int itemsCollected = 0;

    [HideInInspector] public NPCSpawner npcSpawner;

    private bool isLeaving = false;

    void Start()
    {
        maxItems = Random.Range(1, 6);
        ChooseItem();
    }

    void Update()
    {
        if (targetSpot != null)
        {
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                GrabItem();
            }
        }

        CheckIfExited();
    }

    void ChooseItem()
    {
        Shelf shelf = ShelfManager.Instance.GetRandomShelfWithItems();

        if (shelf == null)
        {
            Debug.LogError("No shelf found!");
            if (itemsCollected == 0)
            {
                isLeaving = true;
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
            Debug.Log("No items on this shelf.");
            GoToCheckout();
            return;
        }

        Debug.Log("NPC chose spot: " + targetSpot.name);
        agent.SetDestination(targetSpot.transform.position);
    }

    void GrabItem()
    {
        ItemData grabbedItem = targetSpot.TakeItem();

        if (grabbedItem != null)
        {
            AddItemToCart(grabbedItem);
            itemsCollected++;
            Debug.Log("NPC grabbed " + grabbedItem.itemName);
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
    }

    void AddItemToCart(ItemData item)
    {
        CartItem existing = cart.Find(c => c.item == item);
        if (existing != null)
        {
            existing.quantity++;
        }
        else
        {
            cart.Add(new CartItem { item = item, quantity = 1 });
        }
    }

    void GoToCheckout()
    {
        Debug.Log(name + " going to checkout");
        CheckoutManager.Instance.JoinQueue(this);
    }

    public bool AtCheckoutSpot()
    {
        if (CheckoutManager.Instance.checkoutSpot == null) return true;
        return !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;
    }

    public void CompleteCheckout()
    {
        Debug.Log(name + " completed checkout");

        PrintCart();

        cart.Clear();

        if (CheckoutManager.Instance.exitPoint != null)
        {
            isLeaving = true;
            agent.SetDestination(CheckoutManager.Instance.exitPoint.position);
        }
    }

    void CheckIfExited()
    {
        if (!isLeaving) return;
        if (CheckoutManager.Instance.exitPoint == null) return;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            float distanceToExit = Vector3.Distance(transform.position, CheckoutManager.Instance.exitPoint.position);
            if (distanceToExit <= 1f)
            {
                if (npcSpawner != null) npcSpawner.CustomerLeft();

                Destroy(gameObject);
            }
        }
    }

    void PrintCart()
    {
        Debug.Log("NPC CART");

        foreach (var entry in cart)
        {
            Debug.Log(entry.item.itemName + " x" + entry.quantity);
        }
    }
}

[System.Serializable]
public class CartItem
{
    public ItemData item;
    public int quantity;
}