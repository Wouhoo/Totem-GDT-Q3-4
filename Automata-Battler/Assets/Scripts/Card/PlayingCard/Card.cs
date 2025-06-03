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
public class Card : AbstractCard, IAction
{
    // Card references
    private Board board;
    private Referee referee;
    private CardRenderer cardRenderer;

    // Prefab Stuff
    [Header("Card Properties")]
    public HexCoordinates _position { get; private set; }

    [SerializeField] private int health = 1;
    [SerializeField] private int damage = 1;
    public int _initiative { get; private set; }
    [SerializeField] private bool inPlay = false; // true = on the board, false = in hand.
    [SerializeField] private List<CardInstruction> instructions = new List<CardInstruction>();

    // Getting functions
    public int _health => health;
    public int _damage => damage;
    public bool _inPlay => inPlay;
    public List<CardInstruction> _instructions => instructions;

    void Awake()
    {
        board = FindFirstObjectByType<Board>();
        referee = FindFirstObjectByType<Referee>();
        cardRenderer = GetComponent<CardRenderer>();
    }

    void Start()
    {
        cardRenderer.Render_All();
    }

    // 
    // Setting Functions
    //

    [Rpc(SendTo.ClientsAndHost)] // Make sure initative is updated for both players
    public void SetInitiativeRpc(int amount)
    {
        _initiative = amount;
        cardRenderer.Render_Initiative();
    }

    public void Set_Position(HexCoordinates pos)
    {
        _position = pos;
    }

    public void Set_Health(int amount)
    {
        health = amount;
        cardRenderer.Render_Health();
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
    // Placement Action
    //

    public PlayerCameraState Get_ActionCamera()
    {
        return PlayerCameraState.ViewingBoard;
    }
    public PlayerRequestState Get_ActionInput()
    {
        return PlayerRequestState.Tiles_ValidEmpty;
    }

    // Place a card from hand onto the board 
    // For now this will be done only with client-side validation, since this is much easier
    // (it does allow cheating if you can manipulate your local game's memory, but eh)
    public async Task Act(ISelectable selectable)
    {
        if (selectable is HexCell tile)
        {
            if (!Player.Instance._hand.Contains(this)) // Card not in player's hand
                return; // false
            if (tile.GetCard() != null) // tile not free
                return; // false
            if (Player.Instance.AttemptManaUse(_cost))
            {
                Debug.Log("PLAYING CARD");
                Player.Instance._hand.Remove(this); // Player is not a networkobject, so _hand is just a *local* list of references which we can add to/remove from as normal.
                // We now play our card
                _position = tile.coordinates;
                PlayCardRpc(_position);                   // Make server move the card to the correct position
                board.Set_TileOccupant(_position, this);  // Update board state for all players
                referee.AddCardRpc(this);                 // Add card to server's Referee (Card instance is implicitly converted to a NetworkBehaviourReference)
                                                          // (Since PlayCardRpc runs on server anyway, we could also call it as a non-rpc function from PlayCardRpc)
                return; //true
            }
            else
            {
                Debug.Log("Not enough mana!"); // TODO: Show this to the player too somehow
            }
        }
        // else invalid input
    }

    [Rpc(SendTo.Server)] // Make the server play the card (the client doesn't have the authority to do this)
    private void PlayCardRpc(HexCoordinates tileCoords)
    {
        _position = tileCoords;
        CardAnimator.Lerp_JumpTo(transform, HexCoordinates.ToWorldPosition(tileCoords), 0.2f); // Can't await; see if this causes problems
        inPlay = true;
    }

    // Special (rotation and flippin out yo) instruction:

    [Rpc(SendTo.ClientsAndHost)] // Update the instructions on both host and client
    public void InvertInstructionsRpc()
    {
        // flip the list
        instructions.Reverse();
        // rotate each instruction
        RotateInstructions(3);
    }

    public void RotateInstructions(int byAmount = 0)
    {
        for (int i = 0; i < instructions.Count; i++)
        {
            CardInstruction instruction = instructions[i];
            instruction.Rotate(byAmount);
            instructions[i] = instruction;
        }
        cardRenderer.Render_Instructions();
    }
}