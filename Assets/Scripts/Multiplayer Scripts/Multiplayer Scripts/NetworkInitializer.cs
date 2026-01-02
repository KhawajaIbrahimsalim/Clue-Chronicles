using Unity.Netcode;
using UnityEngine;
using Unity.Netcode.Transports.UTP;
using System.Net;

public class NetworkInitializer : MonoBehaviour
{
    void Awake()
    {
        InitializeNetworkSettings();
    }

    void InitializeNetworkSettings()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("❌ NetworkManager not found!");
            return;
        }

        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        if (transport != null)
        {
            // CRITICAL: Set ServerListenAddress to 0.0.0.0 for Android
            transport.SetConnectionData(
                "0.0.0.0",  // Listen on all interfaces
                7777,        // Server port
                "0.0.0.0"    // Server listen address
            );
            
            transport.MaxConnectAttempts = 20;
            transport.ConnectTimeoutMS = 15000;
            
            Debug.Log("✅ Network transport configured for Android");
        }
    }
}