using UnityEngine;

public class HexCell : MonoBehaviour 
{
   public HexCoordinates coordinates; //grid coordinates of this cell

   public Color color; //current color of the cell

   //IMPORTANT FROM HERE yeah so this is a design decision... Do we use the 'hexcoordToCell' function or do we go via cell neighbors?
   //We are going for the functions so this is here only temporarily
   [SerializeField] HexCell[] neighbors;

   public HexCell GetNeighbor (HexDirection direction) 
   {
		return neighbors[(int)direction];
	}

   public void SetNeighbor (HexDirection direction, HexCell cell) 
   {
		neighbors[(int)direction] = cell;
      if (cell != null)
      {
         cell.neighbors[(int)direction.Opposite()] = this;
      }
	}
}
