using System.Data.Common;
using UnityEngine;
using UnityEngine.InputSystem;

// THIS ENTIRE CLASS NEEDS TO BE RE-WORKED!

public class Player : MonoBehaviour
{
    private Referee referee;
    private Camera mainCamera;

    private LayerMask cardLayerMask;
    private LayerMask tileLayerMask;


    // Reference to your input actions
    private PlayerInput playerInput;
    private InputAction clickAction;

    // Player attributes

    private int _mana = 5;

    public bool Try_UseMana(int amount)
    {
        // tries to use the mana amount, succsess is reported
        if (amount > _mana)
            return false;
        else
        {
            _mana -= amount;
            return true;
        }
    }

    //
    // INTERACTION SYSTEM
    //

    private Interactable _hovered;
    private Interactable _selected;

    void Update()
    {
        // get raycast on layer of interactables (cards + cells)
        // run HoverInteractable on result (even if null!)
    }

    // also add an on click which does a raycast and runs SelectInteractable

    private void HoverInteractable(Interactable interacted)
    {
        // if new object equals old object then pass
        if (interacted == _hovered)
            return;

        if (_hovered != null)
            _hovered.OnDehover();
        _hovered = interacted;
        if (_hovered != null)
            _hovered.OnHover();
    }



    private void SelectInteractable(Interactable interacted)
    {

        // null : else -> select else

        // card (board) : card (any, other) -> select other card
        // card (board) : else -> deselect

        // card (hand) : cell -> place card & deselect
        // card (hand) : card (hand, other) -> select other card
        // card (hand) : else -> deselect

        // tile : tile (other) -> select other tile
        // tile : else -> select else
        // (tiles should not really be selected ever tho...)

        if (_selected == null)
            SwapSelectionTo(interacted);

        else if (_selected is Card selectedCard)
        {
            if (selectedCard.Get_InPlay())
            {
                if (interacted is Card && _selected != interacted)
                    SwapSelectionTo(interacted);

                else SwapSelectionTo(null);
            }

            else // if (!selectedCard.Get_InPlay()) [ie the card is in hand]
            {
                if (interacted is HexCell hexCell && _mana >= selectedCard.Get_Cost()) // && hexCell.Occupant == null
                {
                    selectedCard.PlaceCard(hexCell.coordinates);
                    _mana -= selectedCard.Get_Cost();
                    SwapSelectionTo(null);
                }

                else if (interacted is Card interactedCard && !interactedCard.Get_InPlay() && _selected != interacted)
                {
                    SwapSelectionTo(interacted);
                }

                else SwapSelectionTo(null);
            }
        }

        else if (_selected is HexCell selectedHexCell)
        {
            if (_selected == interacted) SwapSelectionTo(null);

            else SwapSelectionTo(interacted);
        }
    }

    private void SwapSelectionTo(Interactable newSelected)
    {
        if (_selected != null)
            _selected.OnDeselect();
        _selected = newSelected;
        if (_selected != null)
            _selected.OnSelect();
    }
}

/*
private void Awake()
{
    mainCamera = Camera.main;
    referee = FindFirstObjectByType<Referee>();
    cardLayerMask = LayerMask.GetMask("Cards");
    tileLayerMask = LayerMask.GetMask("Tiles");

    // Set up new Input System
    playerInput = GetComponent<PlayerInput>();
    clickAction = playerInput.actions["Jump"];
}

private void OnEnable()
{
    clickAction.performed += OnClick;
}

private void OnDisable()
{
    clickAction.performed -= OnClick;
}

private void OnClick(InputAction.CallbackContext context)
{
    HandleSelection();
}

private void HandleSelection()
{
    Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

    if (selectedCard == null)
    {
        // Select a card
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, cardLayerMask))
        {
            Card card = hit.collider.GetComponent<Card>();
            if (card != null)
            {
                selectedCard = card;
                Debug.Log("Card Selected");
            }
        }
    }
    else
    {
        // Select a tile
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, tileLayerMask))
        {
            Tile tile = hit.collider.GetComponent<Tile>();
            if (tile != null)
            {
                UseCardOnTile(tile);
                Debug.Log("Placed");
            }
        }
        selectedCard = null; // deselect regardless
    }
}


private void UseCardOnTile(Tile tile)
{
    // Place the card
    Vector3Int pos = tile.Get_Position();

    // TODO

    // Reset selection
    selectedCard = null;
}
*/

