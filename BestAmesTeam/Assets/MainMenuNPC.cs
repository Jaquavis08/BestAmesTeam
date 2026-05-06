using UnityEngine;
using UnityEngine.AI;

public class MainMenuNPC : MonoBehaviour
{
    public NavMeshAgent agent;
    public Animator animator;

    [Header("Path")]
    public Transform inStoreTransform;
    public Transform spawnTransform;

    private bool goingToStore = true;

    void Start()
    {
        inStoreTransform = transform.parent.Find("Store").transform;
        spawnTransform = transform.parent.transform;
        agent.SetDestination(inStoreTransform.position);
    }

    void Update()
    {
        if (agent == null) return;

        // Check if NPC reached destination
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (goingToStore)
            {
                GoToSpawn();
            }
        }

        // Optional animation (walking)
        if (animator != null)
        {
            float speed = agent.velocity.magnitude;
            animator.SetFloat("Speed", speed);
        }
    }

    void GoToSpawn()
    {

        // Teleport to spawn point
        agent.SetDestination(spawnTransform.position);
        goingToStore = false;

        // Destroy after short delay (optional)
        Destroy(gameObject, 10f);
    }
}