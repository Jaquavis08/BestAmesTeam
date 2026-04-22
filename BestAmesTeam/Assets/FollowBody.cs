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

        // Build map from mainBody
        Dictionary<string, Transform> parentMap = new Dictionary<string, Transform>();

        foreach (Transform t in mainBody.GetComponentsInChildren<Transform>(true))
        {
            if (!parentMap.ContainsKey(t.name))
            {
                parentMap.Add(t.name, t);
            }
        }

        // Match from partsList
        for (int i = 0; i < partsList.Count; i++)
        {
            foreach (Transform t in partsList[i].GetComponentsInChildren<Transform>(true))
            {
                if (!t.CompareTag("CharacterEdit")) continue;

                if (parentMap.TryGetValue(t.name, out Transform match))
                {
                    partsToFollow.Add(t);
                    partsParent.Add(match);

                    Debug.Log($"Matched: {t.name}");
                }
            }
        }

        Debug.Log($"FINAL COUNT: {partsToFollow.Count}");
    }


    string GetPath(Transform t, Transform root)
    {
        string path = t.name;

        while (t.parent != null && t.parent != root)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }

        return path;
    }
}