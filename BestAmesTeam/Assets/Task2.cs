using UnityEngine;

public class Task2 : MonoBehaviour
{
    public int QuestNumber;
    void OnTriggerStay(Collider other)
    {
        if (other.name.Contains("Box"))
        {
            TaskDisplayer.instance.Tasks[QuestNumber].completed = true;
            Destroy(gameObject);
        }
    }
}
