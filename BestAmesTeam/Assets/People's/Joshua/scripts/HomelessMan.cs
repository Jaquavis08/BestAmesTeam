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
    
        if (isTheif && gameObject.GetComponent<NPCController>().itemsCollected < 0 && Input.GetKeyDown(KeyCode.G))
        {
            ThiefCaught();
        }
    }

    void ThiefCaught()
    {
        print("Thief caught! Attempting to return stolen items.");
        if (gameObject.GetComponent<NPCController>().cart == null || gameObject.GetComponent<NPCController>().cart.Count == 0) return;
        print("Running thief caught logic");

        // Try to find a reasonable return transform on the target spot (prefer 'standPoint' if present)
        Transform returnPoint = null;
        if (thiefTargetSpot != null)
        {
            var spotType = thiefTargetSpot.GetType();
            var fp = spotType.GetField("standPoint", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (fp != null)
                returnPoint = fp.GetValue(thiefTargetSpot) as Transform;
            else
            {
                var pp = spotType.GetProperty("standPoint", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (pp != null)
                    returnPoint = pp.GetValue(thiefTargetSpot) as Transform;
            }

            if (returnPoint == null && thiefTargetSpot is Component comp)
                returnPoint = comp.transform;
        }

        // Work on a copy so we can remove safely from the original list
        var copy = gameObject.GetComponent<NPCController>().cart.ToArray();
        foreach (var cartItem in copy)
        {
            if (cartItem == null)
            {
                gameObject.GetComponent<NPCController>().cart.Remove(cartItem);
                continue;
            }

            // Try to extract a GameObject from the CartItem via common field/property names or by searching for GameObject/Component typed members
            GameObject itemGo = null;
            var ciType = cartItem.GetType();

            // Common names first
            string[] candidateNames = { "gameObject", "itemObject", "item", "obj", "gameObj" };
            foreach (var name in candidateNames)
            {
                var f = ciType.GetField(name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (f != null && typeof(UnityEngine.Object).IsAssignableFrom(f.FieldType))
                {
                    var val = f.GetValue(cartItem);
                    if (val is GameObject g) { itemGo = g; break; }
                    if (val is Component c) { itemGo = c.gameObject; break; }
                }

                var p = ciType.GetProperty(name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (p != null && typeof(UnityEngine.Object).IsAssignableFrom(p.PropertyType))
                {
                    var val = p.GetValue(cartItem);
                    if (val is GameObject g2) { itemGo = g2; break; }
                    if (val is Component c2) { itemGo = c2.gameObject; break; }
                }
            }

            // If not found, search any field/property of type GameObject or Component
            if (itemGo == null)
            {
                foreach (var f in ciType.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance))
                {
                    if (typeof(GameObject).IsAssignableFrom(f.FieldType))
                    {
                        itemGo = f.GetValue(cartItem) as GameObject;
                        if (itemGo != null) break;
                    }
                    if (typeof(Component).IsAssignableFrom(f.FieldType))
                    {
                        var comp = f.GetValue(cartItem) as Component;
                        if (comp != null) { itemGo = comp.gameObject; break; }
                    }
                }
            }
            if (itemGo == null)
            {
                foreach (var p in ciType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance))
                {
                    if (typeof(GameObject).IsAssignableFrom(p.PropertyType))
                    {
                        itemGo = p.GetValue(cartItem) as GameObject;
                        if (itemGo != null) break;
                    }
                    if (typeof(Component).IsAssignableFrom(p.PropertyType))
                    {
                        var comp = p.GetValue(cartItem) as Component;
                        if (comp != null) { itemGo = comp.gameObject; break; }
                    }
                }
            }

            // If the target spot exposes a method to accept returned items, try to call it
            bool returnedViaMethod = false;
            if (thiefTargetSpot != null)
            {
                string[] methodNames = { "PlaceItem", "ReturnItem", "AddItem", "ReceiveItem", "SetItem", "InsertItem" };
                foreach (var mName in methodNames)
                {
                    var m = thiefTargetSpot.GetType().GetMethod(mName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (m == null) continue;

                    var parms = m.GetParameters();
                    try
                    {
                        if (parms.Length == 1)
                        {
                            var pt = parms[0].ParameterType;
                            if (pt.IsInstanceOfType(cartItem))
                            {
                                m.Invoke(thiefTargetSpot, new object[] { cartItem });
                                returnedViaMethod = true;
                                break;
                            }
                            if (itemGo != null && pt.IsInstanceOfType(itemGo))
                            {
                                m.Invoke(thiefTargetSpot, new object[] { itemGo });
                                returnedViaMethod = true;
                                break;
                            }
                            // accept generic UnityEngine.Object
                            if (itemGo != null && typeof(UnityEngine.Object).IsAssignableFrom(pt))
                            {
                                m.Invoke(thiefTargetSpot, new object[] { itemGo });
                                returnedViaMethod = true;
                                break;
                            }
                        }
                        else if (parms.Length == 0)
                        {
                            m.Invoke(thiefTargetSpot, null);
                            returnedViaMethod = true;
                            break;
                        }
                    }
                    catch { /* swallow reflection invocation errors to keep this robust */ }
                }
            }

            // If no method returned the item, try placing the GameObject at the return point transform
            if (!returnedViaMethod && itemGo != null && returnPoint != null)
            {
                try
                {
                    itemGo.transform.position = returnPoint.position;
                    itemGo.transform.rotation = returnPoint.rotation;
                    itemGo.transform.SetParent(returnPoint, true);
                }
                catch { }
            }

            gameObject.GetComponent<NPCController>().cart.Remove(cartItem);
        }

        // Reset thief state and make them leave
        isTheif = false;
        thiefHasTarget = false;
        thiefInteracting = false;
        isLeaving = true;

        if (agent != null)
        {
            agent.isStopped = false;
            if (exitPoint != null)
                agent.SetDestination(exitPoint.position);
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
