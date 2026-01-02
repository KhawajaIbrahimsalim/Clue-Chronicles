using UnityEngine;

public class AndroidMulticastHelper : MonoBehaviour
{
#if UNITY_ANDROID && !UNITY_EDITOR
    AndroidJavaObject multicastLock;
#endif

    void Awake()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        var wifi = activity.Call<AndroidJavaObject>("getSystemService", "wifi");

        multicastLock = wifi.Call<AndroidJavaObject>("createMulticastLock", "unity_lock");
        multicastLock.Call("setReferenceCounted", false);
        multicastLock.Call("acquire");

        Debug.Log("✅ Multicast lock acquired");
#endif
    }
}
