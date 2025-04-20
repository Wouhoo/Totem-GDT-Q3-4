using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

// THIS ENTIRE CLASS NEEDS TO BE RE-WORKED!

public class Player : MonoBehaviour
{
    [SerializeField] private static bool activePlayer;
    private Referee referee;
    private Board board;
    [SerializeField] private Deck deck;

    [SerializeField] private LayerMask cardLayerMask;
    [SerializeField] private LayerMask tileLayerMask;
    [SerializeField] private LayerMask buttonsLayerMask;
    private LayerMask interactablesLayerMask;

    [SerializeField] public GameObject obj_toBoard_Button;
    [SerializeField] public GameObject obj_toHand_Button;
    [SerializeField] public GameObject obj_play_Button;

    [SerializeField] private Interactable toBoard_Button;
    [SerializeField] private Interactable toHand_Button;
    [SerializeField] private Interactable play_Button;

    void Awake()
    {
        interactablesLayerMask = cardLayerMask | tileLayerMask | buttonsLayerMask;

        toBoard_Button = obj_toBoard_Button.GetComponent<Button_ToBoard>();
        toHand_Button = obj_toHand_Button.GetComponent<Button_ToHand>();
        play_Button = obj_play_Button.GetComponent<Button_Play>();
    }

    void Start()
    {
        referee = FindFirstObjectByType<Referee>();
        board = FindFirstObjectByType<Board>();


        DrawCards();
        ToState(State.ViewHand);
    }

    //
    // Update system
    //

    private bool canInteract = false;
    private Transform target;
    [SerializeField] private Transform handCameraPosition;
    [SerializeField] private Transform boardCameraPosition;
    [SerializeField] private float lerpSpeed = 1f;
    private HashSet<Interactable> allowedInteractables;
    private Card selectedCard;

    void Update()
    {
        // LERP IF NEEDED

        if (target != null)
            LerpToTarget();

        if (!canInteract)
            return;

        Interactable interacted = GetInteractable();

        SwapHoveredTo(interacted);
        if (Input.GetMouseButtonDown(0))
            SelectInteractable(interacted);
    }

    private void LerpToTarget()
    {
        transform.position = Vector3.Lerp(transform.position, target.position, Time.deltaTime * lerpSpeed);
        transform.rotation = Quaternion.Lerp(transform.rotation, target.rotation, Time.deltaTime * lerpSpeed);

        float distance = Vector3.Distance(transform.position, target.position);
        float angle = Quaternion.Angle(transform.rotation, target.rotation);

        if (distance < 0.01f && angle < 1f)
        {
            // Snap into final position
            transform.position = target.position;
            transform.rotation = target.rotation;

            // Clear target to stop updating
            target = null;
            canInteract = true;
        }
    }

    //
    // Interaction States
    //

    private State state = State.ViewHand;

    public enum State
    {
        PlaceCard,
        ViewHand,
        ViewBoard,
        WatchGame
    }

    //
    // Interaction System
    //

    private Interactable _hovered;
    private Interactable _selected;

    private void SwapHoveredTo(Interactable hovered)
    {
        // if new object equals old object then pass
        if (_hovered == hovered)
            return;

        if (_hovered != null)
            _hovered.OnDehover();
        _hovered = hovered;
        if (_hovered != null)
            _hovered.OnHover();
    }

    private void SelectInteractable(Interactable interacted)
    {
        switch (state)
        {
            case State.PlaceCard:
                if (interacted is HexCell tile)
                {
                    selectedCard.PlaceCard(tile.coordinates);
                    hand.Remove(selectedCard);
                    mana -= selectedCard._cost;
                    SwapSelectedTo(null);
                    referee.AddCard(selectedCard);
                    ToState(State.ViewBoard);
                    break;
                }
                selectedCard = null;
                SwapSelectedTo(null);
                ToState(State.ViewHand);
                break;

            case State.ViewHand:
                if (interacted is Button_Play)
                {
                    referee.EndTurn();
                    break;
                }
                else if (interacted is Button_ToBoard)
                {
                    ToState(State.ViewBoard);
                    break;
                }
                else if (interacted is Card card)
                {
                    selectedCard = card;
                    ToState(State.PlaceCard);
                    break;
                }
                break;

            case State.ViewBoard:
                Debug.Log("???");
                if (interacted is Button_Play)
                {
                    referee.EndTurn();
                    break;
                }
                else if (interacted is Button_ToHand)
                {
                    ToState(State.ViewHand);
                    break;
                }
                break;

            case State.WatchGame:
                break;
        }
    }

    private void SwapSelectedTo(Interactable selected)
    {
        if (_selected != null)
            _selected.OnDeselect();
        _selected = selected;
        if (_selected != null)
            _selected.OnSelect();
    }

    //
    // Interaction Raycast
    //

    private Interactable GetInteractable()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, interactablesLayerMask))
        {
            Interactable interacted = hit.collider.GetComponent<Interactable>();
            if (allowedInteractables.Contains(interacted))
                return interacted;
        }
        return null;
    }

    //
    // State Transitions
    //

    public void ToState(State toState)
    {
        canInteract = false;
        switch (toState)
        {
            case State.PlaceCard:
                state = State.PlaceCard;
                allowedInteractables = new HashSet<Interactable>();
                // Tiles with nothing on them
                foreach (HexCell tile in board.cells.Values)
                {
                    if (tile.Get_Card() == null)
                        allowedInteractables.Add(tile);
                }

                target = boardCameraPosition;
                Debug.Log(state);
                break;

            case State.ViewHand:
                state = State.ViewHand;
                allowedInteractables = new HashSet<Interactable>();
                // Cards in hand + play button + toboard button
                foreach (Card card in hand)
                    allowedInteractables.Add(card);
                allowedInteractables.Add(play_Button);
                allowedInteractables.Add(toBoard_Button);

                target = handCameraPosition;
                Debug.Log(state);
                break;

            case State.ViewBoard:
                state = State.ViewBoard;
                allowedInteractables = new HashSet<Interactable>();
                // All cards in play & all tiles & the play button & the toHand button
                foreach (Card card in referee.cardList)
                    allowedInteractables.Add(card);
                foreach (HexCell tile in board.cells.Values)
                    allowedInteractables.Add(tile);
                allowedInteractables.Add(play_Button);
                allowedInteractables.Add(toHand_Button);

                target = boardCameraPosition;
                Debug.Log(state);
                break;

            case State.WatchGame:
                state = State.WatchGame;
                allowedInteractables = new HashSet<Interactable>();
                target = handCameraPosition;
                Debug.Log(state);
                break;
        }
    }

    //
    // Hand
    //

    private List<Card> hand = new List<Card>();

    public void DrawCards()
    {
        if (hand.Count == 5)
            return;
        int neededCards = 5 - hand.Count;
        for (int i = 0; i < neededCards; i++)
            hand.Add(deck.DrawCard());

        for (int i = 0; i < 5; i++)
        {
            Card card = hand[i];
            card.transform.position = new Vector3(20 * i, 0, -25);
        }
    }

    //
    // Mana System
    //

    private int mana = 5;

    public void ResetMana()
    {
        mana = 5;
    }
}
