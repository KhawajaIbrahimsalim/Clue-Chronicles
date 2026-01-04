using UnityEngine;
using Cinemachine;
using Unity.Netcode;

public class CameraRoleSwitcher : NetworkBehaviour
{
    public CinemachineVirtualCamera controllerCam;
    public CinemachineVirtualCamera spectatorCam;
    
    void Start()
    {
        // Start with both cameras disabled
        if (controllerCam != null) controllerCam.Priority = 0;
        if (spectatorCam != null) spectatorCam.Priority = 0;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        // Wait a moment for network to be ready
        Invoke(nameof(SetupCameras), 0.3f);
    }

    void SetupCameras()
    {
        if (NetworkManager.Singleton == null) 
        {
            Debug.LogError("NetworkManager not found!");
            return;
        }
        
        Debug.Log($"Setting up cameras - IsOwner: {IsOwner}, IsHost: {NetworkManager.Singleton.IsHost}");
        
        // ONLY setup cameras for OUR character (the one we own)
        if (IsOwner)
        {
            if (NetworkManager.Singleton.IsHost)
            {
                // HOST: Get controller camera to follow detective
                Debug.Log("🎥 HOST gets controller camera");
                if (controllerCam != null) 
                {
                    controllerCam.Priority = 20;
                    
                }
                if (spectatorCam != null) spectatorCam.Priority = 0;
            }
            else
            {
                // CLIENT: Get spectator camera (watch only)
                Debug.Log("🎥 CLIENT gets spectator camera");
                if (controllerCam != null) controllerCam.Priority = 0;
                if (spectatorCam != null) spectatorCam.Priority = 20;
            }
        }
        else
        {
            // This is someone else's character - NO CAMERA
            Debug.Log("🎥 This is another player's character - no camera");
            if (controllerCam != null) controllerCam.Priority = 0;
            if (spectatorCam != null) spectatorCam.Priority = 0;
        }
    }
}