using UnityEngine;

public class Task1 : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            TaskDisplayer.instance.Tasks[0].completed = true; // Mark the first task as completed
            Destroy(gameObject);
        }
    }
}
