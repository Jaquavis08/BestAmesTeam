using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CheckoutManager : MonoBehaviour
{
    public static CheckoutManager Instance;

    public Transform checkoutSpot;
    public Transform exitPoint;

    [Header("Checkout UI")]
    public GameObject checkoutItemUIPrefab;
    public Transform checkoutUIParent;

    public Queue<NPCController> checkoutQueue = new Queue<NPCController>();

    void Awake()
    {
        Instance = this;
    }

    public void JoinQueue(NPCController npc)
    {
        if (checkoutQueue.Contains(npc)) return;

        checkoutQueue.Enqueue(npc);

        UpdateQueuePositions();

        if (checkoutQueue.Count == 1)
        {
            StartCoroutine(ProcessQueue());
        }
    }

    void UpdateQueuePositions()
    {
        int index = 0;

        foreach (NPCController npc in checkoutQueue)
        {
            if (npc.agent != null)
            {
                Vector3 offset = checkoutSpot.forward * index * 1.8f;

                Vector3 targetPos = checkoutSpot.position + offset;

                npc.queueTargetPosition = targetPos; // 🔥 STORE EXACT POSITION
                npc.inQueue = true;

                //Debug.LogWarning($"NPC {npc.name} assigned queue position {index} at {targetPos}");

                npc.inQueue = true;

                npc.agent.isStopped = true;
                npc.agent.ResetPath();
                npc.SetNPCPosition(targetPos);

                index++;
            }
        }
    }

    IEnumerator ProcessQueue()
    {
        while (checkoutQueue.Count > 0)
        {
            NPCController currentNPC = checkoutQueue.Peek();

            if (currentNPC == null)
            {
                checkoutQueue.Dequeue();
                continue;
            }

            while (!currentNPC.AtCheckoutSpot())
            {
                yield return null;
            }

            yield return new WaitForSeconds(3f);

            currentNPC.CompleteCheckout(true);

            checkoutQueue.Dequeue();

            // 🔥 IMPORTANT
            currentNPC.inQueue = false;

            yield return null; // 🔥 let NavMesh settle first
            UpdateQueuePositions();
        }
    }


    public IEnumerator CheckoutRoutine(List<CartItem> cartData)
    {
        if (this == null) yield break;

        if (checkoutUIParent != null)
        {
            foreach (Transform child in checkoutUIParent)
            {
                Destroy(child.gameObject);
            }
        }

        yield return new WaitForSeconds(0.2f);

        foreach (var entry in cartData)
        {
            if (entry == null || entry.item == null) continue;

            float totalPrice = entry.item.Value * entry.quantity;
            float tax = totalPrice * Random.Range(0.07f, 0.1f); // Example tax rate
            print(tax);

            if (Currency.Instance != null)
            {
                Currency.Instance.AddCurrency(totalPrice + tax);
            }

            if (checkoutItemUIPrefab != null && checkoutUIParent != null)
            {
                GameObject ui = Instantiate(
                    checkoutItemUIPrefab,
                    checkoutUIParent
                );

                CheckoutItemUI uiScript = ui.GetComponent<CheckoutItemUI>();
                if (uiScript != null)
                {
                    uiScript.Setup(entry.item.itemName, entry.quantity, totalPrice, tax);
                }
            }

            yield return new WaitForSeconds(0.1f);
        }
    }


}