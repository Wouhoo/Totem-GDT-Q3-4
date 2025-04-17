using UnityEngine;
// #if UNITY_EDITOR
// using UnityEditor;
// #endif

public class HexCell : MonoBehaviour, Interactable
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

    void OnValidate()
    {
        // Only run this in the Editor (not in builds or play mode)
        // #if UNITY_EDITOR
        // // Record the position change for Unity’s Undo system
        // Undo.RecordObject(transform, "Snap Cell to Hex Coordinate");

        // // Compute the world position from the hex coordinate
        // Vector3 worldPos = HexCoordinates.ToWorldPosition(coordinates);

        // If you’re using localPosition (i.e. parented under a grid), use localPosition.
        // Otherwise, use position.
        transform.localPosition = HexCoordinates.ToWorldPosition(coordinates);;

        // Mark the scene as dirty so Unity knows to save the change
        // EditorUtility.SetDirty(transform);
        // #endif
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

    // FOR TIM:
    //BOO THIS CAUSED THE FIRST MERGE CONFLICT
    private Card _card = null;
    public Card Get_Card()
    {
        return _card;
    }
    public void Set_Card(Card card)
    {
        _card = card;
    }

    public void OnSelect() { }
    public void OnDeselect() { }
    public void OnHover() { }
    public void OnDehover() { }

}
