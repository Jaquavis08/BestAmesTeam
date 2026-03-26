using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class HomelessMan : MonoBehaviour
{
    public NavMeshSurface navMeshSurface;

    public GameObject Shelf;

    public List<CartItem> cart = new List<CartItem>();

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
    
        if (isTheif && gameObject.GetComponent<NPCController>().itemsCollected < 0 && Input.GetKeyDown(KeyCode.G))
        {
            ThiefCaught();
        }
        
    }

    void ThiefCaught()
    {
       
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
