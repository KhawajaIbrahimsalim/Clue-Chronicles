using System.Collections;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    private DialogueData current;
    private int index;
    private bool active;

    void Awake()
    {
        Instance = this;
    }

    public bool IsActive => active;

    public void StartDialogue(DialogueData data)
    {
        current = data;
        index = 0;
        active = true;

        DialogueUI.Instance.Show(
            current.lines[index].speaker,
            current.lines[index].text
        );
    }

    public void NextLine()
    {
        if (!active) return;

        index++;

        if (index >= current.lines.Length)
        {
            EndDialogue();
            return;
        }

        var line = current.lines[index];
        DialogueUI.Instance.Show(line.speaker, line.text);

        Debug.Log($"DialogueManager: NextLine to index {index}");
    }

    void EndDialogue()
    {
        active = false;
        DialogueUI.Instance.Hide();
    }
}
