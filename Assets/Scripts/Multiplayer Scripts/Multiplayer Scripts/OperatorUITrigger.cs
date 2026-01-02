using Unity.Netcode;
using UnityEngine;

public class OperatorUITrigger : NetworkBehaviour
{
    public void Activate()
    {
        if (!IsOwner) return;
        Activate_ServerRpc();
    }

    [ServerRpc]
    void Activate_ServerRpc()
    {
        Activate_ClientRpc();
    }

    [ClientRpc]
    void Activate_ClientRpc()
    {
        // Only Client (Player 2)
        if (NetworkManager.Singleton.IsClient &&
            !NetworkManager.Singleton.IsHost)
        {
            OperatorUI.Instance.EnableUI(true);
        }
    }
}
