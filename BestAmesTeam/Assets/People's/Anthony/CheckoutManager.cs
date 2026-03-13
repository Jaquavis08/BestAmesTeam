using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckoutManager : MonoBehaviour
{
    public static CheckoutManager Instance;
    public Transform checkoutSpot;
    public Transform exitPoint;

    public Queue<NPCController> checkoutQueue = new Queue<NPCController>();

    void Awake()
    {
        Instance = this;
    }

    public void JoinQueue(NPCController npc)
    {
        checkoutQueue.Enqueue(npc);

        int positionInLine = checkoutQueue.Count - 1;
        Vector3 lineOffset = new Vector3(0, 0, -1f * positionInLine);
        npc.agent.SetDestination(checkoutSpot.position + lineOffset);

        Debug.Log(npc.name + " joined, Current queue count: " + checkoutQueue.Count);

        if (positionInLine == 0)
        {
            StartCoroutine(ProcessQueue());
        }
    }

    private IEnumerator ProcessQueue()
    {
        while (checkoutQueue.Count > 0)
        {
            NPCController currentNPC = checkoutQueue.Peek();

            while (!currentNPC.AtCheckoutSpot())
            {
                yield return null;
            }

            Debug.Log(currentNPC.name + " is checking out...");

            yield return new WaitForSeconds(5f);

            currentNPC.CompleteCheckout();

            checkoutQueue.Dequeue();
            Debug.Log("Complete, Queue remaining: " + checkoutQueue.Count);

            if (checkoutQueue.Count > 0)
            {
                int idx = 0;
                foreach (NPCController npc in checkoutQueue)
                {
                    Vector3 offset = new Vector3(0, 0, -1f * idx);
                    npc.agent.SetDestination(checkoutSpot.position + offset);
                    idx++;
                }
            }
        }
    }
}