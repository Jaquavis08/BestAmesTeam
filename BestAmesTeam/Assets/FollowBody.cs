using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class FollowBody : MonoBehaviour
{
    public List<Transform> partsToFollow = new List<Transform>();
    public List<Transform> partsParent = new List<Transform>();

    public Transform mainBody;
    public List<Transform> partsList = new List<Transform>();

    void LateUpdate()
    {
        if (mainBody == null || partsList.Count == 0) return;

        if (partsToFollow.Count == 0 || partsParent.Count == 0)
        {
            Repair();
        }

        int count = Mathf.Min(partsToFollow.Count, partsParent.Count);

        for (int i = 0; i < count; i++)
        {
            if (partsToFollow[i] == null || partsParent[i] == null) continue;

            partsToFollow[i].position = partsParent[i].position;
            partsToFollow[i].rotation = partsParent[i].rotation;
        }
    }

    [ContextMenu("Repair")]
    public void Repair()
    {
        partsParent.Clear();
        partsToFollow.Clear();

        List<Transform> parentAll = new List<Transform>();
        foreach (Transform t in mainBody.GetComponentsInChildren<Transform>())
        {
            if (t != mainBody)
                parentAll.Add(t);
        }

        List<Transform> followAll = new List<Transform>();
        foreach (Transform part in partsList)
        {
            foreach (Transform t in part.GetComponentsInChildren<Transform>())
            {
                if (t != part)
                    followAll.Add(t);
            }
        }

        int count = Mathf.Min(parentAll.Count, followAll.Count);

        for (int i = 0; i < count; i++)
        {
            partsParent.Add(parentAll[i]);
            partsToFollow.Add(followAll[i]);
        }

        Debug.Log($"Matched {count} transforms");
    }
}