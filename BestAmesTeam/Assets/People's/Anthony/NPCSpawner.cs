using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    public GameObject npcPrefab;
    public Transform spawnPoint;
    public Transform NPCFolder;

    public float spawnInterval = 8f;
    public int maxCustomers = 10;

    private int currentCustomers = 0;

    void Start()
    {
        InvokeRepeating(nameof(SpawnCustomer), 2f, spawnInterval);
    }

    void SpawnCustomer()
    {
        if (currentCustomers >= maxCustomers) return;
        if (npcPrefab == null || spawnPoint == null)
        {
            Debug.LogWarning("NPC Prefab or Spawn Point not assigned.");
            return;
        }

        GameObject npc = Instantiate(npcPrefab, spawnPoint.position, spawnPoint.rotation);
        npc.transform.parent = NPCFolder.transform;
        currentCustomers++;


        NPCController controller = npc.GetComponent<NPCController>();
        if (controller != null)
        {
            controller.npcSpawner = this;
        }
    }

    public void CustomerLeft()
    {
        currentCustomers--;
        if (currentCustomers < 0) currentCustomers = 0;
    }
}