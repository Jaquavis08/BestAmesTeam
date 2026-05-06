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

    private float GnomeSpawnTimeMin = 100f; // 100
    private float GnomeSpawnTimeMax = 240f; // 240
    public GameObject GnomePrefab;
    public int GnomeSpawnCount = 0;

    public int currentCustomers = 0;

    public bool SpawningNPC = true;

    void Start()
    {
        InvokeRepeating(nameof(SpawnCustomer), 2f, spawnInterval);

        InvokeRepeating(nameof(SpawnGnome), Random.Range(GnomeSpawnTimeMin + 30f, GnomeSpawnTimeMax), Random.Range(GnomeSpawnTimeMin, GnomeSpawnTimeMax));
    }


    void SpawnGnome()
    {
        if (GnomePrefab == null || spawnPoint == null)
        {
            Debug.LogWarning("Gnome Prefab or Spawn Point not assigned.");
            return;
        }
        if (GnomeSpawnCount >= 1) return;
        GameObject gnome = Instantiate(GnomePrefab, spawnPoint.position, spawnPoint.rotation);
        gnome.transform.parent = NPCFolder.transform;
        GnomeSpawnCount++;
    }

    public void GnomeLeft()
    {
        GnomeSpawnCount--;
        if (GnomeSpawnCount < 0) GnomeSpawnCount = 0;
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
    }
}