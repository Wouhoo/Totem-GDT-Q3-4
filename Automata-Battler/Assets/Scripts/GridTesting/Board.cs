using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem; //fucking sucks
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class Board : MonoBehaviour
{
	[SerializeField] public Dictionary<HexCoordinates, HexCell> cells { get; private set; }

	public Text cellLabelPrefab;

	Canvas gridCanvas;

	public Color defaultColor = Color.white;
	public Color touchedColor = Color.magenta;


	//utility functions

	//gets the cell from the array from hex coordinate 
	public HexCell GetHexCellAtHexCoordinate(HexCoordinates coordinates)
	{
		if (cells.ContainsKey(coordinates)) return cells[coordinates];
		else
		{
			Debug.LogWarning("Requested coordinate not in the Board: " + coordinates + ", Null returned");
			return null;
		}
	}

	void Awake()
	{
		gridCanvas = GetComponentInChildren<Canvas>(); //I still have this rather than have it be on the cells. Could make it only show up with gizmos.

		cells = new Dictionary<HexCoordinates, HexCell>();

		//Debug.Log("GENERATING BOARD");
		GenerateBoard(); // For clarity: this means Board generates a dictionary with references to all board cells.
						 // *The cells themselves already exist*; they are not spawned here.

		// TEST: See if client also received a correct copy of the cells
		/* Debug.Log("STARTING BOARD PRINT");
        foreach (HexCell cell in cells.Values) 
        {
            Debug.Log(cell.coordinates);
        } */
	}

	void GenerateBoard()
	{
		// Find every HexCell in children (including grandchildren, as they are children of 'Cells' empty parent object)
		HexCell[] allCells = GetComponentsInChildren<HexCell>();

		// Loop through and register each one
		foreach (var cell in allCells)
		{
			// Use the cell’s own coordinates as the key
			if (cells.ContainsKey(cell.coordinates))  //duplicate check
			{
				Debug.LogError("Duplicate Hexcell at hex coordinate: " + cell.coordinates + ", Cell ignored ");
				continue;
			}
			cells[cell.coordinates] = cell;

			//IMPORTANT this means: Cell is not shown if it isn't in the grid
			//no longer required, cells have their own meshes from the start
			//cell.mesh.GenerateMesh();

			Text label = Instantiate<Text>(cellLabelPrefab);
			label.rectTransform.SetParent(gridCanvas.transform, false);
			label.rectTransform.anchoredPosition = new Vector2(cell.transform.position.x, cell.transform.position.z);
			label.text = cell.coordinates.ToStringOnSeparateLines();
		}
	}

	//TIM COMPATIBILITY STUFF
	public bool TileExistance(HexCoordinates pos)
	{
		// Checks if the tile is part of the board
		return cells.ContainsKey(pos);
	}

	public Card TileOccupant(HexCoordinates pos)
	{
		// Returns tile occupant (null if not occupied (or non existant))
		if (!TileExistance(pos))
		{
			Debug.Log("Error: tried obtaining occupant from a non existant tile");
			return null;
		}
		return cells[pos].GetCard();
	}

	public bool TileIsHostileCommander(ulong playerId, HexCoordinates pos)
	{
		if (!TileExistance(pos))
		{
			Debug.Log("Error: Non-existant tile");
			return false;
		}
		if (cells[pos].commander != 0 && cells[pos].commander != playerId)
			return true;
		return false;
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

		if (card != null && TileOccupant(pos) != null && !TileIsHostileCommander(card._ownerPlayer, pos)) // Adds a card to a tile 
		{
			Debug.Log("Error: tried adding occupant to an occupied tile");
			return;
		}

		cells[pos].SetCard(card); // This first sets the card on server so the server can do validation, then it is set on the client as well
	}

	public bool CanPlace(ulong playerId, HexCoordinates pos)
	{
		if (!TileExistance(pos))
			return false;
		return TileExistance(pos) && TileOccupant(pos) == null && !TileIsHostileCommander(playerId, pos);
	}

	public bool CanAttack(ulong playerId, HexCoordinates pos)
	{
		if (!TileExistance(pos)) return false;
		if (TileOccupant(pos) != null) return true;
		if (TileIsHostileCommander(playerId, pos)) return true;
		return false;
	}

	public async Task Attack(ulong playerId, HexCoordinates pos, int damageAmount)
	{
		if (!CanAttack(playerId, pos))
		{
			Debug.Log("Error: tried invalid attack");
			return;
		}
        if (TileIsHostileCommander(playerId, pos)) // If target is a commander tile, damage commander (even if there is a unit there)
            cells[pos].DamageCommander(damageAmount);
        else if (TileOccupant(pos) != null)
			await I_TakeDamage.Execute(TileOccupant(pos), damageAmount);
	}
}
