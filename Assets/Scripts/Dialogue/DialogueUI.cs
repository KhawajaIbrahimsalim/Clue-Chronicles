using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    public static DialogueUI Instance;

    public GameObject panel;
    public TextMeshProUGUI speaker;
    public TextMeshProUGUI body;

    void Awake()
    {
        Instance = this;
    }

    public void Show(string speakerName, string text)
    {
        panel.SetActive(true);
        speaker.text = speakerName;
        body.text = text;
    }

    public void Hide()
    {
        panel.SetActive(false);
    }
}
