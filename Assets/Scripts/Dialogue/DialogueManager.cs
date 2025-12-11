using System.Collections;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    private DialogueData current;
    private int index = 0;
    private Coroutine endDialogueCoroutine;

    void Awake()
    {
        Instance = this;
    }

    public void StartDialogue(DialogueData data)
    {
        current = data;
        index = 0;

        ShowLine();

        // Stop the previous coroutine if it's running
        if (endDialogueCoroutine != null)
        {
            StopCoroutine(endDialogueCoroutine);
        }

        // Start a new coroutine to end the dialogue after a delay
        endDialogueCoroutine = StartCoroutine(EndDialogueAfterDelay(5f));
    }

    public void NextLine()
    {
        index++;

        if (index >= current.lines.Length)
        {
            endDialogueCoroutine = StartCoroutine(EndDialogueAfterDelay(0f));
            return;
        }

        ShowLine();
    }

    void ShowLine()
    {
        var line = current.lines[index];
        DialogueUI.Instance.Show(line.speaker, line.text);
    }

    IEnumerator EndDialogueAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        DialogueUI.Instance.Hide();
    }
}
