using Unity.Netcode;
using UnityEngine;

public class OperatorUI : MonoBehaviour
{
    public static OperatorUI Instance;
    public GameObject rootUI;

    void Awake()
    {
        Instance = this;

        // Host should NEVER see this UI
        if (NetworkManager.Singleton.IsHost)
            rootUI.SetActive(false);
    }

    public void EnableUI(bool value)
    {
        if (!NetworkManager.Singleton.IsHost)
            rootUI.SetActive(value);
    }

    public void DisableUI()
    {
        rootUI.SetActive(false);
    }
}
