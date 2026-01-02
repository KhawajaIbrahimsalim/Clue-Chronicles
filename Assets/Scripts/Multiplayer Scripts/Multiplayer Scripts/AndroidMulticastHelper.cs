using UnityEngine;

public class AndroidMulticastHelper : MonoBehaviour
{
#if UNITY_ANDROID && !UNITY_EDITOR
    AndroidJavaObject multicastLock;
#endif

    void Start()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            var wifi = activity.Call<AndroidJavaObject>("getSystemService", "wifi");

            multicastLock = wifi.Call<AndroidJavaObject>("createMulticastLock", "unity_netcode_lock");
            multicastLock.Call("setReferenceCounted", false);
            multicastLock.Call("acquire");

            Debug.Log("✅ Android multicast lock acquired");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"⚠️ Could not acquire multicast lock: {e.Message}");
        }
#endif
    }

    void OnDestroy()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (multicastLock != null)
        {
            try
            {
                multicastLock.Call("release");
                Debug.Log("✅ Android multicast lock released");
            }
            catch { }
        }
#endif
    }
}