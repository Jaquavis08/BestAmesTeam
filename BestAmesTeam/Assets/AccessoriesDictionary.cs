using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AccessoriesDictionary", menuName = "Scriptable Objects/AccessoriesDictionary")]
public class AccessoriesDictionary : ScriptableObject
{
    public List<GameObject> Shirts;
    public List<GameObject> Pants;
    public List<GameObject> Hair;

    public Material[] materials;

    public Material GetRandomColor()
    {
        int number = UnityEngine.Random.Range(0, materials.Length);
        return materials[number];
    }
}
