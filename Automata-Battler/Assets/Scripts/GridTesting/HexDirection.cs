using System.Collections.Generic;

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

    public static HexDirection Opposite(this HexDirection direction)
    {
        return (int)direction < 3 ? (direction + 3) : (direction - 3);
    }

    //Getting relative coordinate from HexDirection
    private static readonly Dictionary<HexDirection, HexCoordinates> directionRelativeCoordinates = new Dictionary<HexDirection, HexCoordinates>
    {
        { HexDirection.N,  new HexCoordinates(0, -1) },    // (0, +1, -1)
        { HexDirection.NE, new HexCoordinates(1, -1) },    // (+1, 0, -1)
        { HexDirection.SE, new HexCoordinates(1, 0) },   // (+1, -1, 0)
        { HexDirection.S,  new HexCoordinates(0, 1) },   // (0, -1, +1)
        { HexDirection.SW, new HexCoordinates(-1, 1) },   // (-1, 0, +1)
        { HexDirection.NW, new HexCoordinates(-1, 0) }    // (-1, +1, 0)
    };

    public static HexCoordinates GetRelativeCoordinates(this HexDirection direction)
    {
        return directionRelativeCoordinates[direction];
    }

    public static HexDirection Rotate(this HexDirection direction, int byAmount)
    {
        return (HexDirection)(((int)direction + byAmount) % 6);
    }
}