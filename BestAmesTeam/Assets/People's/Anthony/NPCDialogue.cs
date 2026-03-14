using TMPro;
using UnityEngine;

public class NPCDialogue : MonoBehaviour
{
    public TextMeshProUGUI dialogueText;
    public GameObject dialogueBox;

    public void ShowDialogue(string message, float duration = 7f)
    {
        dialogueBox.SetActive(true);
        dialogueText.text = message;

        CancelInvoke();
        Invoke(nameof(HideDialogue), duration);
    }

    void HideDialogue()
    {
        dialogueBox.SetActive(false);
    }
}