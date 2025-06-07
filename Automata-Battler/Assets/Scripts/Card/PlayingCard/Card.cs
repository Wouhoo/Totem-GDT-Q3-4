using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Rendering;
using Unity.Mathematics.Geometry;
using Unity.Mathematics;
using UnityEngine.UIElements;
using System.Threading.Tasks;

using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.Netcode;

[DisallowMultipleComponent]
public class Card : AbstractCard, IAction, ISelectable
{
    // Card references
    private CardRenderer cardRenderer;

    // Prefab Stuff
    [Header("Card Properties")]
    public HexCoordinates _position { get; private set; }

    [SerializeField] private int health = 1;
    [SerializeField] private int damage = 1;
    public int _initiative { get; private set; }
    public bool _inPlay { get; private set; } = false; // true = on the board, false = in hand.
    [SerializeField] private List<CardInstruction> instructions = new List<CardInstruction>();

    // Getting functions
    public int _health => health;
    public int _damage => damage;
    public List<CardInstruction> _instructions => instructions;

    void Awake()
    {
        cardRenderer = GetComponent<CardRenderer>();
    }

    void Start()
    {
        cardRenderer.Render_UpdateText();
    }

    // 
    // Setting Functions
    //

    [Rpc(SendTo.ClientsAndHost)] // Make sure initative is updated for both players
    public void SetInitiativeRpc(int amount)
    {
        _initiative = amount;
        cardRenderer.Render_UpdateText();
    }

    public void Set_Position(HexCoordinates pos)
    {
        _position = pos;
    }

    [Rpc(SendTo.ClientsAndHost)] // Update health for both players
    public void SetHealthRpc(int amount)
    {
        health = amount;
        cardRenderer.Render_UpdateText();
    }

    //
    // HOVER Functions
    //

    public new void OnHoverEnter()
    {
        // glow
        var OutlineController = GetComponent<OutlineController>();
        if (OutlineController != null)
            OutlineController.SetOutline(true);

        cardRenderer.RenderUI(1);
    }

    public new void OnHoverExit()
    {
        // stop glowing
        var OutlineController = GetComponent<OutlineController>();
        if (OutlineController != null)
            OutlineController.SetOutline(false);

        cardRenderer.RenderUI(0);
    }

    //
    // INSTRUCTIONS
    // 

    public async Task ExecuteInstructions()
    {
        foreach (CardInstruction instruction in instructions)
            await instruction.Execute(this);
    }

    //
    // Placement Action & Rotation Action
    //


    public bool Q_RotationArrowsShown = false;
    [SerializeField] public RotationArrow ra_Clock;
    [SerializeField] public RotationArrow ra_Counter;

    public bool Q_CanBeginAction()
    {
        // Action: Place the card
        if (_inPlay == false)
        {
            if (_cost > Player.Instance._mana)
            {
                Debug.Log("Not enough mana!");
                UIManager.Instance.PlayNotEnoughManaEffect();
                return false;
            }
            return true;
        }
        // Action: Rotate the card
        else
        {
            if (Player.Instance._canRotateCard)
            {
                cardRenderer.RenderArrows(1);
                Q_RotationArrowsShown = true;
                return true;
            }
            return false;
        }
    }

    public PlayerCameraState Get_ActionCamera()
    {
        return PlayerCameraState.ViewingBoard;
    }
    public PlayerRequestState Get_ActionInput()
    {
        // Action: Place the card
        if (_inPlay == false)
            return PlayerRequestState.Tiles_ValidEmpty;
        // Action: Rotate the card
        else
            return PlayerRequestState.RotationArrows;
    }

    // Place a card from hand onto the board 
    // For now this will be done only with client-side validation, since this is much easier
    // (it does allow cheating if you can manipulate your local game's memory, but eh)
    public async Task<PlayerCameraState> Act(ISelectable selectable)
    {
        // Action: Place the card
        if (_inPlay == false)
        {
            if (selectable is HexCell tile && Player.Instance._hand.Contains(this) && tile.GetCard() == null) // Card in player's hand & tile free
            {
                Player.Instance.UseMana(_cost);
                Player.Instance.RemoveCardFromHand(this); // Player is not a networkobject, so _hand is just a *local* list of references which we can add to/remove from as normal.
                                                          // We now play our card
                _position = tile.coordinates;
                PlayCardRpc(_position);                   // Make server move the card to the correct position
                Board.Instance.Set_TileOccupant(_position, this);  // Update board state for all players
                Referee.Instance.AddCardRpc(this);                 // Add card to server's Referee (Card instance is implicitly converted to a NetworkBehaviourReference)
                                                                   // (Since PlayCardRpc runs on server anyway, we could also call it as a non-rpc function from PlayCardRpc)
                Debug.Log($"Action Completed: Placing Card {this} in {tile.coordinates}");
                return PlayerCameraState.ViewingBoard; // Sucsess!
            }
            // Else we failed some how:
            return PlayerCameraState.ViewingHand; // Failure
        }
        // Action: Rotate the card
        else // _inPlay == true
        {
            Debug.Log(1);
            if (selectable is RotationArrow arrow)
            {
                Debug.Log(2);
                Player.Instance.UseRotation();
                if (arrow.clockwise) RotateInstructionsRpc(5);
                else if (!arrow.clockwise) RotateInstructionsRpc(1);
                cardRenderer.RenderArrows(0);
                Q_RotationArrowsShown = false;
                return PlayerCameraState.ViewingBoard; // Sucsess!
            }
            cardRenderer.RenderArrows(0);
            Q_RotationArrowsShown = false;
            return PlayerCameraState.ViewingBoard; // Failure
        }
    }

    [Rpc(SendTo.Server)] // Make the server play the card (the client doesn't have the authority to do this)
    private void PlayCardRpc(HexCoordinates tileCoords)
    {
        _position = tileCoords;
        CardAnimator.Lerp_JumpTo(transform, HexCoordinates.ToWorldPosition(tileCoords), 0.2f); // Can't await; see if this causes problems
        _inPlay = true;
    }

    // Special (rotation and flippin out yo) instruction:

    [Rpc(SendTo.ClientsAndHost)] // Update the instructions on both host and client
    public void RotateInstructionsRpc(int byAmount = 0)
    {
        for (int i = 0; i < instructions.Count; i++)
        {
            CardInstruction instruction = instructions[i];
            instruction.Rotate(byAmount);
            instructions[i] = instruction;
        }
        cardRenderer.Render_UpdateText();
    }
}