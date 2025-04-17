using UnityEngine;

public class HexCell : MonoBehaviour
{
    [Header("Hex Coordinates")]
    public HexCoordinates coordinates; //grid coordinates of this cell

    public Color color = Color.white; //current color of the cell

    [Header("Cell properties")]
    private SpriteRenderer placeholderSprite;
    [SerializeField] public BoardCellMesh mesh; 
    //IMPORTANT FROM HERE yeah so this is a design decision... Do we use the 'hexcoordToCell' function or do we go via cell neighbors?
    //We are going for the functions so this is here only temporarily
    [SerializeField] HexCell[] neighbors;

    //turn off the sprite in play mode
        void Start()
        {
            placeholderSprite = GetComponentInChildren<SpriteRenderer>();
            placeholderSprite.enabled = false;
        }
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

    // FOR TIM:
    // private Card _card = null;
    // public Card Get_Card()
    // {
    //     return _card;
    // }
    // public void Set_Card(Card card)
    // {
    //     _card = card;
    // }
// }
