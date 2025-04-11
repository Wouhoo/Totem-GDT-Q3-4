//the direction we can go from a given hex cell
//0 IS NORTH
public enum HexDirection 
{
	N, NE, SE, S, SW, NW
}

//TODO: think it's way easier to just convert from direction to relative coords but let's see ~Lars

//for the opposite() thingy
public static class HexDirectionExtensions 
{

	public static HexDirection Opposite (this HexDirection direction) 
    {
		return (int)direction < 3 ? (direction + 3) : (direction - 3);
	}
}