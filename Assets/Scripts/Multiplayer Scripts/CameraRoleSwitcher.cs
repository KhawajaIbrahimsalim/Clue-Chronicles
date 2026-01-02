using UnityEngine;
using Cinemachine;
using Unity.Netcode;

public class CameraRoleSwitcher : MonoBehaviour
{
    public CinemachineVirtualCamera controllerCam;
    public CinemachineVirtualCamera spectatorCam;

    void Start()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            controllerCam.Priority = 20;
            spectatorCam.Priority = 0;
        }
        else
        {
            controllerCam.Priority = 0;
            spectatorCam.Priority = 20;
        }
    }
}
