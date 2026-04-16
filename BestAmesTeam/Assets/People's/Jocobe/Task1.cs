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
}
