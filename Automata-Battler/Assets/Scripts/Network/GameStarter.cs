using UnityEngine;
using Unity.Netcode;

public class GameStarter : NetworkBehaviour // Loads the game scene from the main menu (decoupled from RelayManager so RelayManager doesn't have to be a NetworkObject)
{
    // Make this a singleton
    public static GameStarter Instance { get; private set; }

    public bool singleplayer = false;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("More than one GameStarter instance!");
        }
        Instance = this;
    }

    public override void OnNetworkSpawn() // Runs when this NetworkObject is instantiated *on the network*
    // (this is kinda the equivalent of Start() for NetworkObjects, but doesn't run until a connection is established, i.e. if you start hosting or join a session as client)
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback; // Fire off a function every time a client connects
        }
    }

    private void NetworkManager_OnClientConnectedCallback(ulong obj)
    {
        // Load game when both players have connected
        if(NetworkManager.Singleton.ConnectedClientsList.Count == 2 || singleplayer)
        {
            // TODO: Replace scene name with actual game scene name (if we ever change that)
            NetworkManager.Singleton.SceneManager.LoadScene("Scenes/GridTesting", UnityEngine.SceneManagement.LoadSceneMode.Single); // LoadMode: only loads the new scene, unloads all previous ones
        }
    }
}
