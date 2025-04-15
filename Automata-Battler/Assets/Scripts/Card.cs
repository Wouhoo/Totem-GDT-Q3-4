using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Rendering;
using Unity.Mathematics.Geometry;
using Unity.Mathematics;
using UnityEngine.UIElements;

public class Card : MonoBehaviour
{
    private HexGrid hexGrid; // there is only 1
    private Board board; // there is only 1
    private Vector3Int _position;


    // Prefab Stuff
    [SerializeField] public string _name;
    [SerializeField] private int _health = 1;


    void Start()
    {
        board = FindFirstObjectByType<Board>();
    }

    // 
    // GET & SET
    //

    public void Set_Position(Vector3Int pos)
    {
        _position = pos;
        UpdateVisuals_Temp();

    }

    public void RecieveDamage(int amount)
    {
        _health = math.max(0, _health - amount);
        if (_health == 0)
            Die();
    }

    // 
    // PRE PLAY (IN HAND)
    //





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
    // INSTRUCTION FUNCTIONS
    //

    private void Die()
    {
        // remove position on board
        // destroy this game object
    }

    private void Move_asJump(HexDirection direction, int byAmount)
    {
        Vector3Int target = _position + byAmount *;//hexGrid.Direction_to_hexPos(direction)

        if (board.TileExistance(target) && board.TileOccupant(target) == null) // ask if move is possible
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
        Vector3Int targetDirection = ;//hexGrid.Direction_to_hexPos(direction)

        int amountMoved = 0;
        for (...)
        {
            if (!board.TileExistance(_position + (amountMoved + 1) * targetDirection))
                break;
            if (board.TileOccupant(_position + (amountMoved + 1) * targetDirection) != null)
                break;
            amountMoved++;
        }

        if (amountMoved != 0)
        {
            board.Set_TileOccupant(_position, null);
            _position += (amountMoved + 1) * targetDirection;
            board.Set_TileOccupant(_position, this);
            // animate
            return;
        }

        // Failed to move
        // animate
        return;
    }

    private void Attack_asDirect(HexDirection direction, int damageAmount)
    {
        // Ask if that cell is free / exists

        // if yes
        // animate

        // if no
        // ask who occupies it
        // attack
        // animate
    }
    private void Attack_asJump(HexDirection direction, int byAmount, int damageAmount)
    {
        Vector3Int target = _position + byAmount *;//hexGrid.Direction_to_hexPos(direction)

        if (board.TileExistance(target) && board.TileOccupant(target) == null) // ask if move is possible
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
    private void Attack_asSlide(HexDirection direction, int damageAmount)
    {

    }

    //
    // Visuals
    // 

    void Visuals_Movement()
    {

    }

    void Visuals_Attack()
    {

    }

    void Visuals_Die()
    {

    }

    void UpdateVisuals_Temp()
    {
        transform.position = board.HexToGridCoordinates(_position);
        // TODO : make with proper animations
    }

    void Visual_Placement()
    {

    }
}