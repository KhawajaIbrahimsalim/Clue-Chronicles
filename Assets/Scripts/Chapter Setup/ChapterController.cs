using UnityEngine;

public class ChapterController : MonoBehaviour
{
    [SerializeField] private ChapterProperties[] chapters;
    public static ChapterController Instance;

    public int CurrentChapterIndex = 0;
    public int Objectiveindex = 0;

    void Awake()
    {
        Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        chapters[CurrentChapterIndex].gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
