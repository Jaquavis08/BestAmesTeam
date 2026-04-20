using UnityEngine;

public class boxboi : MonoBehaviour
{
    public static boxboi instance;
    public BoxCollider boxbox;

    public void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }
    }
}
