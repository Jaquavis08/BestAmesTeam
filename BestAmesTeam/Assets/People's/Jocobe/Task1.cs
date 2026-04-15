using UnityEngine;

public class Task1 : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject spawnPrefab;
    [SerializeField] private Transform spawnParent;
    [SerializeField] private Vector3 spawnOffset = Vector3.zero;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            TaskDisplayer.instance.Tasks[0].completed = true; // Mark the first task as completed
            Destroy(gameObject);
        }
    }

    public void SpawnPrefab()
    {
        if (spawnPrefab == null)
        {
            Debug.LogWarning("SpawnPrefab called but no spawnPrefab is assigned on " + name);
            return;
        }

        Vector3 basePosition = spawnParent != null ? spawnParent.position : transform.position;
        Vector3 spawnPosition = basePosition + spawnOffset;

        Instantiate(spawnPrefab, spawnPosition, Quaternion.identity, spawnParent);
        if (TaskDisplayer.instance.Tasks[3] != null)
            TaskDisplayer.instance.Tasks[3].completed = true;
    }
}
