using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Rendering;
using Unity.Mathematics.Geometry;
using Unity.Mathematics;
using UnityEngine.UIElements;
using UnityEditor.Scripting;

using Mono.Cecil.Cil;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

[DisallowMultipleComponent]
public class Card : MonoBehaviour, ISelectable
{
    // Card references
    private Board board; // there is only 1

    private CardAnimator cardAnimator;
    private CardRenderer cardRenderer;

    // Prefab Stuff
    [Header("Card Properties")]
    private HexCoordinates position;
    private Player ownerPlayer;
    [SerializeField] private int cost = 1;
    [SerializeField] private int health = 1;
    [SerializeField] private int damage = 1;
    [SerializeField] private int initiative;
    [SerializeField] private bool inPlay = false; // true = on the board, false = in hand.
    [SerializeField] private List<CardInstruction> instructions = new List<CardInstruction>();

    // Getting functions
    public HexCoordinates _position => position;
    public Player _ownerPlayer => ownerPlayer;
    public int _cost => cost;
    public int _health => health;
    public int _damage => damage;
    public int _initiative => initiative;
    public bool _inPlay => inPlay;
    public List<CardInstruction> _instructions => instructions;

    void Awake()
    {
        board = FindFirstObjectByType<Board>();
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
        ownerPlayer = player;
    }

    public void Set_Initiative(int amount)
    {
        initiative = amount;
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

    public void PlaceCard(HexCoordinates pos)
    {
        // should already be checked that placement is valid
        position = pos;
        //temp:
        cardAnimator.Move_asJump(pos);
        inPlay = true;

    }

    //
    // INSTRUCTIONS
    // 

    public void ExecuteInstructions()
    {
        foreach (var call in _instructions)
        {
            switch (call.instructionType)
            {
                case CardInstructionType.Move:
                    Move_asJump(call.direction, 1);
                    break;
                case CardInstructionType.Jump:
                    Move_asJump(call.direction, 2);
                    break;
                case CardInstructionType.Slide:
                    Move_asSlide(call.direction, 99);
                    break;
                case CardInstructionType.Attack:
                    Attack_asJump(call.direction, 1, _damage);
                    break;
                case CardInstructionType.Arrow:
                    Attack_asJump(call.direction, 2, _damage);
                    break;
                case CardInstructionType.Shoot:
                    Attack_asSlide(call.direction, 99, _damage);
                    break;
                case CardInstructionType.Die:
                    Die();
                    break;
            }
        }
    }

    // 
    // INSTRUCTION FUNCTIONS
    //

    private void Die()
    {
        // Remove from board
        board.Set_TileOccupant(position, null);
        // destroy this game object
        Destroy(gameObject);
    }

    private void Move_asJump(HexDirection direction, int byAmount)
    {
        HexCoordinates target = position + byAmount * direction.GetRelativeCoordinates();

        if (board.CanPlace(target)) // ask if move is possible
        {
            board.Set_TileOccupant(position, null);
            position = target;
            board.Set_TileOccupant(position, this);
            cardAnimator.Move_asJump(position);
            return;
        }

        // Failed to move
        // animate
        return;
    }

    private void Move_asSlide(HexDirection direction, int byAmount)
    {
        HexCoordinates targetDirection = direction.GetRelativeCoordinates();

        int amountMoved = 0;
        for (int i = 0; i < byAmount; i++)
        {
            if (!board.CanPlace(position + (amountMoved + 1) * targetDirection))
                break;
            amountMoved++;
        }

        if (amountMoved != 0)
        {
            board.Set_TileOccupant(position, null);
            position += amountMoved * targetDirection;
            board.Set_TileOccupant(position, this);
            cardAnimator.Move_asSlide(position);
            return;
        }

        // Failed to move
        return;
    }

    private void Attack_asJump(HexDirection direction, int byAmount, int damageAmount)
    {
        HexCoordinates target = position + byAmount * direction.GetRelativeCoordinates();

        if (board.CanAttack(target)) // ask if attack is possible
        {
            board.TileOccupant(target).TakeDamage(damageAmount);
            return;
        }

        // Failed to attack
        return;
    }

    private void Attack_asSlide(HexDirection direction, int byAmount, int damageAmount)
    {
        HexCoordinates targetDirection = direction.GetRelativeCoordinates();

        int amountMoved = 1;
        for (int i = 1; i <= byAmount; i++)
        {
            if (!board.CanPlace(position + amountMoved * targetDirection))
                break;
            amountMoved++;
        }

        HexCoordinates target = position + amountMoved * targetDirection;

        // successfull attack
        if (board.CanAttack(target))
        {
            board.TileOccupant(target).TakeDamage(damageAmount);
            return;
        }

        // Failed to attack
        return;
    }

    public void TakeDamage(int amount) // Note this instruction is public!
    {
        health = math.max(0, health - amount);

        cardAnimator.TakeDamage();
        cardRenderer.Render_Health();

        if (health == 0)
            Die();
    }

}