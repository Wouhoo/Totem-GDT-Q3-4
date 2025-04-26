using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Rendering;
using Unity.Mathematics.Geometry;
using Unity.Mathematics;
using UnityEngine.UIElements;
using UnityEditor.Scripting;
using System.Threading.Tasks;

using Mono.Cecil.Cil;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

[DisallowMultipleComponent]
public class Card : MonoBehaviour, ISelectable
{
    // Card references
    private Board board;
    private Referee referee;
    private CardRenderer cardRenderer;

    // Prefab Stuff
    [Header("Card Properties")]
    public HexCoordinates _position { get; private set; }
    public Player _ownerPlayer { get; private set; }
    [SerializeField] private int cost = 1;
    [SerializeField] private int health = 1;
    [SerializeField] private int damage = 1;
    public int _initiative { get; private set; }
    [SerializeField] private bool inPlay = false; // true = on the board, false = in hand.
    [SerializeField] private List<CardInstruction> instructions = new List<CardInstruction>();

    // Getting functions
    public int _cost => cost;
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

    public void Set_Owner(Player player)
    {
        _ownerPlayer = player;
    }

    public void Set_Initiative(int amount)
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
    // PLAYER INTERACTIONS
    //


    public void OnHoverEnter()
    {
        // glow
    }

    public void OnHoverExit()
    {
        // stop glowing
    }

    public void OnSelect()
    {
        if (inPlay)
        {

        }
        else // inHand
        {
            // heavy glow?
            // move to play?
        }
    }

    public async Task PlaceCard(HexCoordinates pos)
    {
        // should already be checked that placement is valid
        _position = pos;
        board.Set_TileOccupant(_position, this);
        //temp:
        await CardAnimator.Lerp_JumpTo(transform, HexCoordinates.ToWorldPosition(pos), 0.2f);
        inPlay = true;
    }

    //
    // INSTRUCTIONS
    // 

    public async Task ExecuteInstructions()
    {
        foreach (var instruction in instructions)
            await instruction.Execute(this);
    }
}