using StarterAssets;
using Unity.Netcode;
using UnityEngine;

public enum PlayerRole
{
    Controller,
    Operator
}

public class PlayerRoleManager : NetworkBehaviour
{
    public PlayerRole Role;

    public override void OnNetworkSpawn()
    {
        if (IsServer && IsOwner)
        {
            Role = PlayerRole.Controller;
            EnableController();
        }
        else if (IsClient && !IsOwner)
        {
            Role = PlayerRole.Operator;
            EnableOperator();
        }
    }

    void EnableController()
    {
        GetComponent<ThirdPersonController>().enabled = true;
        OperatorUI.Instance?.DisableUI();
    }

    void EnableOperator()
    {
        GetComponent<ThirdPersonController>().enabled = false;
        OperatorUI.Instance?.EnableUI(false);
    }
}
