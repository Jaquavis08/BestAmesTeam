using UnityEngine;

[CreateAssetMenu(fileName = "RaceDictionary", menuName = "Scriptable Objects/RaceDictionary")]
public class RaceDictionary : ScriptableObject
{
    public Material[] Races;

    public Material GetRandomRace()
    {
        int number = UnityEngine.Random.Range(0, Races.Length);
        return Races[number];
    }
}
