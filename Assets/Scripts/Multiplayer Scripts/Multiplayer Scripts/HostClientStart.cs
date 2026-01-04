using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class HostClientStart : MonoBehaviour
{
    public LanHostBroadcaster hostBroadcaster;
    
    public void StartHost()
    {
        Debug.Log("🎮 Host button clicked");
        StartCoroutine(StartHostCoroutine());
    }
    
    IEnumerator StartHostCoroutine()
    {
        Debug.Log("🔄 Starting host sequence...");
        
        // Shutdown any existing connection
        if (NetworkManager.Singleton.IsListening)
        {
            Debug.Log("⚠️ Shutting down existing connection...");
            NetworkManager.Singleton.Shutdown();
            yield return new WaitForSeconds(0.5f);
        }
        
        // Start host
        Debug.Log("🚀 Starting host...");
        if (NetworkManager.Singleton.StartHost())
        {
            Debug.Log("✅ Host started successfully!");
            
            // Enable broadcaster
            if (hostBroadcaster != null)
            {
                hostBroadcaster.enabled = true;
                Debug.Log("📡 Broadcaster enabled");
            }
            
            // SIMPLIFIED: Load gameplay scene directly after a short delay
            yield return new WaitForSeconds(1f);
            
            Debug.Log("📁 Loading gameplay scene...");
            NetworkManager.Singleton.SceneManager.LoadScene("GamePlay", LoadSceneMode.Single);
        }
        else
        {
            Debug.LogError("❌ Failed to start host");
        }
    }
    
    public void StartClient()
    {
        Debug.Log("🎮 Client button clicked");
        
        // Just enable client mode - actual connection happens via LanDiscovery
        if (!NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.StartClient();
            Debug.Log("🔄 Client mode ready - waiting for host selection");
        }
    }
}