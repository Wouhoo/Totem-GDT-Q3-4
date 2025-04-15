using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    private Camera mainCamera;
    private Card selectedCard = null;
    private LayerMask cardLayerMask;
    private LayerMask tileLayerMask;


    // Reference to your input actions
    private PlayerInput playerInput;
    private InputAction clickAction;


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
        referee.Placement_Request(selectedCard, pos);

        // Reset selection
        selectedCard = null;
    }
}
