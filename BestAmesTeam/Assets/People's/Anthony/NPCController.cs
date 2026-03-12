using UnityEngine;
using UnityEngine.AI;

public class NPCController : MonoBehaviour
{
    public NavMeshAgent agent;

    public float wanderRadius = 10f;
    public float wanderTimer = 5f;

    private float timer;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        timer = wanderTimer;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= wanderTimer)
        {
            Vector3 newPos = RandomNavmechLocation(wanderRadius);

            agent.SetDestination(newPos);
            timer = 0;
        }
    }

    public Vector3 RandomNavmechLocation(float radius)
    {
        Vector3 randomDirection = Random.insideUnitSphere * radius;
        randomDirection += transform.position;
        NavMeshHit navHit;
        NavMesh.SamplePosition(randomDirection, out navHit, radius, NavMesh.AllAreas);
        return navHit.position;
    }
}
