using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Rendering;
using Unity.Mathematics.Geometry;
using Unity.Mathematics;
using UnityEngine.UIElements;
using UnityEditor.Scripting;

public class Card : MonoBehaviour, Interactable
{
    private HexGrid hexGrid; // there is only 1
    private Board board; // there is only 1
    private Player _ownerPlayer; // one of two!
    private HexCoordinates _position;

    // Prefab Stuff
    [SerializeField] static string _name;
    [SerializeField] private int _health = 1;


    void Start()
    {
        hexGrid = FindFirstObjectByType<HexGrid>();
        board = FindFirstObjectByType<Board>();
        // how did wouter do this? wasnt there a better way?
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

    public void RecieveDamage(int amount)
    {
        _health = math.max(0, _health - amount);
        // animate

        if (_health == 0)
            Die();
    }

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
        _inPlay = true;

    }






    //
    // INSTRUCTIONS
    // 

    [SerializeField] private List<CardFunctionCall> instructions = new List<CardFunctionCall>();

    public void ExecuteInstructions()
    {
        foreach (var call in instructions)
        {
            switch (call.functionType)
            {
                case CardFunctionCall.InstructionType.Move:
                    Move_asJump(call.direction, 1);
                    break;
                case CardFunctionCall.InstructionType.Jump:
                    Move_asJump(call.direction, 2);
                    break;
                case CardFunctionCall.InstructionType.Slide:
                    Move_asSlide(call.direction, 99);
                    break;
                case CardFunctionCall.InstructionType.Attack:
                    Attack_asJump(call.direction, 1, call.damageAmount);
                    break;
                case CardFunctionCall.InstructionType.Arrow:
                    Attack_asJump(call.direction, 2, call.damageAmount);
                    break;
                case CardFunctionCall.InstructionType.Shoot:
                    Attack_asSlide(call.direction, 99, call.damageAmount);
                    break;
                case CardFunctionCall.InstructionType.Die:
                    Die();
                    break;
            }
        }
    }

    // 
    // INSTRUCTION FUNCTIONS (THE FIRST LINE OF EACH IS INCOMPLETE)
    //

    private void Die()
    {
        // Remove from board
        board.Set_TileOccupant(_position, null);
        // animate
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
            // animate
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
            // animate
            return;
        }

        // Failed to move
        // animate
        return;
    }

    private void Attack_asJump(HexDirection direction, int byAmount, int damageAmount)
    {
        HexCoordinates target = _position + byAmount * direction.GetRelativeCoordinates();

        if (board.CanAttack(target)) // ask if attack is possible
        {
            // animate
            board.TileOccupant(target).RecieveDamage(damageAmount);
            return;
        }

        // Failed to attack
        // animate
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
            // animate
            board.TileOccupant(target).RecieveDamage(damageAmount);
            return;
        }

        // Failed to attack
        // animate
        return;
    }



    //
    // CARD RENDERING
    //

    // card in deck
    // card in hand
    // card in play

    //
    // CARD ANIMATIONS
    // 

    void Animate_Move_asJump(Vector3 toPos)
    {
        // transform.position
    }

    void Visuals_Attack()
    {

    }

    void Visuals_Die()
    {

    }

    void UpdateVisuals_Temp()
    {
        // transform.position = board.HexToGridCoordinates(_position);
        // TODO : make with proper animations
    }

    void Visual_Placement()
    {

    }


}