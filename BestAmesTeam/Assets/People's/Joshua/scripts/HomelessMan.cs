using System.Collections;
using UnityEngine;
using Unity.AI.Navigation;
using UnityEngine.AI;

public class HomelessMan : MonoBehaviour
{
    public NavMeshSurface navMeshSurface;

    public GameObject Shelf;

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

    Vector3 sleeperTarget;
    bool sleeperHasTarget = false;
    bool sleeperAtTarget = false;

    
    ItemSpot thiefTargetSpot;
    bool thiefHasTarget = false;
    bool thiefInteracting = false;
    bool isLeaving = false;

    void Start()
    {
        // Choose a role at random on startup: 0 = sleeper, 1 = begger, 2 = theif
        int choice = Random.Range(0, 3);
        isSleeper = (choice == 0);
        isBegger = (choice == 1);
        isTheif = (choice == 2);

      
        isInteracting = false;

  
        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }

        // Try to find player by tag if not assigned
        if (player == null)
        {
            var playerGo = GameObject.FindWithTag("Player");
            if (playerGo != null)
                player = playerGo.transform;
        }

        if (navMeshSurface != null)
        {
            navMeshSurface.BuildNavMesh();
        }

        // If sleeper, immediately choose a point to go to
        if (isSleeper)
        {
            ChooseSleeperDestination();
        }

        // If thief, immediately try to pick a shelf target
        if (isTheif)
        {
            PickThiefShelfTarget();
        }
    }

    void Update()
    {
       
        if (isBegger)
        {
            HandleBeggerBehavior();
        }

       
        if (isTheif)
        {
            HandleThiefBehavior();
        }

        if (isSleeper)
        {
            HandleSleeperBehavior();
        }
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
                    {
                        Destroy(gameObject);
                    }
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
        {
            ChooseSleeperDestination();
        }

       
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
                    
                }
            }
        }
    }

    void PickThiefShelfTarget()
    {
        // Try to get a random shelf and a spot on it (reuse ShelfManager pattern used by NPCController)
        Shelf shelf = null;
        try
        {
            shelf = ShelfManager.Instance.GetRandomShelfWithItems();
        }
        catch
        {
            shelf = null;
        }

        if (shelf == null)
        {
            // No shelf found — just leave
            BeginLeaving();
            return;
        }

        thiefTargetSpot = shelf.GetRandomSpotWithItem();
        if (thiefTargetSpot == null)
        {
            // no spot, leave
            BeginLeaving();
            return;
        }

        thiefHasTarget = true;
        thiefInteracting = false;

       
        Vector3 offset = new Vector3(
            Random.Range(-0.3f, 0.3f),
            0f,
            Random.Range(-0.3f, 0.3f)
        );

        if (agent != null)
        {
            agent.isStopped = false;
            agent.SetDestination(thiefTargetSpot.standPoint.position + offset);
        }
    }

    void HandleThiefBehavior()
    {
        if (agent == null) return;

        if (isLeaving)
        {
           
            if (exitPoint != null)
            {
                agent.isStopped = false;
                agent.SetDestination(exitPoint.position);

                if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
                {
                    float distanceToExit = Vector3.Distance(transform.position, exitPoint.position);
                    if (distanceToExit <= 1f)
                    {
                        Destroy(gameObject);
                    }
                }
            }
            return;
        }

       
        if (!thiefHasTarget)
        {
            PickThiefShelfTarget();
            return;
        }

        
        if (thiefHasTarget && !thiefInteracting)
        {
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                
                StartCoroutine(InteractWithShelfAndLeaveRoutine());
            }
        }
    }

    IEnumerator InteractWithShelfAndLeaveRoutine()
    {
        if (thiefInteracting) yield break;
        thiefInteracting = true;
        isInteracting = true;

        // Attempt to take an item if API exists (ItemSpot.TakeItem)
        object item = null;
        if (thiefTargetSpot != null)
        {
            try
            {
                item = thiefTargetSpot.TakeItem();
            }
            catch
            {
                item = null;
            }
        }

        // If there was no item available, leave immediately
        if (item == null)
        {
            isInteracting = false;
            thiefInteracting = false;
            BeginLeaving();
            yield break;
        }

        // Item was taken - optionally log, then simulate interaction delay and leave
        Debug.Log(name + " took " + (item.GetType().GetProperty("itemName")?.GetValue(item) ?? "an item") + " from shelf " + thiefTargetSpot?.name);

        yield return new WaitForSeconds(Random.Range(0.8f, 1.6f));

        // done interacting: begin leaving
        isInteracting = false;
        BeginLeaving();
    }

    void BeginLeaving()
    {
        isLeaving = true;

        if (exitPoint == null)
        {
            if (CheckoutManager.Instance != null && CheckoutManager.Instance.exitPoint != null)
            {
                exitPoint = CheckoutManager.Instance.exitPoint;
            }
            else
            {
                var exitGo = GameObject.FindWithTag("Exit");
                if (exitGo != null)
                    exitPoint = exitGo.transform;
            }
        }

        if (agent != null && exitPoint != null)
        {
            agent.isStopped = false;
            agent.SetDestination(exitPoint.position);
        }
        else if (agent != null && exitPoint == null)
        {
            
            StartCoroutine(LeaveAfterDelayFallback());
        }
    }

    IEnumerator LeaveAfterDelayFallback()
    {
        yield return new WaitForSeconds(5f);
        Destroy(gameObject);
    }

    void ChooseSleeperDestination()
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
        {
            agent.isStopped = false;
        }

        
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

        Gizmos.color = Color.blue;
       
        Gizmos.color = Color.blue;

       
        Gizmos.color = Color.cyan;
        Vector3 center = shopCenter != null ? shopCenter.position : transform.position;
        Gizmos.DrawWireSphere(center, shopRadius);
    }
}
