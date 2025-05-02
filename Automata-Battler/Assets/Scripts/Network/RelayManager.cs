using UnityEngine;
// Don't forget to include packages!
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using System.Threading.Tasks;
using TMPro;

public class RelayManager : MonoBehaviour // Script for initializing a Unity Relay connection.
{
    // Make this a singleton
    public static RelayManager Instance { get; private set; }

    // UI
    [SerializeField] private TextMeshProUGUI joinCodeText;
    [SerializeField] private TMP_InputField joinCodeField;
    [SerializeField] private GameObject waitingForPlayersScreen;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("More than one RelayManager instance!");
        }
        Instance = this;
    }

    private void Start()
    {
        waitingForPlayersScreen.SetActive(false);
    }

    public void StartSingleplayer()
    {
        // Start the game in singleplayer mode.
        // It is still required to start hosting on NetworkManager, but this is purely so RPCs execute correctly;
        // you don't actually need an internet connection for this to work.
        GameStarter.Instance.singleplayer = true; 
        NetworkManager.Singleton.gameObject.GetComponent<UnityTransport>().SetConnectionData("127.0.0.1", 7777); // Instead of using relay, open a "connection" on localhost
        NetworkManager.Singleton.StartHost();
    }

    public async void CreateRelay()
    {
        await InitializeServices(); // Anything below this point won't be executed until services have been initialized

        // Create a new relay (called by Host)
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(1); // Allocate a server with 1 connection (besides the host)

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId); // Get join code for the newly created relay
            Debug.Log(joinCode);

            RelayServerData relayServerData = AllocationUtils.ToRelayServerData(allocation, "dtls"); // Create data using dtls (secure) connection type
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartHost(); // Start hosting using this relay
            joinCodeText.text = "Join Code: " + joinCode;
            waitingForPlayersScreen.SetActive(true); // Show "waiting for players" screen
        }
        catch (RelayServiceException e) // Show potential error
        {
            Debug.Log(e);
        }
    }

    public async void JoinRelay()
    {
        //Debug.Log("Here1");
        await InitializeServices(); // Anything below this point won't be executed until services have been initialized
        //Debug.Log("Here3");

        // Join a relay (called by Client)
        try
        {
            string joinCode = joinCodeField.text; // Get join code from input field (maybe add validation?)
            Debug.Log("Joining relay with code " + joinCode);
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode); // Join relay with given join code

            RelayServerData relayServerData = AllocationUtils.ToRelayServerData(joinAllocation, "dtls"); // Create data using dtls (secure) connection type
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartClient(); // Log in to the relay as client
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
    }

    private async Task InitializeServices()
    {
        // Initialize & log into Unity Gaming Services
        await UnityServices.InitializeAsync(); 
        // Authenticate by anonymous sign-in (and wait until we're authenticated)
        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId); // Log player ID on sign in
        };
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        //Debug.Log("Here2");
    }
}
