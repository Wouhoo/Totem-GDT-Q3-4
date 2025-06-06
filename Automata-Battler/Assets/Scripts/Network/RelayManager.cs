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
using System.Collections;
using UnityEditor;

public class RelayManager : MonoBehaviour // Script for initializing a Unity Relay connection. Also overloaded with some main menu UI stuff.
{
    // Make this a singleton
    public static RelayManager Instance { get; private set; }

    // UI
    [SerializeField] private TextMeshProUGUI joinCodeText;
    [SerializeField] private TMP_InputField joinCodeField;
    [SerializeField] private GameObject waitingForPlayersScreen;
    [SerializeField] private GameObject joiningScreen;
    [SerializeField] private TextMeshProUGUI title;

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
        joiningScreen.SetActive(false);
        //joinCodeField.onEndEdit.AddListener(delegate { JoinRelay(); }); // Join when pressing enter key on input field
        StartCoroutine("AnimateTitleText");
    }


    /* UI STUFF */
    IEnumerator AnimateTitleText()
    {
        while (true)
        {
            // Animate the underscore at the end (cheap little effect to liven up the title screen)
            title.text = "SPARTACUS PROTOCOL_";
            yield return new WaitForSeconds(0.75f);
            title.text = "SPARTACUS PROTOCOL";
            yield return new WaitForSeconds(0.75f);

            // Easter egg :)
            if(Random.Range(0f, 1f) < 0.01f)
            {
                title.text = "SHOUTOUTS TO LEON <3";
                yield return new WaitForSeconds(0.25f);
            }
        }
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
            EditorApplication.ExitPlaymode();
        #else
            Application.Quit();
        #endif
    }


    /* RELAY STUFF (DON'T TOUCH IF YOU DON'T KNOW WHAT YOU'RE DOING) */
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
        waitingForPlayersScreen.SetActive(true); // Show "waiting for players" screen

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
            joinCodeText.text = joinCode;
        }
        catch (RelayServiceException e) // Show potential error
        {
            Debug.Log(e);
        }
    }

    public async void JoinRelay()
    {
        joiningScreen.SetActive(true); // Show "joining" screen

        await InitializeServices(); // Anything below this point won't be executed until services have been initialized

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
    }
}
