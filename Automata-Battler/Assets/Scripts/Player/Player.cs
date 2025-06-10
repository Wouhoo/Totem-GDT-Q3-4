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
            Vector3 additionalRot = new Vector3(0, 0, 0);
            if (playerId == 2) additionalRot = new Vector3(0, 180, 0);
            Quaternion slotRot = Quaternion.Euler(_handSlotTransforms[handSlot].rotation.eulerAngles + additionalRot);
            card.DrawCard_Placement_Rpc(_handSlotTransforms[handSlot].position, slotRot);
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

    public int _health { get; private set; } = 10; // To test game end screen, set to 1

    public void TakeDamage(int amount)
    {
        _health = math.max(0, _health - amount);
        SFXPlayer.Instance.PlaySoundEffect(SFXPlayer.SoundEffect.Damage);
        UIManager.Instance.UpdateCommanderHealthText(_health);
        if (_health == 0)
            Referee.Instance.TriggerGameEndRpc(3 - this.playerId);
    }

    //
    // Player Rotate System:
    //

    public bool _canRotateCard { get; private set; } = false;

    public void UseRotation()
    {
        if (_canRotateCard == false)
            Debug.Log("Error (Player.UseRotation): Used non-existant rotation, confirm wether rotation useage was properly checked!");
        SFXPlayer.Instance.PlaySoundEffect(SFXPlayer.SoundEffect.CardRotate);
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
}
