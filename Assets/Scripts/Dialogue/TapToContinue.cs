using UnityEngine;
using UnityEngine.InputSystem;

public class TapToContinue : MonoBehaviour
{
    void Update()
    {
        if (!DialogueManager.Instance.IsActive)
            return;

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            DialogueManager.Instance.NextLine();
            Debug.Log("TapToContinue: Screen tapped to continue dialogue.");
        }
    }
}
