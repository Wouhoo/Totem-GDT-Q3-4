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
        hexGrid = FindFirstObjectByType<HexGrid>();
        board = FindFirstObjectByType<Board>();
        // how did wouter do this? wasnt there a better way?
    }

    // 
    // SET (the only external thing that can really happen other then executing instructions)
    //

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
        Vector3Int target = _position;  // + byAmount * hexGrid.Direction_to_hexPos(direction)

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
        Vector3Int targetDirection = new Vector3Int(0, 0, 0);//hexGrid.Direction_to_hexPos(direction)

        int amountMoved = 0;
        for (int i = 0; i < byAmount; i++)
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
        Vector3Int target = _position;  // + byAmount * hexGrid.Direction_to_hexPos(direction)

        if (board.TileExistance(target) && board.TileOccupant(target) != null) // ask if attack is possible
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
        Vector3Int targetDirection = new Vector3Int(0, 0, 0);//hexGrid.Direction_to_hexPos(direction)

        bool attackSuccess = false;
        int amountMoved = 0;
        for (int i = 1; i <= byAmount; i++)
        {
            amountMoved++;
            if (!board.TileExistance(_position + amountMoved * targetDirection))
                break;
            if (board.TileOccupant(_position + amountMoved * targetDirection) != null)
            {
                attackSuccess = true;
                break;
            }
        }

        if (attackSuccess)
        {
            Vector3Int target = _position + amountMoved * targetDirection;
            // animate
            board.TileOccupant(target).RecieveDamage(damageAmount);
            return;
        }

        // Failed to attack
        // animate
        return;
    }

    //
    // Visuals
    // 

    void Visuals_Movement(Vector3 toPos)
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
        // transform.position = board.HexToGridCoordinates(_position);
        // TODO : make with proper animations
    }

    void Visual_Placement()
    {

    }
}