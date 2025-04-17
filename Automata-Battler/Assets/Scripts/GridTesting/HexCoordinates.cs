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

    // [SerializeField] private int x, y; //makes coordinate show in editor
	[SerializeField] private int x, z; //makes coordinate show in editor

	public int X {
		get {
			return x;
		}
	}

	public int Y {
		get {
			return -X-Z;
			// return y;
		}
	}

    public int Z { //is derived from x and y
		get {
			return z;
			return -X - Y;
		}
	}

	public HexCoordinates (int x, int z) {
		this.x = x;
		// this.y = y;
		this.z = z;
	}

	// //this is from SQUARE GRID coordinates, look at the arguments!
    // public static HexCoordinates FromOffsetCoordinates (int col, int row) {
	// 	int y = (-col-row) - (col - (col & 1)) / 2;
    // 	return new HexCoordinates(col, y); //modified from CatLikeCoding as we have flat tops, which SUCK to code for no one makes a tutorial for these ~Lars
	// }


	//PUBLIC CONVENIENCE FUNCTIONS

	//Gets the Hex Coordinate a given world position would point to (does not take y value into account)
	//My god this transformation Sucked -Lars ft. ChatGPT
	public static HexCoordinates FromWorldPosition (Vector3 position) 
	{	
		// Invert the z since our ToWorldPosition() returns new Vector3(worldX, 0, -worldZ)
		float worldX = position.x;
		float worldZ = -position.z;

		// Convert world coordinates into axial fractions.
		float q = worldX / (HexMetrics.outerRadius * 1.5f);
		float r = worldZ / (HexMetrics.innerRadius * 2f) - q / 2f;

		// Round to the nearest integers.
		int iX = Mathf.RoundToInt(q);
		int iZ = Mathf.RoundToInt(r);

		// Create HexCoordinates. (Assume your HexCoordinates constructor takes (int q, int r)
		// and calculates cube y as -q - r.)
		return new HexCoordinates(iX, iZ);

		// //just the inverse of the grid building function lol
		// float x = position.x / (HexMetrics.outerRadius * 1.5f);
		// float z = position.z / (HexMetrics.innerRadius * 2.0f) - Mathf.RoundToInt(x)*0.5f + Mathf.RoundToInt(x)/2;
    
		// //Round to the nearest axial integer coordinates
		// int iQ = Mathf.RoundToInt(x);
		// int iY = Mathf.RoundToInt(z);
    
    	// return FromOffsetCoordinates(iQ, iY); //yeah yeah not good practice but this took 2 hours no joke
	}

	//Epic conversion PURE and WITHOUT grid conversion
	//Brexit means Brexit
	public static Vector3 ToWorldPosition (HexCoordinates hex) 
	{	
		float worldX = HexMetrics.outerRadius * 1.5f * hex.X;
    	float worldZ = HexMetrics.innerRadius * 2f * (hex.Z + hex.X / 2f);
		
		return new Vector3(worldX, 0f, worldZ);

	}


	//define addition between HexCoordinates
	public static HexCoordinates operator +(HexCoordinates a, HexCoordinates b) 
	{
        return new HexCoordinates(a.X + b.X, a.Y + b.Y);
    }
	//multiplication, should only be used for relative coords!
	public static HexCoordinates operator *(HexCoordinates a, HexCoordinates b) 
	{
        return new HexCoordinates(a.X * b.X, a.Y * b.Y);
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