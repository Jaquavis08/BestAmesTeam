using System.Collections.Generic;
using UnityEngine;

public class FollowBody : MonoBehaviour
{
    public List<Transform> partsToFollow = new List<Transform>();
    public List<Transform> partsParent = new List<Transform>();

    public Transform mainBody;
    public List<Transform> partsList = new List<Transform>();

    void Update()
    {
        if (partsToFollow.Count <= 0 || partsParent.Count <= 0)
        {
            Repair();
        }

        for (int i = 0; i < partsToFollow.Count; i++)
        {
            if (partsToFollow[i].position != partsParent[i].position || partsToFollow[i].rotation != partsParent[i].rotation)
            {
                partsToFollow[i].position = partsParent[i].position;
                partsToFollow[i].rotation = partsParent[i].rotation;
            }
        }
    }

    public void Repair()
    {
        partsParent.Clear();
        for (int i = 0; i < mainBody.GetChild(0).childCount; i++)
        {
            partsParent.Add(mainBody.GetChild(0).GetChild(i));
        }

        partsToFollow.Clear();
        for (int i = 0; i < partsList.Count; i++)
        {
            Debug.LogError(i);
            for (int x = 0; x < partsList[i].childCount - 1; x++)
            {
                Debug.LogError(x);
                for (int y = 0; y < partsList[i].GetChild(0).GetChild(0).childCount; y++)
                {
                    Debug.LogError(y);
                    partsToFollow.Add(partsList[i].GetChild(0).GetChild(0).GetChild(y));
                }
            }
        }
    }
}
