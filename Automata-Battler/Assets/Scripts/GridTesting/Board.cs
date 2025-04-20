using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem; //fucking sucks
using System;
using System.Collections.Generic;

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
			cell.mesh.GenerateMesh();

			Text label = Instantiate<Text>(cellLabelPrefab);
			label.rectTransform.SetParent(gridCanvas.transform, false);
			label.rectTransform.anchoredPosition = new Vector2(cell.transform.position.x, cell.transform.position.z);
			label.text = cell.coordinates.ToStringOnSeparateLines();
		}
	}



	//just the 'touching' check. If turned on prevents card placement so that's why it's commented out
	// void Update()
	// {
	// 	if (Mouse.current.leftButton.isPressed)
	// 	{
	// 		HandleInput();
	// 	}
	// }

	//unused, see above
	void HandleInput()
	{
		Vector2 mousePosition = Mouse.current.position.ReadValue();
		Ray inputRay = Camera.main.ScreenPointToRay(mousePosition);
		RaycastHit hit;
		if (Physics.Raycast(inputRay, out hit))
		{
			TouchCell(hit.point);
		}
	}

	//unused, see above x2
	void TouchCell(Vector3 position)
	{
		position = transform.InverseTransformPoint(position);
		HexCoordinates coordinates = HexCoordinates.FromWorldPosition(position);
		Debug.Log("touched at " + coordinates.ToString());
		HexCell cell = GetHexCellAtHexCoordinate(coordinates);
		cell.color = touchedColor;

		Debug.Log("Cell world position should be: " + HexCoordinates.ToWorldPosition(coordinates));

		cell.color = Color.magenta;
		cell.mesh.currentColor = Color.magenta; //cool workaround shhhh
		cell.mesh.GenerateMesh();
	}


	/////////////////////////////////////////////////////////////TIM COMPATIBILITY STUFF
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
		return cells[pos].Get_Card();
	}

	// NOTE: in general cards should only add and remove themselves, if you want a card to move you tell the card, not the board!

	//maybe have these conditions be checked before calling, instead of being part of the function -Lars
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

		cells[pos].Set_Card(card);
	}

	public bool CanPlace(HexCoordinates pos)
	{
		return (TileExistance(pos) && TileOccupant(pos) == null);
	}

	public bool CanAttack(HexCoordinates pos)
	{
		return (TileExistance(pos) && TileOccupant(pos) != null);
	}
}
