using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : NetworkBehaviour
{
    public static SceneLoader Instance;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    // NEW SYNTAX: [ServerRpc(RequireOwnership = false)] is now just [ServerRpc]
    // The default behavior is to allow any client to call it
    [ServerRpc]
    public void LoadGameSceneServerRpc()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("GamePlay", LoadSceneMode.Single);
        }
    }
    
    public void LoadSceneForAll(string sceneName)
    {
        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }
        else if (IsClient)
        {
            // Request scene load from server
            LoadGameSceneServerRpc();
        }
    }
}