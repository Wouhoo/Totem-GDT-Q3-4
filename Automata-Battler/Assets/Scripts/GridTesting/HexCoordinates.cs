using UnityEngine;

[System.Serializable]
public struct HexCoordinates 
{

	/*
	In case you were expecting some sort of documentation here it is I guess
	If Lars did his job right you shouldn't have to worry about this as all, 
	and only should have to understand wtf this means.
	
	The coordinate system is cube coordinates. Sadly, every single person online does this with hexagons with a pointy top
	instead of a 'flat' top (like we do).

	the system works with 3 'axis', x, y and z. the three coordinates always sum up to 0.
	note that these axis do NOT line up with the hexes, this is on purpose
	-> it means moving from cell A to neighbor B always modifies TWO coordinates

	- positive x is on the east
	- positive y is to the nort-west
	- positive z is on the south-west

	i.e. say you are at (0,0,0), moving to the cell to the top-right moves you to (1,0,-1) because you went in +x direction, but also in -z.

	~Lars, after following a tutorial meant for a different coordinate system 
	*/

    [SerializeField] private int x, y; //makes coordinate show in editor

	public int X {
		get {
			return x;
		}
	}

	public int Y {
		get {
			return y;
		}
	}

    public int Z { //is derived from x and y
		get {
			return -X - Y;
		}
	}

	public HexCoordinates (int x, int y) {
		this.x = x;
		this.y = y;
	}

	//this is from SQUARE GRID coordinates, look at the arguments!
    public static HexCoordinates FromOffsetCoordinates (int col, int row) {
		int y = row - (col - (col & 1)) / 2;
    	return new HexCoordinates(col, y); //modified from CatLikeCoding as we have flat tops, which SUCK to code for no one makes a tutorial for these ~Lars
	}

	//Gets the Hex Coordinate a given world position would point to (does not take y value into account)
	//My god this transformation Sucked -Lars ft. ChatGPT
	public static HexCoordinates FromPosition (Vector3 position) 
	{	
		//just the inverse of the grid building function lol
		float x = position.x / (HexMetrics.outerRadius * 1.5f);
		float z = position.z / (HexMetrics.innerRadius * 2.0f) - Mathf.RoundToInt(x)*0.5f + Mathf.RoundToInt(x)/2;
    
		//Round to the nearest axial integer coordinates
		int iQ = Mathf.RoundToInt(x);
		int iY = Mathf.RoundToInt(z);
    
    	return FromOffsetCoordinates(iQ, iY); //yeah yeah not good practice but this took 2 hours no joke
	}


    //convenience section

	//define addition between HexCoordinates
	public static HexCoordinates operator +(HexCoordinates a, HexCoordinates b) 
	{
        return new HexCoordinates(a.X + b.X, a.Y + b.Y);
    }
	
    public override string ToString () 
	{
		return "(" +
			X.ToString() + ", " + Y.ToString() + ", " + Z.ToString() + ")";
	}

	public string ToStringOnSeparateLines () 
	{
		return X.ToString() + "\n" + Y.ToString() + "\n" + Z.ToString();
	}
}