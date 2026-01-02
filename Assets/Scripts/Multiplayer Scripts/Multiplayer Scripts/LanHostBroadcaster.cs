using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Net.NetworkInformation;

public class LanHostBroadcaster : MonoBehaviour
{
    UdpClient udp;
    private bool isBroadcasting = false;

    const int PORT = 47777;

    void Start()
    {
        StartBroadcasting();
    }

    void StartBroadcasting()
    {
        try
        {
            udp = new UdpClient();
            udp.EnableBroadcast = true;
            
            isBroadcasting = true;
            InvokeRepeating(nameof(BroadcastHost), 1f, 2f);

            Debug.Log("📡 Broadcaster started automatically");
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ Broadcast error: " + e.Message);
        }
    }

    void BroadcastHost()
    {
        if (!isBroadcasting || udp == null) return;
        
        try
        {
            string message = "ACTIVE_HOST|MyGame";
            byte[] data = Encoding.UTF8.GetBytes(message);
            
            // Broadcast to all devices on network
            IPEndPoint broadcastEndPoint = new IPEndPoint(IPAddress.Broadcast, PORT);
            udp.Send(data, data.Length, broadcastEndPoint);
            
            Debug.Log("📤 Broadcast sent: " + message);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Broadcast error: {e.Message}");
        }
    }

    void OnDestroy()
    {
        isBroadcasting = false;
        CancelInvoke(nameof(BroadcastHost));
        udp?.Close();
    }
}