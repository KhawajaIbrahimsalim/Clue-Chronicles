using Unity.Netcode;
using UnityEngine;

public class UISetter : MonoBehaviour
{
    public static UISetter Instance;
    public GameObject rootHostUI;
    public GameObject rootClientUI;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Host should NEVER see this UI
        if (NetworkManager.Singleton.IsHost)
        {
            rootHostUI.SetActive(true);
            rootClientUI.SetActive(false);
        }
        else if (NetworkManager.Singleton.IsClient)
        {
            rootClientUI.SetActive(true);
            rootHostUI.SetActive(false);
        }
    }

    public void EnableUI(bool value)
    {
        if (NetworkManager.Singleton.IsHost)
            rootHostUI.SetActive(false);
        else if (NetworkManager.Singleton.IsClient)
            rootClientUI.SetActive(true);
    }

    public void DisableUI()
    {
        rootClientUI.SetActive(false);
        rootHostUI.SetActive(false);
    }
}
