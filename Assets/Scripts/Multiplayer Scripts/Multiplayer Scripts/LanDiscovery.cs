using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using TMPro;

public class LanDiscovery : MonoBehaviour
{
    [Header("UI References")]
    public Transform contentParent;
    public GameObject hostButtonPrefab;
    public TextMeshProUGUI statusText;

    HashSet<string> foundHosts = new HashSet<string>();
    UdpClient client;
    private bool isConnecting = false;

    const int port = 47777;
    
    private List<string> localIPs = new List<string>();

    void Start()
    {
        // Don't run discovery if we're the host
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost)
        {
            Debug.Log("🎮 I'm the host - disabling discovery");
            gameObject.SetActive(false);
            return;
        }
        
        GetLocalIPAddresses();
        StartCoroutine(InitializeDiscovery());
    }

    void GetLocalIPAddresses()
    {
        try
        {
            string hostName = Dns.GetHostName();
            IPAddress[] addresses = Dns.GetHostAddresses(hostName);
            
            foreach (IPAddress ip in addresses)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIPs.Add(ip.ToString());
                }
            }
            
            localIPs.Add("127.0.0.1");
        }
        catch { }
    }

    IEnumerator InitializeDiscovery()
    {
        yield return new WaitForSeconds(0.5f);
        
        try
        {
            client = new UdpClient(port);
            client.EnableBroadcast = true;
            StartCoroutine(ListenForHosts());
            UpdateStatus("🔍 Looking for active games...");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Discovery error: {e.Message}");
        }
    }

    IEnumerator ListenForHosts()
    {
        while (true)
        {
            if (client.Available > 0)
            {
                try
                {
                    IPEndPoint ep = new IPEndPoint(IPAddress.Any, port);
                    byte[] data = client.Receive(ref ep);
                    string hostIP = ep.Address.ToString();
                    string message = Encoding.UTF8.GetString(data);
                    
                    // CRITICAL: Only accept ACTIVE_HOST messages
                    if (!message.StartsWith("ACTIVE_HOST"))
                    {
                        continue; // Ignore non-host messages
                    }
                    
                    // Check if this is our own IP
                    if (IsLocalIP(hostIP))
                    {
                        continue; // Skip our own IP
                    }
                    
                    if (!foundHosts.Contains(hostIP))
                    {
                        foundHosts.Add(hostIP);
                        
                        // Parse game info from message
                        string[] parts = message.Split('|');
                        string gameName = parts.Length > 1 ? parts[1] : "Game";
                        string gameInfo = parts.Length > 2 ? parts[2] : "";
                        
                        Debug.Log($"🎮 Found active game: {hostIP} - {gameName}");
                        UpdateStatus($"Found: {gameName}");
                        CreateHostButton(hostIP, gameName, gameInfo);
                    }
                }
                catch { }
            }
            yield return null;
        }
    }

    bool IsLocalIP(string ip)
    {
        return localIPs.Contains(ip);
    }

    void CreateHostButton(string ip, string gameName, string gameInfo)
    {
        if (contentParent == null || hostButtonPrefab == null) return;

        GameObject btnObj = Instantiate(hostButtonPrefab, contentParent);
        TextMeshProUGUI txt = btnObj.GetComponentInChildren<TextMeshProUGUI>();
        Button btn = btnObj.GetComponent<Button>();

        if (txt != null)
            txt.text = $"{gameName}\n{ip}\n{gameInfo}";

        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                if (!isConnecting)
                    JoinHost(ip);
            });
        }
    }

    void JoinHost(string ip)
    {
        if (isConnecting) return;
        
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        if (transport != null)
        {
            transport.SetConnectionData(ip, 7777);
        }
        
        isConnecting = true;
        UpdateStatus($"Joining {ip}...");
        
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        
        if (NetworkManager.Singleton.StartClient())
        {
            Debug.Log($"🔄 Connecting to active game at {ip}");
        }
        else
        {
            UpdateStatus("❌ Failed to connect");
            isConnecting = false;
        }
    }

    void OnClientConnected(ulong clientId)
    {
        Debug.Log($"✅ Connected to game! ID: {clientId}");
        UpdateStatus("✅ Connected! Loading game...");
        isConnecting = false;
        
        // SIMPLIFIED: Just wait for scene to load without subscribing
        // The client will automatically load whatever scene the host loads
        UpdateStatus("✅ Connected! Waiting for host to start game...");
    }

// Remove or comment out the OnSceneLoaded method if not needed

    // FIX: Correct signature for OnLoadComplete
    // void OnSceneLoaded(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    // {
    //     // Check if this scene load is for us
    //     if (clientId == NetworkManager.Singleton.LocalClientId || clientId == 0)
    //     {
    //         Debug.Log($"📁 Client loaded scene: {sceneName}");
    //         UpdateStatus($"Loaded: {sceneName}");
            
    //         if (sceneName == "GamePlay")
    //         {
    //             // Hide discovery UI
    //             if (gameObject != null)
    //                 gameObject.SetActive(false);
    //         }
    //     }
    // }

    void OnClientDisconnected(ulong clientId)
    {
        string reason = NetworkManager.Singleton.DisconnectReason;
        if (string.IsNullOrEmpty(reason)) reason = "Connection failed";
        
        Debug.Log($"❌ Disconnected: {reason}");
        UpdateStatus($"❌ {reason}");
        isConnecting = false;
        
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
    }

    void UpdateStatus(string message)
    {
        Debug.Log(message);
        if (statusText != null)
            statusText.text = message;
    }

    public void RefreshHosts()
    {
        foundHosts.Clear();
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
        UpdateStatus("Refreshing active games...");
    }

    
// Also add cleanup in OnDestroy
    void OnDestroy()
    {
        client?.Close();
    }
}