using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem; //fucking sucks
using System;
using System.Collections.Generic;

public class HexGrid : MonoBehaviour
{

	public int width = 5;
	public int height = 5;

	public HexCell cellPrefab;

	//Array holding all cellsArray in the grid
	public HexCell[] cellsArray;

	//to be replaced by this
	[SerializeField] public Dictionary<HexCoordinates, HexCell> cells { get; private set; }

	public Text cellLabelPrefab;

	Canvas gridCanvas;

	HexMesh hexMesh;

	public Color defaultColor = Color.white;
	public Color touchedColor = Color.magenta;


	//utility functions
	//TO BE REPLACED

	//gets the cell from the array from hex coordinate 
	public HexCell GetHexCellAtHexCoordinate(HexCoordinates coordinates)
	{
		if (cells.ContainsKey(coordinates)) return cells[coordinates];
		//Code to be removed below
		int row = coordinates.Y + (coordinates.X - (coordinates.X & 1)) / 2; //yeah it isnt that simple
		int col = coordinates.X;

		//bounds check
		if (row < 0 || row >= height || col < 0 || col >= width)
		{
			Debug.LogWarning("Requested coordinates out of HexGrid bounds: " + coordinates + ", Null returned");
			return null;
		}
		return cellsArray[row * width + col]; //just to get the editor to shut up for now
	}

	/*
	[SerializeField] private List<Vector3Int> cellPositions;
	*/

	void Awake()
	{

		gridCanvas = GetComponentInChildren<Canvas>();

		gridCanvas = GetComponentInChildren<Canvas>();
		hexMesh = GetComponentInChildren<HexMesh>();


		//NEW STUFF
		cells = new Dictionary<HexCoordinates, HexCell>();

		// Find every HexCell in our children (including grandchildren, etc.)
		HexCell[] allCells = GetComponentsInChildren<HexCell>();

        // Loop through and register each one
        foreach (var cell in allCells)
        {
            // Use the cell’s own coordinates as the key
			if (cells.ContainsKey(cell.coordinates))  //duplicate check
			{ 
				Debug.LogError("Duplicate Hexcell at hex coordinate: "+ cell.coordinates + ", Cell ignored ");
				continue;
			}
            cells[cell.coordinates] = cell;

			// cell.color = Color.magenta;
			// cell.mesh.currentColor = Color.magenta; //cool workaround shhhh

			cell.mesh.GenerateMesh();

			Text label = Instantiate<Text>(cellLabelPrefab);
			label.rectTransform.SetParent(gridCanvas.transform, false);
			label.rectTransform.anchoredPosition = new Vector2(cell.transform.position.x, cell.transform.position.z);
			label.text = cell.coordinates.ToStringOnSeparateLines();
        }

		//OLD STUFF
		//create grid of cellsArray
		cellsArray = new HexCell[height * width];

		// for (int z = 0, i = 0; z < height; z++)
		// {
		// 	for (int x = 0; x < width; x++)
		// 	{
		// 		CreateCell(x, z, i++);
		// 	}
		// }
	}


	//Happens AFTER Awake, get the vertices to draw
	void Start()
	{
		//hexMesh.Triangulate(cells);
	}

	void CreateCell(int x, int z, int i)
	{
		Vector3 position;
		position.x = x * (HexMetrics.outerRadius * 1.5f);
		position.y = 0f;
		position.z = (z + x * 0.5f - x / 2) * (HexMetrics.innerRadius * 2.0f); //make sure it 'alternates'

		HexCell cell = cellsArray[i] = Instantiate<HexCell>(cellPrefab);

		cell.transform.SetParent(transform, false);
		cell.transform.localPosition = position;
		cell.coordinates = HexCoordinates.FromWorldPosition(position);
		//cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z); //set grid coordinates
		cell.color = defaultColor; //set the default color

		//set the cell's neighbors PLEASE LET'S NOT DO THIS JUST USE A FUNC FOR RELATIVE COORDS
		// N/S
		// if (z > 0) 
		// {
		// 	cell.SetNeighbor(HexDirection.S, cellsArray[i - width]);
		// }
		// // NE/SW
		// if(x > 0 && (x % 2 == 1  || z > 0))
		// {
		// 	if (x % 2 == 1)
		// 	{
		// 		cell.SetNeighbor(HexDirection.SW, cellsArray[i - 1]);
		// 	}
		// 	else //then we need the previous row's entry
		// 	{
		// 		cell.SetNeighbor(HexDirection.SW, cellsArray[i - 1 - width]);
		// 	}
		// }
		// // NW/SE
		// if ((x > 0 && (x % 2 == 0)) || (x < width && z > 0 && (x % 2 == 0)))
		// {
		// 	if (x > 0) //we are on a 'non-shifted' col, grab last entry
		// 	{
		// 		cell.SetNeighbor(HexDirection.NW, cellsArray[i - 1]);
		// 	}
		// 	if(z>0) //z > 0, we are on the 'next' row, grab the offset one from last row
		// 	{
		// 		cell.SetNeighbor(HexDirection.SE, cellsArray[i - width + 1]);
		// 	}
		// }

		//set the text, which is just the coordinate again
		Text label = Instantiate<Text>(cellLabelPrefab);
		label.rectTransform.SetParent(gridCanvas.transform, false);
		label.rectTransform.anchoredPosition =
			new Vector2(position.x, position.z);
		label.text = cell.coordinates.ToStringOnSeparateLines();
	}

	//stuff that's only partially usable


	//@Tim NO TOUCH MY CODE
	// void Update()
	// {
	// 	if (Mouse.current.leftButton.isPressed)
	// 	{
	// 		HandleInput();
	// 	}
	// }

	void HandleInput()
	{
		Vector2 mousePosition = Mouse.current.position.ReadValue();
		Ray inputRay = Camera.main.ScreenPointToRay(mousePosition);
		//Debug.Log("Dafuq? at " + inputRay);
		RaycastHit hit;
		if (Physics.Raycast(inputRay, out hit))
		{
			TouchCell(hit.point);
		}
	}

	void TouchCell(Vector3 position)
	{
		position = transform.InverseTransformPoint(position);
		HexCoordinates coordinates = HexCoordinates.FromWorldPosition(position);
		Debug.Log("touched at " + coordinates.ToString());
		HexCell cell = GetHexCellAtHexCoordinate(coordinates);
		cell.color = touchedColor;

		Debug.Log("Cell world position should be: " + HexCoordinates.ToWorldPosition(coordinates));

		//for testing purposes
		foreach (HexDirection dir in Enum.GetValues(typeof(HexDirection)))
		{
			cell.SetNeighbor(dir, GetHexCellAtHexCoordinate(coordinates + dir.GetRelativeCoordinates()));
		}
		cell.color = Color.magenta;
		cell.mesh.currentColor = Color.magenta; //cool workaround shhhh
		cell.mesh.GenerateMesh();
		//hexMesh.Triangulate(cells);
	}


	/////////////////////////////////////////////////////////////TIM COMPATIBILITY STUFF TO BE CLEANED UP ETC ETC
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

	//shouldn't be necessary, I think can be handled elsewhere -Lars
	public bool CanPlace(HexCoordinates pos)
    {
        return (TileExistance(pos) && TileOccupant(pos) == null);
    }

    public bool CanAttack(HexCoordinates pos)
    {
        return (TileExistance(pos) && TileOccupant(pos) != null);
    }
}
