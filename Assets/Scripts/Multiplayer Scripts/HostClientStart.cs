using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HostClientStart : MonoBehaviour
{
    public LanHostBroadcaster hostBroadcaster;

    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();

        // Start broadcasting ONLY after host starts
        hostBroadcaster.enabled = true;

        NetworkManager.Singleton.SceneManager.LoadScene(
            "GamePlay", LoadSceneMode.Single);
    }

    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }
}
