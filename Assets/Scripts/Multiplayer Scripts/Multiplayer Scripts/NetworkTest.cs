using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NetworkTest : NetworkBehaviour
{
    public TextMeshProUGUI statusText;
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        if (IsServer)
        {
            statusText.text = "HOST - Server Ready";
            Debug.Log("🎮 I am the HOST (Server)");
        }
        else if (IsClient)
        {
            statusText.text = "CLIENT - Connected";
            Debug.Log("🎮 I am a CLIENT");
        }
    }
}