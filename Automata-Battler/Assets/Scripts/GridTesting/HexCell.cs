using UnityEngine;

public class HexCell : MonoBehaviour, ISelectable
{
    [Header("Hex Coordinates")]
    public HexCoordinates coordinates; //grid coordinates of this cell

    public Color color = Color.white; //current color of the cell, testing purposes

    [Header("Cell properties")]
    private SpriteRenderer placeholderSprite;
    [SerializeField] public BoardCellMesh mesh;

    //turn off the sprite in play mode, as the mesh will be generated
    void Start()
    {
        placeholderSprite = GetComponentInChildren<SpriteRenderer>();
        placeholderSprite.enabled = false;
    }

    //'Snap' the coordinate of a cell to it's hex coordinate
    // Only run this in the Editor (not in builds or play mode)
    void OnValidate()
    {
        // using localPosition because parented under Board
        // Otherwise, use position.
        transform.localPosition = HexCoordinates.ToWorldPosition(coordinates); ;
    }


    //Occupant stuff
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
    public void OnHoverEnter() { }
    public void OnHoverExit() { }

}
