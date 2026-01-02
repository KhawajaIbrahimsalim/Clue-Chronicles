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
    public Transform contentParent;       // ScrollView/Viewport/Content
    public GameObject hostButtonPrefab;    // Button prefab

    HashSet<string> foundHosts = new HashSet<string>();
    UdpClient client;

    const int port = 47777;

    void Start()
    {
        client = new UdpClient(port);
        client.EnableBroadcast = true;
        StartCoroutine(ListenForHosts());
        Debug.Log("📡 Listening for LAN hosts...");
    }

    IEnumerator ListenForHosts()
    {
        IPEndPoint ep = new IPEndPoint(IPAddress.Any, port);

        while (true)
        {
            if (client.Available > 0)
            {
                byte[] data = client.Receive(ref ep);
                string hostIP = ep.Address.ToString();

                if (!foundHosts.Contains(hostIP))
                {
                    foundHosts.Add(hostIP);
                    Debug.Log("📡 Host found: " + hostIP);
                    CreateHostButton(hostIP);
                }
            }
            yield return null;
        }
    }

    void CreateHostButton(string ip)
    {
        GameObject btnObj = Instantiate(hostButtonPrefab, contentParent);
        TextMeshProUGUI txt = btnObj.GetComponentInChildren<TextMeshProUGUI>();
        Button btn = btnObj.GetComponent<Button>();

        txt.text = "Join Host: " + ip;

        btn.onClick.AddListener(() =>
        {
            JoinHost(ip);
        });
    }

    void JoinHost(string ip)
    {
        Debug.Log("➡️ Joining host: " + ip);
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.ConnectionData.Address = ip;
        NetworkManager.Singleton.StartClient();
    }
}
