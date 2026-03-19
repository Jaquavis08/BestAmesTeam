using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TutorialPopUpManager : MonoBehaviour
{
    public List<TutorialPopUp> tutorialPopUps = new List<TutorialPopUp>();
}

[System.Serializable]
public class TutorialPopUp
{
    public string description;
    public Sprite image;
    public Sprite icon;
}