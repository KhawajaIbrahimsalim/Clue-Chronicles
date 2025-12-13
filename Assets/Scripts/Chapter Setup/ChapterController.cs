using System.Collections;
using TMPro;
using UnityEngine;

[System.Serializable]
public class ChapterSettings
{
    public string chapterName;
    public GameObject ChapterPrefab;
    public int ObjectiveCount;
    public string[] ObjectivesDescriptions;
}

public class ChapterController : MonoBehaviour
{
    public ChapterSettings[] chapters;
    public static ChapterController Instance;

    public int CurrentChapterIndex = 0;
    public int Objectiveindex = 0;

    [Header("UI Elements")]
    public GameObject ChapterComplete_Canvas;
    public TextMeshProUGUI ObjectiveText;
    public TextMeshProUGUI ChapterText;

    void Awake()
    {
        Instance = this;

        chapters[CurrentChapterIndex].ChapterPrefab.SetActive(true);

        ObjectiveText.text = chapters[CurrentChapterIndex].ObjectivesDescriptions[Objectiveindex];
        ChapterText.text = chapters[CurrentChapterIndex].chapterName;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AdvanceObjective()
    {
        Objectiveindex++;
        if (Objectiveindex >= chapters[CurrentChapterIndex].ObjectiveCount)
        {
            AdvanceChapter();
        }
    }

    public void AdvanceChapter()
    {
        chapters[CurrentChapterIndex].ChapterPrefab.SetActive(false);
        CurrentChapterIndex++;
        Objectiveindex = 0;

        if (CurrentChapterIndex < chapters.Length)
        {
            chapters[CurrentChapterIndex].ChapterPrefab.SetActive(true);
        }
        else
        {
            StartCoroutine(ShowChapterCompleteUI());
        }
    }

    IEnumerator ShowChapterCompleteUI()
    {
        yield return new WaitForSeconds(3f);
        ChapterComplete_Canvas.SetActive(true);
    }
}
