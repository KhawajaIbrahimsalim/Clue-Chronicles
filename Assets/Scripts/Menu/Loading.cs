using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Loading : MonoBehaviour
{
    public Slider loadingBar;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(StartLoading());
    }

    IEnumerator StartLoading()
    {
        float progress = 0f;
        while (progress < 1f)
        {
            progress += 0.1f;
            loadingBar.value = progress;
            yield return new WaitForSeconds(0.1f);
        }
    }
}
