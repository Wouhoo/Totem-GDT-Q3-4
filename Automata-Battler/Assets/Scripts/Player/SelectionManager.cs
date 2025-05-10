using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using NUnit.Framework.Constraints;

[RequireComponent(typeof(Player))]
[RequireComponent(typeof(PlayerStateManager))]
[DisallowMultipleComponent]
public class SelectionManager : MonoBehaviour
{
    private Player player; // To Wouter: int thing
    private PlayerStateManager playerStateManager;
    private Referee referee;
    private Board board;

    [SerializeField] private LayerMask cardLayerMask;
    [SerializeField] private LayerMask tileLayerMask;
    [SerializeField] private LayerMask buttonsLayerMask;
    private LayerMask selectablesLayerMask;

    [SerializeField] private Button toBoard_Button;
    [SerializeField] private Button toHand_Button;
    [SerializeField] private Button play_Button;

    private ISelectable currentHover;
    private IAction currentAction;

    void Awake()
    {
        player = GetComponent<Player>(); // To Wouter: int thing
        playerStateManager = GetComponent<PlayerStateManager>();

        referee = FindFirstObjectByType<Referee>();
        board = FindFirstObjectByType<Board>();

        selectablesLayerMask = cardLayerMask | tileLayerMask | buttonsLayerMask;
    }

    void Update()
    {
        if (!playerStateManager.IsHoverAllowed()) return; // Interactions are fully disabled

        ISelectable selected = GetSelectable(); // To Wouter: So all function on this game object will be server side

        ManageHover(selected);
        if (Input.GetMouseButtonDown(0)) // Player attempts to click
            ManageSelection(selected);  // THIS IS OK! (NOT AWAIT)
    }

    private ISelectable GetSelectable()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); // To Wouter: will using a "main" camera cause issues?
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
            currentHover.OnHoverExit(); // To Wouter: this can be done client side
        currentHover = selectable;
        if (currentHover != null)
            currentHover.OnHoverEnter(); // To Wouter: this can be done client side
    }

    private async Task ManageSelection(ISelectable selectable)
    {
        Debug.Log($"{player} selected {selectable}");
        if (playerStateManager._currentRequestState == PlayerRequestState.None) // Begin selection process
        {
            if (selectable is Button button)
            {
                await ManageButtonSelection(button);
                return;
            }
            else if (selectable is IAction action)
            {
                // pre action check for mana if needed:
                if (selectable is AbstractCard card && card._cost > player._mana) // To Wouter: int thing
                    return;
                currentAction = action;
                await playerStateManager.ToState(PlayerState.Acting, currentAction.Get_ActionCamera(), currentAction.Get_ActionInput());
                return;
            }
        }
        else // Finish selection process
        {
            if (currentAction == null)
                return; // UHHHHHH... NOT POSSIBLE HOPEFULLY????
            await currentAction.Act(selectable); // To Wouter: server side i think
            currentAction = null;
            // if sucsess
            // await playerStateManager.ToState(PlayerState.Playing, PlayerCameraState.ViewingBoard, PlayerRequestState.None);
            // if fail
            await playerStateManager.ToState(PlayerState.Playing, PlayerCameraState.ViewingHand, PlayerRequestState.None);
        }
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
                await referee.EndTurn(); // To Wouter: server side i think
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
        else if (playerRequest == PlayerRequestState.None) // Begin a selection process
        {
            switch (playerCamera)
            {
                case PlayerCameraState.ViewingHand:
                    allowedSelectables.UnionWith(Get_CardsInHand());
                    allowedSelectables.Add(play_Button);
                    allowedSelectables.Add(toBoard_Button);
                    return;

                case PlayerCameraState.ViewingBoard:
                    allowedSelectables.UnionWith(Get_CardsOnBoard());
                    allowedSelectables.UnionWith(Get_ValidEmptyTiles());
                    allowedSelectables.Add(play_Button);
                    allowedSelectables.Add(toHand_Button);
                    return;

                case PlayerCameraState.ViewingActions:
                    allowedSelectables.UnionWith(Get_CardsOnBoard());
                    allowedSelectables.UnionWith(Get_ValidEmptyTiles());
                    allowedSelectables.Add(play_Button);
                    allowedSelectables.Add(toHand_Button);
                    allowedSelectables.Add(toBoard_Button);
                    return;
            }
        }
        else if (playerState == PlayerState.Acting)
        {
            switch (playerRequest)
            {
                case PlayerRequestState.Tiles_ValidEmpty:
                    allowedSelectables.UnionWith(Get_ValidEmptyTiles());
                    return;
                case PlayerRequestState.Cards_InHand:
                    allowedSelectables.UnionWith(Get_CardsInHand());
                    return;
                case PlayerRequestState.Cards_InPlay:
                    allowedSelectables.UnionWith(Get_CardsOnBoard());
                    return;
            }
        }
    }

    // Utility functions for getting selectables

    private HashSet<ISelectable> Get_CardsInHand()
    {
        HashSet<ISelectable> selectables = new HashSet<ISelectable>();
        foreach (AbstractCard card in player._hand) // To Wouter: specify player int thing
            selectables.Add(card);
        return selectables;
    }

    private HashSet<ISelectable> Get_CardsOnBoard()
    {
        HashSet<ISelectable> selectables = new HashSet<ISelectable>();
        foreach (Card card in referee.cardList)
            selectables.Add(card);
        return selectables;
    }

    private HashSet<ISelectable> Get_ValidEmptyTiles()
    {
        HashSet<ISelectable> selectables = new HashSet<ISelectable>();
        foreach (HexCell tile in board.cells.Values)
        {
            if (tile.GetCard() == null && (tile.commander == player || tile.commander == null)) // To Wouter: int thing
                selectables.Add(tile);
        }
        return selectables;
    }
}
