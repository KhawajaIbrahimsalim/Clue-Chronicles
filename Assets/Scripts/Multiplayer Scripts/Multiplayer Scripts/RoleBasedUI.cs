using Unity.Netcode;
using UnityEngine;

public class RoleBasedUI : MonoBehaviour
{
    public enum UIRole { HostOnly, ClientOnly, Both }
    public UIRole uiRole = UIRole.Both;
    
    void Start()
    {
        Invoke(nameof(CheckAndSetUI), 0.5f);
    }
    
    void CheckAndSetUI()
    {
        if (NetworkManager.Singleton == null) return;
        
        bool shouldShow = false;
        
        switch (uiRole)
        {
            case UIRole.HostOnly:
                shouldShow = NetworkManager.Singleton.IsHost;
                break;
            case UIRole.ClientOnly:
                shouldShow = NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsHost;
                break;
            case UIRole.Both:
                shouldShow = true;
                break;
        }
        
        gameObject.SetActive(shouldShow);
        
        if (shouldShow)
            Debug.Log($"UI '{gameObject.name}' shown for {uiRole}");
    }
}