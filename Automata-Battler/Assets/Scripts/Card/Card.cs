using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Rendering;
using Unity.Mathematics.Geometry;
using Unity.Mathematics;
using UnityEngine.UIElements;
using UnityEditor.Scripting;
using TMPro;
using Mono.Cecil.Cil;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

[DisallowMultipleComponent]
public class Card : MonoBehaviour, Interactable
{
    // Card references
    private Board board; // there is only 1
    private Player _ownerPlayer; // one of two!
    private CardAnimator cardAnimator;
    private HexCoordinates _position;
    [SerializeField] private TextMeshProUGUI textName;
    [SerializeField] private TextMeshProUGUI textHealth;
    [SerializeField] private TextMeshProUGUI textInstructions;


    // Prefab Stuff
    [Header("Card Properties")]
    [SerializeField] private int _health = 1;
    private int _damage = 1;

    // Rendering Stuff

    // Animation stuff


    void Awake()
    {
        textHealth.text = _health.ToString();
        Render_Instructions();
    }

    void Start()
    {
        board = FindFirstObjectByType<Board>();
        cardAnimator = GetComponent<CardAnimator>();
    }

    //
    // GET
    //

    private bool _inPlay = false; // true = on the board, false = in hand.
    [SerializeField] private int _cost = 1;

    public bool Get_InPlay()
    {
        return _inPlay;
    }

    public int Get_Cost()
    {
        return _cost;
    }

    // 
    // SET (the only external thing that can really happen other then executing instructions)
    //

    public void Set_Owner(Player player)
    {
        _ownerPlayer = player;
    }

    // 
    // PLAYER INTERACTIONS
    //


    public void OnHover()
    {

    }

    public void OnDehover()
    {

    }

    public void OnSelect()
    {

    }

    public void OnDeselect()
    {

    }

    public void PlaceCard(HexCoordinates pos)
    {
        // should already be checked that placement is valid
        _position = pos;
        //temp:
        cardAnimator.Move_asJump(pos);
        _inPlay = true;

    }






    //
    // INSTRUCTIONS
    // 

    [SerializeField] private List<CardInstruction> _instructions = new List<CardInstruction>();

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
        board.Set_TileOccupant(_position, null);
        // destroy this game object
        Destroy(this);
    }

    private void Move_asJump(HexDirection direction, int byAmount)
    {
        HexCoordinates target = _position + byAmount * direction.GetRelativeCoordinates();

        if (board.CanPlace(target)) // ask if move is possible
        {
            board.Set_TileOccupant(_position, null);
            _position = target;
            board.Set_TileOccupant(_position, this);
            cardAnimator.Move_asJump(_position);
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
            if (!board.CanPlace(_position + (amountMoved + 1) * targetDirection))
                break;
            amountMoved++;
        }

        if (amountMoved != 0)
        {
            board.Set_TileOccupant(_position, null);
            _position += amountMoved * targetDirection;
            board.Set_TileOccupant(_position, this);
            cardAnimator.Move_asSlide(_position);
            return;
        }

        // Failed to move
        return;
    }

    private void Attack_asJump(HexDirection direction, int byAmount, int damageAmount)
    {
        HexCoordinates target = _position + byAmount * direction.GetRelativeCoordinates();

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
            if (!board.CanPlace(_position + amountMoved * targetDirection))
                break;
            amountMoved++;
        }

        HexCoordinates target = _position + amountMoved * targetDirection;

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
        _health = math.max(0, _health - amount);

        if (_health == 0)
            Die();
    }

    //
    // CARD RENDERING
    //

    private void Render_Instructions()
    {
        textInstructions.text = string.Join(" ", _instructions.Select(instruction =>
        {
            return instruction.Visualization();
        }));
    }

}