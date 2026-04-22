using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    public static NPCSpawner Instance;

     void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [Header("Spawner Settings")]
    public GameObject npcPrefab;
    public Transform spawnPoint;
    public Transform NPCFolder;

    public float spawnInterval = 8f;
    public int maxCustomers = 10;

    public int currentCustomers = 0;

    public bool SpawningNPC = true;

    void Start()
    {
        InvokeRepeating(nameof(SpawnCustomer), 2f, spawnInterval);
    }

    void SpawnCustomer()
    {
        if (currentCustomers >= maxCustomers || SpawningNPC == false) return;
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
            if (controller.ShelfCheck() == false)
            {
                Destroy(npc);
                currentCustomers--;
            }
        }
    }

    public void CustomerLeft()
    {
        currentCustomers--;
        if (currentCustomers < 0) currentCustomers = 0;
        if (TaskDisplayer.instance.Tasks.Count > 3)
            TaskDisplayer.instance.Tasks[3].completed = true;
    }
}