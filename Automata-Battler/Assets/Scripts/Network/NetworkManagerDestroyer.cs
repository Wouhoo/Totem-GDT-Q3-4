using UnityEngine;
using Unity.Netcode;

public class NetworkManagerDestroyer : NetworkBehaviour
{
    // Script to prevent duplicate NetworkManagers
    private void Awake()
    {
        if (NetworkManager.Singleton != null)
            Destroy(gameObject);
    }
}
