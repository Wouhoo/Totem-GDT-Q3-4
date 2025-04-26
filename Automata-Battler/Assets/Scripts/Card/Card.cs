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

    private CardAnimator cardAnimator;
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
        cardAnimator = GetComponent<CardAnimator>();
        cardRenderer = GetComponent<CardRenderer>();
    }

    void Start()
    {
        cardRenderer.Render_All();
    }

    // 
    // SET (the only external thing that can really happen other then executing instructions)
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
        await cardAnimator.Move_asJump(pos);
        inPlay = true;
    }

    //
    // INSTRUCTIONS
    // 

    public async Task ExecuteInstructions()
    {
        foreach (var call in _instructions)
        {
            Debug.Log(call.instructionType);
            switch (call.instructionType)
            {
                case CardInstructionType.Move:
                    await Move_asJump(call.direction, 1);
                    break;
                case CardInstructionType.Jump:
                    await Move_asJump(call.direction, 2);
                    break;
                case CardInstructionType.Slide:
                    await Move_asSlide(call.direction, 99);
                    break;
                case CardInstructionType.Attack:
                    await Attack_asJump(call.direction, 1, _damage);
                    break;
                case CardInstructionType.Arrow:
                    await Attack_asJump(call.direction, 2, _damage);
                    break;
                case CardInstructionType.Shoot:
                    await Attack_asSlide(call.direction, 99, _damage);
                    break;
                case CardInstructionType.Die:
                    await Die();
                    break;
            }
        }
    }

    // 
    // INSTRUCTION FUNCTIONS
    //

    private async Task Die()
    {
        // Remove from board
        await cardAnimator.Die();
        board.Set_TileOccupant(_position, null);
        referee.RemoveCard(this);
        // destroy this game object
        Destroy(gameObject);
    }

    private async Task Move_asJump(HexDirection direction, int byAmount)
    {
        HexCoordinates target = _position + byAmount * direction.GetRelativeCoordinates();

        if (board.CanPlace(target)) // ask if move is possible
        {
            board.Set_TileOccupant(_position, null);
            _position = target;
            board.Set_TileOccupant(_position, this);
            await cardAnimator.Move_asJump(_position);
            return;
        }

        // Failed to move
        await cardAnimator.Move_asJump_FAIL(target);
        return;
    }

    private async Task Move_asSlide(HexDirection direction, int byAmount)
    {
        HexCoordinates targetDirection = direction.GetRelativeCoordinates();

        int amountMoved = 0;
        for (int i = 0; i < byAmount; i++)
        {
            if (!board.CanPlace(_position + (amountMoved + 1) * targetDirection))
                break;
            amountMoved++;
        }

        if (amountMoved != 0)
        {
            board.Set_TileOccupant(_position, null);
            _position += amountMoved * targetDirection;
            board.Set_TileOccupant(_position, this);
            await cardAnimator.Move_asSlide(_position);
            return;
        }

        // Failed to move
        return;
    }

    private async Task Attack_asJump(HexDirection direction, int byAmount, int damageAmount)
    {
        HexCoordinates target = _position + byAmount * direction.GetRelativeCoordinates();


        Debug.Log(target);
        Debug.Log(board.TileExistance(target));
        Debug.Log(board.TileOccupant(target));

        if (board.CanAttack(target)) // ask if attack is possible
        {

            await cardAnimator.Attack_asJump_1(target);
            await board.TileOccupant(target).TakeDamage(damageAmount);
            await cardAnimator.Attack_asJump_1(_position);
            return;
        }

        // Failed to attack
        await cardAnimator.Move_asJump_FAIL(_position);
        return;
    }

    private async Task Attack_asSlide(HexDirection direction, int byAmount, int damageAmount)
    {
        HexCoordinates targetDirection = direction.GetRelativeCoordinates();

        int amountMoved = 1;
        for (int i = 1; i <= byAmount; i++)
        {
            if (!board.CanPlace(_position + amountMoved * targetDirection))
                break;
            amountMoved++;
        }

        HexCoordinates target = _position + amountMoved * targetDirection;

        // successfull attack
        if (board.CanAttack(target))
        {
            await board.TileOccupant(target).TakeDamage(damageAmount);
            return;
        }

        // Failed to attack
        return;
    }

    public async Task TakeDamage(int amount) // Note this instruction is public!
    {
        health = math.max(0, health - amount);

        await cardAnimator.TakeDamage();
        cardRenderer.Render_Health();

        if (health == 0)
            await Die();
    }

}