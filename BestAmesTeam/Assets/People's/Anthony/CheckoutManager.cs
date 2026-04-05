using System.Collections;
using System.Collections.Generic;
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
            Vector3 offset = checkoutSpot.forward * index * 1.8f;

            Vector3 targetPos = checkoutSpot.position + offset;

            npc.queueTargetPosition = targetPos; // 🔥 STORE EXACT POSITION
            npc.inQueue = true;

            npc.agent.isStopped = false;
            npc.agent.SetDestination(targetPos);

            index++;
        }
    }

    IEnumerator ProcessQueue()
    {
        while (checkoutQueue.Count > 0)
        {
            NPCController currentNPC = checkoutQueue.Peek();

            while (!currentNPC.AtCheckoutSpot())
            {
                yield return null;
            }

            yield return new WaitForSeconds(3f);

            currentNPC.CompleteCheckout(true);

            checkoutQueue.Dequeue();

            UpdateQueuePositions();
        }
    }
}