using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Net.NetworkInformation;
using System.Linq;

public class LanHostBroadcaster : MonoBehaviour
{
    UdpClient udp;
    IPEndPoint endPoint;

    const int PORT = 47777;

    void Start()
    {
        IPAddress broadcastIP = GetBroadcastAddress();

        if (broadcastIP == null)
        {
            Debug.LogError("❌ No broadcast address found");
            return;
        }

        udp = new UdpClient();
        udp.EnableBroadcast = true;
        endPoint = new IPEndPoint(broadcastIP, PORT);

        InvokeRepeating(nameof(BroadcastHost), 1f, 2f);

        Debug.Log("📡 Broadcasting to " + broadcastIP);
    }

    void BroadcastHost()
    {
        string message = "HOST|MyGameHost";
        byte[] data = Encoding.UTF8.GetBytes(message);
        udp.Send(data, data.Length, endPoint);
    }

    IPAddress GetBroadcastAddress()
    {
        foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (ni.OperationalStatus != OperationalStatus.Up)
                continue;

            var ipProps = ni.GetIPProperties();
            foreach (var ua in ipProps.UnicastAddresses)
            {
                if (ua.Address.AddressFamily == AddressFamily.InterNetwork)
                {
                    byte[] ip = ua.Address.GetAddressBytes();
                    byte[] mask = ua.IPv4Mask.GetAddressBytes();

                    byte[] broadcast = new byte[4];
                    for (int i = 0; i < 4; i++)
                        broadcast[i] = (byte)(ip[i] | (~mask[i]));

                    return new IPAddress(broadcast);
                }
            }
        }
        return null;
    }

    void OnDestroy()
    {
        udp?.Close();
    }
}
