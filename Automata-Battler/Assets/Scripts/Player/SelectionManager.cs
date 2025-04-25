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
        if (!playerStateManager.IsInteractionAllowed()) return; // Interactions are disabled

        ISelectable selected = GetSelectable();

        ManageHover(selected);
        if (Input.GetMouseButtonDown(0))
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
        }
        return null;
    }

    private void ManageHover(ISelectable selectable)
    {
        // if new object equals old object then pass
        if (currentHover == selectable)
            return;

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
                if (selectable is HexCell tile)
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
                await playerStateManager.ToState(PlayerState.ViewingBoard);
                break;

            case PlayerState.ViewingHand:
                if (selectable is Card card)
                {
                    if (player._hand.Contains(card) && card._cost <= player._mana) // card in hand and we have enough mana
                    {
                        selectedCard = card;
                        await playerStateManager.ToState(PlayerState.PlacingCard);
                    }
                }
                else if (selectable is Button button1)
                {
                    if (button1 == toBoard_Button)
                        await playerStateManager.ToState(PlayerState.ViewingBoard);
                    else if (button1 == play_Button)
                        await referee.EndTurn();
                }
                break;

            case PlayerState.ViewingBoard:
                if (selectable is Button button2)
                {
                    if (button2 == toHand_Button)
                        await playerStateManager.ToState(PlayerState.ViewingHand);
                    else if (button2 == play_Button)
                        await referee.EndTurn();
                }
                break;

            case PlayerState.WatchingGame:
                // should never happen...
                break;

            case PlayerState.Transitioning:
                // should never happen...
                break;
        }
    }

    private HashSet<ISelectable> allowedSelectables;
    public void UpdateSelectables(PlayerState playerState)
    {
        switch (playerState)
        {
            case PlayerState.PlacingCard:
                allowedSelectables = new HashSet<ISelectable>();
                // Tiles with nothing on them
                foreach (HexCell tile in board.cells.Values)
                {
                    if (tile.Get_Card() == null)
                        allowedSelectables.Add(tile);
                }
                break;

            case PlayerState.ViewingHand:
                allowedSelectables = new HashSet<ISelectable>();
                // Cards in hand + play button + toboard button
                foreach (Card card in player._hand)
                    allowedSelectables.Add(card);
                allowedSelectables.Add(play_Button);
                allowedSelectables.Add(toBoard_Button);
                break;

            case PlayerState.ViewingBoard:
                allowedSelectables = new HashSet<ISelectable>();
                // Cards on board + play button + tohand button
                foreach (Card card in referee.cardList)
                    allowedSelectables.Add(card);
                allowedSelectables.Add(play_Button);
                allowedSelectables.Add(toHand_Button);
                break;

            case PlayerState.WatchingGame:
                // None
                break;

            case PlayerState.Transitioning:
                // None
                break;
        }
    }
}
