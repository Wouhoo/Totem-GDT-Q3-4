using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem; //fucking sucks

public class HexGrid : MonoBehaviour {

	public int width = 5;
	public int height = 5;

	public HexCell cellPrefab;

    HexCell[] cells;

	public Text cellLabelPrefab;

	Canvas gridCanvas;

	HexMesh hexMesh;

	public Color defaultColor = Color.white;
	public Color touchedColor = Color.magenta;

	void Awake () {

		gridCanvas = GetComponentInChildren<Canvas>();

		gridCanvas = GetComponentInChildren<Canvas>();
		hexMesh = GetComponentInChildren<HexMesh>();
		//create grid of cells
		cells = new HexCell[height * width];

		for (int z = 0, i = 0; z < height; z++) {
			for (int x = 0; x < width; x++) {
				CreateCell(x, z, i++);
			}
		}
	}

	//Happens AFTER Awake, get the vertices to draw
	void Start () {
		hexMesh.Triangulate(cells);
	}
	
	void CreateCell (int x, int z, int i) 
	{
		Vector3 position;
		position.x = x * (HexMetrics.outerRadius * 1.5f);
		position.y = 0f;
		position.z = (z + x * 0.5f - x / 2) * (HexMetrics.innerRadius * 2.0f); //make sure it 'alternates'

		HexCell cell = cells[i] = Instantiate<HexCell>(cellPrefab);

		cell.transform.SetParent(transform, false);
		cell.transform.localPosition = position;
		cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z); //set grid coordinates
		cell.color = defaultColor; //set the default color

		//set the cell's neighbors PLEASE LET'S NOT DO THIS JUST USE A FUNC FOR RELATIVE COORDS
		// N/S
		if (z > 0) 
		{
			cell.SetNeighbor(HexDirection.S, cells[i - width]);
		}
		// NE/SW
		if(x > 0 && (x % 2 == 1  || z > 0))
		{
			if (x % 2 == 1)
			{
				cell.SetNeighbor(HexDirection.SW, cells[i - 1]);
			}
			else //then we need the previous row's entry
			{
				cell.SetNeighbor(HexDirection.SW, cells[i - 1 - width]);
			}
		}
		// NW/SE
		if ((x > 0 && (x % 2 == 0)) || (x < width && z > 0 && (x % 2 == 0)))
		{
			if (x > 0) //we are on a 'non-shifted' col, grab last entry
			{
				cell.SetNeighbor(HexDirection.NW, cells[i - 1]);
			}
			if(z>0) //z > 0, we are on the 'next' row, grab the offset one from last row
			{
				cell.SetNeighbor(HexDirection.SE, cells[i - width + 1]);
			}
		}

		//set the text, which is just the coordinate again
		Text label = Instantiate<Text>(cellLabelPrefab);
		label.rectTransform.SetParent(gridCanvas.transform, false);
		label.rectTransform.anchoredPosition =
			new Vector2(position.x, position.z);
		label.text = cell.coordinates.ToStringOnSeparateLines();
	}

	void Update () 
	{
		if (Mouse.current.leftButton.isPressed) {
			HandleInput();
		}
	}

	void HandleInput () 
	{
		Vector2 mousePosition = Mouse.current.position.ReadValue();
		Ray inputRay = Camera.main.ScreenPointToRay(mousePosition);
		//Debug.Log("Dafuq? at " + inputRay);
		RaycastHit hit;
		if (Physics.Raycast(inputRay, out hit)) {
			TouchCell(hit.point);
		}
	}
	
	void TouchCell (Vector3 position) 
	{
		position = transform.InverseTransformPoint(position);
		HexCoordinates coordinates = HexCoordinates.FromPosition(position);
		Debug.Log("touched at " + coordinates.ToString());

		//secret function to get array index from hex coordinate
		//TODO this fucking sucks imma just make a mapping for this later ~Lars
		int row = coordinates.Y +(coordinates.X - (coordinates.X & 1)) / 2;  //TODO: make this a func
		int col = coordinates.X; //ofc must be lower than 'width'
		int index = row * width + col;
		//int index = coordinates.X + coordinates.Y * width + coordinates.Y / 2; //Yeah yeah ofc it's fucking easy for the tutorial's coord system
		HexCell cell = cells[index];
		cell.color = touchedColor;
		hexMesh.Triangulate(cells);
	}
}
