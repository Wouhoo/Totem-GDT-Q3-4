using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

[RequireComponent(typeof(Player))]
[RequireComponent(typeof(PlayerStateManager))]
[DisallowMultipleComponent]
public class SelectionManager : MonoBehaviour
{
    private Player player;
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
    private Card selectedCard;

    void Awake()
    {
        player = GetComponent<Player>();
        playerStateManager = GetComponent<PlayerStateManager>();

        referee = FindFirstObjectByType<Referee>();
        board = FindFirstObjectByType<Board>();

        selectablesLayerMask = cardLayerMask | tileLayerMask | buttonsLayerMask;
    }

    void Update()
    {
        if (!playerStateManager.IsHoverAllowed()) return; // Interactions are fully disabled

        ISelectable selected = GetSelectable();

        ManageHover(selected);
        if (Input.GetMouseButtonDown(0)) // Player attempts to click
            ManageSelection(selected);  // THIS IS OK! (NOT AWAIT)
    }

    private ISelectable GetSelectable()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, selectablesLayerMask))
        {
            ISelectable selectable = hit.collider.GetComponent<ISelectable>();
            if (allowedSelectables.Contains(selectable))
                return selectable;
            //sorry tim im workaround-ing ~Lars
            // else if (selectable is HexCell)
            // {
            //return selectable;
            //}
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
            currentHover.OnHoverExit();
        currentHover = selectable;
        if (currentHover != null)
            currentHover.OnHoverEnter();
    }

    private async Task ManageSelection(ISelectable selectable)
    {
        Debug.Log($"{player} selected {selectable}");
        switch (playerStateManager._currentState)
        {
            case PlayerState.PlacingCard:
                if (selectable is HexCell tile && player._isPlayerTurn)
                {
                    Task<bool> task_PlayCard = player.PlayCard(selectedCard, tile);
                    await task_PlayCard;
                    bool res_PlayCard = task_PlayCard.Result;
                    if (res_PlayCard)
                    {
                        selectedCard = null;
                        await playerStateManager.ToState(PlayerState.ViewingBoard);
                        break;
                    }
                }
                selectedCard = null;
                await playerStateManager.ToState(PlayerState.ViewingHand);
                break;

            case PlayerState.ViewingHand:
                if (selectable is Card card && player._isPlayerTurn)
                {
                    if (player._hand.Contains(card) && card._cost <= player._mana) // card in hand and we have enough mana
                    {
                        selectedCard = card;
                        await playerStateManager.ToState(PlayerState.PlacingCard);
                    }
                }
                else if (selectable is Button button1)
                    await ManageButtonSelection(button1);
                break;

            case PlayerState.ViewingBoard:
                if (selectable is Button button2)
                    await ManageButtonSelection(button2);
                break;

            case PlayerState.WatchingGame:
                // should never happen...
                break;

            case PlayerState.Transitioning:
                // should never happen...
                break;
        }
    }

    // Utility functions for managing selections

    private async Task ManageButtonSelection(Button button)
    {
        Debug.Log(button.buttonType);
        switch (button.buttonType)
        {
            case ButtonType.ViewHand:
                await playerStateManager.ToState(PlayerState.ViewingHand);
                break;
            case ButtonType.ViewBoard:
                await playerStateManager.ToState(PlayerState.ViewingBoard);
                break;
            case ButtonType.EndTurn:
                await referee.EndTurn();
                break;
        }
    }

    // Setting Selectables

    private HashSet<ISelectable> allowedSelectables;
    public void UpdateSelectables(PlayerState playerState)
    {
        allowedSelectables = new HashSet<ISelectable>();
        switch (playerState)
        {
            case PlayerState.PlacingCard:
                allowedSelectables.UnionWith(Get_EmptyTiles());
                break;

            case PlayerState.ViewingHand:
                allowedSelectables.UnionWith(Get_CardsInHand());
                allowedSelectables.Add(play_Button);
                allowedSelectables.Add(toBoard_Button);
                break;

            case PlayerState.ViewingBoard:
                allowedSelectables.UnionWith(Get_CardsOnBoard());
                allowedSelectables.UnionWith(Get_EmptyTiles());
                allowedSelectables.Add(play_Button);
                allowedSelectables.Add(toHand_Button);
                break;

            case PlayerState.WatchingGame:
                break;

            case PlayerState.Transitioning:
                break;
        }
    }

    // Utility functions for getting selectables

    private HashSet<ISelectable> Get_CardsInHand()
    {
        HashSet<ISelectable> selectables = new HashSet<ISelectable>();
        foreach (Card card in player._hand)
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

    private HashSet<ISelectable> Get_EmptyTiles()
    {
        HashSet<ISelectable> selectables = new HashSet<ISelectable>();
        foreach (HexCell tile in board.cells.Values)
        {
            if (tile.Get_Card() == null)
                selectables.Add(tile);
        }
        return selectables;
    }
}
