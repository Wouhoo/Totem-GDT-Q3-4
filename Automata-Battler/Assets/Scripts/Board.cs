using System;
using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor.UI;
using NUnit.Framework.Constraints;

public class Board : MonoBehaviour
{
    private HexGrid hexGrid;
    private Dictionary<HexCoordinates, HexCell> _tileMap;

    void Start()
    {
        hexGrid = FindFirstObjectByType<HexGrid>();
        // set board maps keys
        // foreach (Vector3Int tilePos in XXXXXX)
        //    _tileMap.Add(tilePos, null);

    }

    public bool TileExistance(HexCoordinates pos)
    {
        // Checks if the tile is part of the board
        if (!_tileMap.ContainsKey(pos))
            return false;
        return true;
    }

    public Card TileOccupant(HexCoordinates pos)
    {
        // Returns tile occupant (null if not occupied (or non existant))
        if (!TileExistance(pos))
        {
            Debug.Log("Error: tried obtaining occupant from a non existant tile");
            return null;
        }
        return _tileMap[pos].Get_Card();
    }

    // NOTE: in general cards should only add and remove themselves, if you want a card to move you tell the card, not the board!

    public void Set_TileOccupant(HexCoordinates pos, Card card = null)
    {
        if (!TileExistance(pos))
        {
            Debug.Log("Error: tried setting occupant of a non existant tile");
            return;
        }

        if (card == null && TileOccupant(pos) == null) // Removes card from a tile
        {
            Debug.Log("Error: tried removing occupant from an empty tile");
            return;
        }

        if (card != null && TileOccupant(pos) != null) // Adds a card to a tile 
        {
            Debug.Log("Error: tried adding occupant to an occupied tile");
            return;
        }

        _tileMap[pos].Set_Card(card);
    }

    public bool CanPlace(HexCoordinates pos)
    {
        if (TileExistance(pos) && TileOccupant(pos) == null)
            return true;
        return false;
    }

    public bool CanAttack(HexCoordinates pos)
    {
        if (TileExistance(pos) && TileOccupant(pos) != null)
            return true;
        return false;
    }
}
