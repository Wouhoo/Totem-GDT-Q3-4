using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Rendering;
using Unity.Mathematics.Geometry;
using Unity.Mathematics;
using UnityEngine.UIElements;

/// <summary>
/// 
/// Parent class to all cards
/// 
/// This class does not change its own state EVER; only other classes can do this via the "Set" functions.
/// 
/// </summary>

public class Card : MonoBehaviour
{
    // Parent Stuff
    private Referee referee;
    private Board board;
    private Vector3Int _position;


    // Prefab Stuff
    [SerializeField] public string _name;
    [SerializeField] public int _health;


    void Start()
    {
        board = FindFirstObjectByType<Board>();
        referee = FindFirstObjectByType<Referee>();
    }

    // 
    // GET & SET
    //

    public void Set_Position(Vector3Int pos)
    {
        _position = pos;
        UpdateVisuals_Temp();

    }

    public Vector3Int Get_Position()
    {
        return _position;
    }

    public void RecieveDamage(int amount)
    {
        _health = math.max(0, _health - amount);
        if (_health == 0)
            Die();
    }


    // 
    // INSTRUCTION FUNCTIONS
    //

    private void Die()
    {
        referee.Die_Request(this);
    }

    private void Attack(Vector3Int toPos, bool globalPos, bool asSlide, int byAmount)
    {
        if (!globalPos)
        {
            // Change to global coordinates
            toPos.x += _position.x;
            toPos.y += _position.y;
            toPos.z ^= _position.z;  // This is a bit flip; we in essnse have 2 gird boards stacked ontop of eachother
        }

        if (asSlide)
            return; //TODO

        else // if as jump
            referee.Attack_Jump_Request(this, toPos, byAmount);
    }

    private void Move(Vector3Int toPos, bool globalPos, bool asSlide)
    {
        if (!globalPos)
        {
            // Change to global coordinates
            toPos.x += _position.x;
            toPos.y += _position.y;
            toPos.z ^= _position.z;  // This is a bit flip; we in essnse have 2 gird boards stacked ontop of eachother
        }

        if (asSlide)
            return; //TODO

        else // if as jump
            referee.Move_Jump_Request(this, toPos);
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
                case CardFunctionCall.InstructionType.Move_Jump:
                    Move(call.position, call.globalPositioning, false);
                    break;
                case CardFunctionCall.InstructionType.Move_Slide:
                    Move(call.position, call.globalPositioning, true);
                    break;
                case CardFunctionCall.InstructionType.Attack_Jump:
                    Attack(call.position, call.globalPositioning, false, call.damageAmount);
                    break;
                case CardFunctionCall.InstructionType.Attack_Slide:
                    Attack(call.position, call.globalPositioning, true, call.damageAmount);
                    break;
            }
        }
    }

    //
    // VISUALS
    // 

    void UpdateVisuals_Temp()
    {
        transform.position = board.HexToGridCoordinates(_position);
        // TODO : make with proper animations
    }
}