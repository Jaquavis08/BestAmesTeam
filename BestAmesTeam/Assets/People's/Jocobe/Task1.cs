using UnityEngine;

public class Task1 : MonoBehaviour
{
    public int QuestNumber;
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            TaskDisplayer.instance.Tasks[QuestNumber].completed = true;
            Destroy(gameObject);
        }
    }
}
