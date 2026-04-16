using UnityEngine;

public class RandomCharacter : MonoBehaviour
{
    public RaceDictionary raceDictionary;
    public GameObject CharacterBase;

    void Awake()
    {
        CharacterBase.GetComponent<SkinnedMeshRenderer>().material = raceDictionary.GetRandomRace();
    }
}
