using UnityEngine;

[CreateAssetMenu(fileName = "RaceDictionary", menuName = "Scriptable Objects/RaceDictionary")]
public class RaceDictionary : ScriptableObject
{
    public Texture2D[] Races;

    public Texture2D GetRandomRace()
    {
        int number = UnityEngine.Random.Range(0, Races.Length);
        return Races[number];
    }
}
