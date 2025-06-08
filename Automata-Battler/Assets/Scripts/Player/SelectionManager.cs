using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using NUnit.Framework.Constraints;

[RequireComponent(typeof(Player))]
[RequireComponent(typeof(PlayerStateManager))]
[DisallowMultipleComponent]
public class SelectionManager : MonoBehaviour
{
    //private Player player; // Player is now a singleton, so this is no longer necessary
    public static SelectionManager Instance { get; private set; }

    private PlayerStateManager playerStateManager;

    [SerializeField] private LayerMask cardLayerMask;
    [SerializeField] private LayerMask tileLayerMask;
    [SerializeField] private LayerMask buttonsLayerMask;
    [SerializeField] private LayerMask rotationarrowsLayerMask;
    private LayerMask selectablesLayerMask;

    [SerializeField] private Button toBoard_Button;
    [SerializeField] private Button toHand_Button;
    [SerializeField] private Button play_Button;

    private ISelectable currentHover;
    private IAction currentAction;

    public bool inputAllowed; // bool so the UI can disable inputs

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        playerStateManager = GetComponent<PlayerStateManager>();

        selectablesLayerMask = cardLayerMask | tileLayerMask | buttonsLayerMask | rotationarrowsLayerMask;
    }

    void Update()
    {
        if (!playerStateManager.IsHoverAllowed()) return; // Interactions are fully disabled
        if (!inputAllowed) return;

        ISelectable selected = GetSelectable(); // To Wouter: So all function on this game object will be server side

        ManageHover(selected);
        if (Input.GetMouseButtonDown(0)) // Player attempts to click
            ManageSelection(selected);  // THIS IS OK! (NOT AWAIT)
    }

    private ISelectable GetSelectable()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); // To Wouter: will using a "main" camera cause issues?   <- It shouldn't, the main camera can be different for both players
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, selectablesLayerMask))
        {
            ISelectable selectable = hit.collider.GetComponent<ISelectable>();
            if (allowedSelectables.Contains(selectable))
                return selectable;
        }
        return null;
    }

    private void ManageHover(ISelectable selectable)
    {
        // if new object equals old object then pass
        if (currentHover == selectable)
            return;
        Debug.Log("currentHover is now: " + currentHover);
        if (currentHover != null)
            currentHover.OnHoverExit(); // This should be done client side
        currentHover = selectable;
        if (currentHover != null)
            currentHover.OnHoverEnter(); // This should be done client side
    }

    private void NotYourTurnMessage()
    {
        Debug.Log("Not your turn!");
        UIManager.Instance.PlayNotYourTurnEffect();
    }


    private async Task ManageSelection(ISelectable selectable)
    {
        Debug.Log($"Player {Player.Instance.playerId} selected {selectable}");

        if (playerStateManager._currentState == PlayerState.Transitioning || playerStateManager._currentState == PlayerState.WatchingGame)
            NotYourTurnMessage();
        else if (playerStateManager._currentRequestState == PlayerRequestState.None) // Begin selection process
        {
            if (selectable is Button button)
                await ManageButtonSelection(button);
            else if (selectable is IAction action) // Start a new action (possibly)
            {
                if (playerStateManager._currentState != PlayerState.Playing)
                    NotYourTurnMessage();
                else if (action.Q_CanBeginAction())
                {   // Begin the action process
                    currentAction = action;
                    await playerStateManager.ToState(PlayerState.Playing, currentAction.Get_ActionCamera(), currentAction.Get_ActionInput());
                }
            }
        }
        else if (currentAction != null) // Finish current action process
        {
            PlayerCameraState PostActionState = await currentAction.Act(selectable);
            currentAction = null;
            await playerStateManager.ToState(PlayerState.Playing, PostActionState, PlayerRequestState.None);
        }
        else Debug.Log("Error (SelectionManager.ManageSelection): Requesting without an action.");
        return;
    }

    // Utility functions for managing selections

    private async Task ManageButtonSelection(Button button)
    {
        Debug.Log(button.buttonType);
        switch (button.buttonType)
        {
            case ButtonType.ViewHand:
                await playerStateManager.ToState(playerStateManager._currentState, PlayerCameraState.ViewingHand, PlayerRequestState.None);
                break;
            case ButtonType.ViewBoard:
                await playerStateManager.ToState(playerStateManager._currentState, PlayerCameraState.ViewingBoard, PlayerRequestState.None);
                break;
            case ButtonType.EndTurn:
                // You're only allowed to press the button if it's your turn
                if (playerStateManager._currentState != PlayerState.Playing)
                    NotYourTurnMessage();
                else
                    Referee.Instance.EndTurnRpc(); // Make server end turn
                                                   // ^ cannot be awaited anymore since it is async; check if this leads to any troubles
                break;
        }
    }

    // Setting Selectables

    private HashSet<ISelectable> allowedSelectables = new HashSet<ISelectable>();
    public void UpdateSelectables(PlayerState playerState, PlayerCameraState playerCamera, PlayerRequestState playerRequest)
    {
        allowedSelectables = new HashSet<ISelectable>();
        if (playerState == PlayerState.Transitioning || playerState == PlayerState.WatchingGame)
            return;
        switch (playerRequest)
        {
            case PlayerRequestState.None: // Begin a selection process (should be None always when state is Viewing!)
                switch (playerCamera)
                {
                    case PlayerCameraState.ViewingHand:
                        allowedSelectables.UnionWith(Get_CardsInHand()); // Hover for placement of card & card info
                        allowedSelectables.Add(play_Button);
                        allowedSelectables.Add(toBoard_Button);
                        return;

                    case PlayerCameraState.ViewingBoard:
                        allowedSelectables.UnionWith(Get_CardsOnBoard()); // Hover for UI and Rotation selection
                        // allowedSelectables.UnionWith(Get_ValidEmptyTiles()); // Hover for placement on tile
                        allowedSelectables.Add(play_Button);
                        allowedSelectables.Add(toHand_Button);
                        return;
                }
                return;
            case PlayerRequestState.Tiles_ValidEmpty:
                allowedSelectables.UnionWith(Get_ValidEmptyTiles());
                return;
            case PlayerRequestState.Cards_InHand:
                allowedSelectables.UnionWith(Get_CardsInHand());
                return;
            case PlayerRequestState.Cards_InPlay:
                allowedSelectables.UnionWith(Get_CardsOnBoard());
                return;
            case PlayerRequestState.RotationArrows:
                allowedSelectables.UnionWith(Get_RotationArrows());
                Debug.Log(allowedSelectables);
                return;
        }
    }

    // Utility functions for getting selectables

    private HashSet<ISelectable> Get_CardsInHand()
    {
        HashSet<ISelectable> selectables = new HashSet<ISelectable>();
        foreach (AbstractCard card in Player.Instance._hand) // To Wouter: specify player int thing
            selectables.Add(card);
        return selectables;
    }

    private HashSet<ISelectable> Get_CardsOnBoard()
    {
        HashSet<ISelectable> selectables = new HashSet<ISelectable>();
        foreach (Card card in Referee.Instance.cardList)
            selectables.Add(card);
        return selectables;
    }

    private HashSet<ISelectable> Get_ValidEmptyTiles()
    {
        HashSet<ISelectable> selectables = new HashSet<ISelectable>();
        foreach (HexCell tile in Board.Instance.cells.Values)
        {
            if (tile.GetCard() == null && tile.commander == Player.Instance.playerId) // Only allow playing on empty tiles owned by this player
                selectables.Add(tile);
        }
        return selectables;
    }

    private HashSet<ISelectable> Get_RotationArrows()
    {
        HashSet<ISelectable> selectables = new HashSet<ISelectable>();
        foreach (Card card in Referee.Instance.cardList)
        {
            if (card.Q_RotationArrowsShown)
            {
                selectables.Add(card.ra_Clock);
                selectables.Add(card.ra_Counter);
            }
        }
        return selectables;
    }
}
