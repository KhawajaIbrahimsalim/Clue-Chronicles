using UnityEngine;
using Cinemachine;
using Unity.Netcode;

public class CameraRoleSwitcher : NetworkBehaviour
{
    public CinemachineVirtualCamera controllerCam;
    public CinemachineVirtualCamera spectatorCam;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        // Give it a small delay to ensure NetworkManager is ready
        Invoke(nameof(SetupCameras), 0.1f);
    }

    void SetupCameras()
    {
        if (NetworkManager.Singleton == null) return;

        if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
        {
            Debug.Log("🎥 Setting up HOST camera");
            if (controllerCam != null) controllerCam.Priority = 20;
            if (spectatorCam != null) spectatorCam.Priority = 0;
        }
        else if (NetworkManager.Singleton.IsClient)
        {
            Debug.Log("🎥 Setting up CLIENT camera");
            if (controllerCam != null) controllerCam.Priority = 0;
            if (spectatorCam != null) spectatorCam.Priority = 20;
        }
    }
}