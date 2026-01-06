using UnityEngine;

public class PuzzelManager : MonoBehaviour
{
    private int fingerPrintCode = 0;
    private int ActualFingerPrintCode = 3; // Example correct code

    [Header("UI Elements")]
    [SerializeField] private GameObject FingerPrint_SolvedUI;
    [SerializeField] private GameObject FingerPrint_NotCorrentUI;

    public void SetFingerPrintCode(int code)
    {
        fingerPrintCode = code;

        if (fingerPrintCode == ActualFingerPrintCode)
        {
            FingerPrint_SolvedUI.SetActive(true);
            FingerPrint_NotCorrentUI.SetActive(false);
        }
        else
        {
            FingerPrint_SolvedUI.SetActive(false);
            FingerPrint_NotCorrentUI.SetActive(true);
        }
    }

    public void SetActualFingerPrintCode(int code)
    {
        ActualFingerPrintCode = code;
    }
}
