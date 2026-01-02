using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class HostClientStart : MonoBehaviour
{
    public LanHostBroadcaster hostBroadcaster;
    
    public void StartHost()
    {
        StartCoroutine(StartHostCoroutine());
    }
    
    IEnumerator StartHostCoroutine()
    {
        // Shutdown any existing connection
        if (NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.Shutdown();
            yield return new WaitForSeconds(0.5f);
        }
        
        // Start host
        if (NetworkManager.Singleton.StartHost())
        {
            Debug.Log("✅ Host started");
            
            // Enable broadcaster (it will auto-start in its Start() method)
            if (hostBroadcaster != null)
            {
                hostBroadcaster.enabled = true;
                Debug.Log("✅ Broadcaster enabled");
            }
            
            // Load gameplay scene
            NetworkManager.Singleton.SceneManager.LoadScene("Loading", LoadSceneMode.Single);
        }
        else
        {
            Debug.LogError("❌ Failed to start host");
        }
    }
    
    public void StartClient()
    {
        // Just enable client mode - actual connection happens via LanDiscovery
        if (!NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.StartClient();
        }
    }
}