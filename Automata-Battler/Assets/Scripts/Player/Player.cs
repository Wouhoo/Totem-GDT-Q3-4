using System.Collections.Generic;
using System.Collections;
using System.Data.Common;
using System.Threading.Tasks;
using NUnit.Framework.Constraints;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(PlayerStateManager))]
[DisallowMultipleComponent]
public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; } // In multiplayer, this is now a singleton

    private PlayerStateManager playerStateManager;
    private CameraController cameraController;

    public ulong playerId { get; private set; } = 0; // server = 1, client = 2. Use 0 for null values (e.g. if a HexCell belongs to neither player, its commander value is 0)
                                                     // CAREFUL: when sending an RPC to a specific player (using RpcTarget.Single), you need to put in the *actual* clientId (0 for server, 1 for client),
                                                     // so don't forget to subtract 1.


    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        playerStateManager = GetComponent<PlayerStateManager>();
        cameraController = GetComponent<CameraController>();

        // Assign intergers here (not start, since the ref needs them at start!)
        playerId = NetworkManager.Singleton.LocalClientId + 1; // The +1 is important so we can use 0 as a "null value"
        Debug.Log(string.Format("PLAYER ID: {0}", playerId));

        // Rotate Scene
        if (2 == playerId) // Rotate around the world center
            transform.RotateAround(new Vector3(0, 0, 0), new Vector3(0, 1, 0), 180);
    }

    private void Start()
    {
        Referee.Instance.PlayerReadyRpc(playerId); // Let the server referee know that this player is ready
        // (even safer: wait with calling this function until all of the above initializations have finished executing)
    }

    //
    // Hand
    //

    public AbstractCard[] _hand { get; private set; } = new AbstractCard[5];

    [SerializeField] public List<Transform> _handSlotTransforms; // Must be of length 5!

    public void RemoveCardFromHand(AbstractCard card)
    {
        for (int i = 0; i < _hand.Length; i++)
        {
            if (_hand[i] == this)
                _hand[i] = null;
        }
    }

    public void DrawCards()
    {
        // W: There used to be a problem that the card manager may not have spawned yet at this point.
        // This *should* be fixed by the check now built into Referee, but I'm keeping this error catch just in case
        if (!CardManager.Instance.IsSpawned)
            Debug.LogError("CARDMANAGER HASN'T SPAWNED YET!!!");

        Debug.Log("DRAWING CARDS");
        for (int i = 0; i < _hand.Length; i++)
        {
            if (_hand[i] == null)
            {
                Debug.Log($"Drawing to slot {i}");
                CardManager.Instance.DrawCardRpc(playerId, i);
            }
        }
    }

    public void AddCardToHand(NetworkBehaviourReference cardReference, int handSlot) // Has to be a separate function so it can be called from CardManager
    {
        //print("ADDING CARD TO HAND");
        if (cardReference.TryGet(out AbstractCard card)) // Get AbstractCard from NetworkBehaviourReference (TryGet returns False if the card cannot be found; this should never happen)
        {
            //print("FOUND CARD");
            _hand[handSlot] = card;
            // Make CardManager sort hand (can't do this locally since the client doesn't have the authority to move card gameobjects).
            // This requires retrieving the NetworkObjectReferences of the cards in hand (you cannot send a list of NetworkBehaviourReferences,
            // but *can* send an *array* of Network*Object*References), sending them to the server, and moving them there.
            // This incurs a significant amount of overhead and is just bad practice all around, but if cards are to be networkobjects, we kinda have to do it this way.
            // If we had set up the game with multiplayer in mind from the get-go, the card visual (GameObject) would be decoupled from its logic (a data container);
            // in that case we would sync the Vector3 position in the card data and let client and server both move their visual object locally.

            // Note: this *seems* to be going all right now, although I cannot guarantee that we don't get a race condition here in the future.
            // If player 2's cards are not being drawn correctly, the first place to look would be here; check if everything is executed in the right order.
            // NetworkObjectReference[] handToSend = new NetworkObjectReference[_hand.Count];
            // for (int i = 0; i < handToSend.Length; i++)
            //     handToSend[i] = _hand[i].NetworkObject;
            //Debug.Log(string.Format("PLAYER {0} INITIALIZING SORTING WITH A HAND OF SIZE {1}", playerId, handToSend.Length));
            // CardManager.Instance.SortHandRpc(handToSend, playerId);

            // From tim:
            card.Set_WorldPosition_Rpc(_handSlotTransforms[handSlot].position);
        }
    }

    //
    // Mana System
    //

    public int _mana = 3;

    public void UseMana(int amount)
    {
        _mana -= amount;
        UIManager.Instance.UpdateManaText(_mana);
        if (_mana < 0)
            Debug.Log("Error (Player.UseMana): Used non-existant mana, confirm wether mana useage was properly checked!");
    }

    //
    // Player Damage & Health System
    //

    public int _health { get; private set; } = 10;

    public void TakeDamage(int amount)
    {
        _health = math.max(0, _health - amount);
        UIManager.Instance.UpdateCommanderHealthText(_health);
        if (_health == 0)
            Die();
    }

    private void Die()
    {
        // TODO
    }

    //
    // Player Rotate System:
    //

    public bool _canRotateCard { get; private set; } = false;

    public void UseRotation()
    {
        if (_canRotateCard == false)
            Debug.Log("Error (Player.UseRotation): Used non-existant rotation, confirm wether rotation useage was properly checked!");
        _canRotateCard = false;
    }

    //
    // State Changes For The Ref:
    //

    public async Task BeginTurn()
    {
        _canRotateCard = true;
        _mana = 3;
        UIManager.Instance.UpdateManaText(_mana);
        await playerStateManager.ToState(PlayerState.Playing, PlayerCameraState.ViewingHand, PlayerRequestState.None);
    }

    public async Task BeginView()
    {
        _mana = 3;
        UIManager.Instance.UpdateManaText(_mana);
        await playerStateManager.ToState(PlayerState.Viewing, PlayerCameraState.ViewingBoard, PlayerRequestState.None);
    }

    public async Task WatchGame()
    {
        await playerStateManager.ToState(PlayerState.WatchingGame, PlayerCameraState.ViewingBoard, PlayerRequestState.None);
    }

    public async Task EndTurn()
    {
        DrawCards();
        // Immediately followed by a call to either BeginView or WatchGame in Referee, so no need to change state here
    }
}
