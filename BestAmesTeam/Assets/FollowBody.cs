using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

[ExecuteAlways]
public class FollowBody : MonoBehaviour
{
    public AccessoriesDictionary AccessoriesDictionary;
    public RaceDictionary raceDictionary;

    public List<Transform> partsToFollow = new List<Transform>();
    public List<Transform> partsParent = new List<Transform>();

    public Transform mainBody;
    public GameObject CharacterBase;
    public List<Transform> partsList = new List<Transform>();

    private void Awake()
    {
        Setup();
    }

    void LateUpdate()
    {
        if (mainBody == null || partsList.Count == 0) return;

        if (partsToFollow.Count == 0 || partsParent.Count == 0)
        {
            Setup();
        }

        int count = Mathf.Min(partsToFollow.Count, partsParent.Count);

        for (int i = 0; i < count; i++)
        {
            if (partsToFollow[i] == null || partsParent[i] == null) continue;

            partsToFollow[i].position = partsParent[i].position;
            partsToFollow[i].rotation = partsParent[i].rotation;
        }
    }

    [ContextMenu("Setup")]
    public void Setup()
    {
        GetOutfits();
        Repair();
    }

    [ContextMenu("Clear")]
    public void Clear()
    {
        while (this.transform.childCount > 2)
        {
            Transform child = this.transform.GetChild(2);
            if (child.CompareTag("CharacterEdit"))
            {
                DestroyImmediate(child.gameObject);
            }
        }

        partsList.Clear();
    }

    [ContextMenu("GetOutfit")]
    public void GetOutfits()
    {
        Clear();

        // Spawn Shirt
        GameObject shirt = Instantiate(AccessoriesDictionary.Shirts[Random.Range(0, AccessoriesDictionary.Shirts.Count)]).gameObject;
        shirt.transform.parent = this.transform;
        shirt.transform.localPosition = Vector3.zero;
        shirt.transform.localScale = Vector3.one;

        // Spawn Pants
        GameObject pants = Instantiate(AccessoriesDictionary.Pants[Random.Range(0, AccessoriesDictionary.Pants.Count)]).gameObject;
        pants.transform.parent = this.transform;
        pants.transform.localPosition = Vector3.zero;
        pants.transform.localScale = Vector3.one;


        CharacterBase.GetComponent<SkinnedMeshRenderer>().material = raceDictionary.GetRandomRace();

        // Implementation for GetOutfit
        for (int i = 2; i < transform.childCount; i++)
        {
            SkinnedMeshRenderer renderer = transform.GetChild(i).GetComponentInChildren<SkinnedMeshRenderer>();
            if (renderer != null)
            {
                renderer.material = AccessoriesDictionary.GetRandomColor();
            }
        }
    }

    [ContextMenu("Repair")]
    public void Repair()
    {
        partsParent.Clear();
        partsToFollow.Clear();
        partsList.Clear();

        // Build map from mainBody
        Dictionary<string, Transform> parentMap = new Dictionary<string, Transform>();

        foreach (Transform t in mainBody.GetComponentsInChildren<Transform>(true))
        {
            if (!parentMap.ContainsKey(t.name))
            {
                parentMap.Add(t.name, t);
            }
        }

        for (int i = 0;i < this.transform.childCount;i++)
        {
            Transform child = this.transform.GetChild(i);
            if (child.CompareTag("CharacterEdit") && !partsList.Contains(child))
            {
                partsList.Add(child);
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