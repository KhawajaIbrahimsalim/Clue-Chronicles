using System.Diagnostics;
using UnityEngine;

[DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
public class PlayerTriggers : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject Pick_btn;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Interactable Obj"))
        {
            Pick_btn.SetActive(true);
        }
    }

   void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Interactable Obj"))
        {
            Pick_btn.SetActive(false);
        }
    }

    private string GetDebuggerDisplay()
    {
        return ToString();
    }
}
