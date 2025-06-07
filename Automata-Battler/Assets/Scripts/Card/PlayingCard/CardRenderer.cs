using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using TMPro;
using System.Linq;
using Unity.Netcode;
using System.Threading.Tasks;
using System;

[RequireComponent(typeof(Card))]
[DisallowMultipleComponent]
public class CardRenderer : MonoBehaviour
{
    private Card card;
    [SerializeField] private Transform elementsTransform;

    [SerializeField] private RenderElement cost;
    [SerializeField] private RenderElement health;
    [SerializeField] private RenderElement damage;
    [SerializeField] private RenderElement initiative;
    [SerializeField] private RenderElement instructions;
    [SerializeField] private RenderElement rotateClockwise;
    [SerializeField] private RenderElement rotateCounterclockwise;

    private List<RenderElement> UIElements_InPlay;
    private List<RenderElement> UIElements_InHand;
    private List<RenderElement> UIElements_RotationArrows;


    void Awake()
    {
        card = GetComponent<Card>();
        UIElements_InPlay = new List<RenderElement> { health, damage, initiative, instructions };
        // UIElements_InHand = new List<RenderElement> { elements_InHand };
        UIElements_InHand = new List<RenderElement> { };
        UIElements_RotationArrows = new List<RenderElement> { rotateClockwise, rotateCounterclockwise };
    }

    void Start()
    {
        ulong playerId = Player.Instance.playerId;
        if (playerId == 2)
            elementsTransform.localRotation = Quaternion.Euler(0, 180, 0);
    }

    public void Render_UpdateText()
    {
        cost.RenderText($"$:{card._cost}");
        health.RenderText($"{card._health}");
        damage.RenderText($"{card._damage}");
        initiative.RenderText($"{card._initiative}");

        ulong playerId = Player.Instance.playerId;
        if (playerId == 1)
            instructions.RenderText(string.Join(" ", card._instructions.Select(instruction =>
                    { return instruction.GetVisual(); })));
        else if (playerId == 2)
            instructions.RenderText(string.Join(" ", card._instructions.Select(instruction =>
                    { return instruction.GetVisual_Client(); })));
    }

    float UIState_InPlay = 0f;
    float UIState_InHand = 0f;
    float UIState_RotationArrows = 0f;

    int UITarget_InPlay = 0;
    int UITarget_InHand = 0;
    int UITarget_RotationArrows = 0;


    private bool UIControlerInPlay_Active = false;
    private bool UIControlerInHand_Active = false;
    private bool UIControlerRotationArrows_Active = false;

    private IEnumerator UI_Controler_InPlay()
    {
        float t;
        while ((UITarget_InPlay == 0 && UIState_InPlay > UITarget_InPlay) || (
        UITarget_InPlay == 1 && UIState_InPlay < UITarget_InPlay))
        {
            t = Easing_Smooth(UIState_InPlay);
            foreach (RenderElement renderElement in UIElements_InPlay)
                renderElement.RevealAmount(t);
            UIState_InPlay += Time.deltaTime * 5 * (UITarget_InPlay - 0.5f) * 2;
            yield return null;
        }
        foreach (RenderElement renderElement in UIElements_InPlay)
            renderElement.RevealAmount(UITarget_InPlay);
        UIControlerInPlay_Active = false;
    }

    private IEnumerator UI_Controler_InHand()
    {
        float t;
        while ((UITarget_InHand == 0 && UIState_InHand > UITarget_InHand) || (
        UITarget_InHand == 1 && UIState_InHand < UITarget_InHand))
        {
            t = Easing_Smooth(UIState_InHand);
            foreach (RenderElement renderElement in UIElements_InHand)
                renderElement.RevealAmount(t);
            UIState_InHand += Time.deltaTime * 5 * (UITarget_InHand - 0.5f) * 2;
            yield return null;
        }
        foreach (RenderElement renderElement in UIElements_InHand)
            renderElement.RevealAmount(UITarget_InHand);
        // if (Final_InHand_Routine) cost.gameObject.SetActive(false); // disable??
        UIControlerInHand_Active = false;
    }

    private IEnumerator UI_Controler_RotationArrows()
    {
        rotateClockwise.gameObject.SetActive(true);
        rotateCounterclockwise.gameObject.SetActive(true);
        float t;
        while ((UITarget_RotationArrows == 0 && UIState_RotationArrows > UITarget_RotationArrows) || (
        UITarget_RotationArrows == 1 && UIState_RotationArrows < UITarget_RotationArrows))
        {
            t = Easing_Smooth(UIState_RotationArrows);
            foreach (RenderElement renderElement in UIElements_RotationArrows)
                renderElement.RevealAmount(t);
            UIState_RotationArrows += Time.deltaTime * 5 * (UITarget_RotationArrows - 0.5f) * 2;
            yield return null;
        }
        foreach (RenderElement renderElement in UIElements_RotationArrows)
            renderElement.RevealAmount(UITarget_RotationArrows);
        if (UITarget_RotationArrows == 0)
        {
            rotateClockwise.gameObject.SetActive(false);
            rotateCounterclockwise.gameObject.SetActive(false);
        }
        UIControlerRotationArrows_Active = false;
    }

    private static float Easing_Smooth(float t)
    {
        return 3 * t * t - 2 * t * t * t;
    }

    public void RenderUI(int target) // 1 is shown, 0 is hidden
    {
        if (card._inPlay)
        {
            UITarget_InPlay = target;
            if (!UIControlerInPlay_Active)
            {
                UIControlerInPlay_Active = true;
                StartCoroutine(UI_Controler_InPlay());
            }
        }
        else if (!card._inPlay)
        {
            UITarget_InHand = target;
            if (!UIControlerInHand_Active)
            {
                UIControlerInHand_Active = true;
                StartCoroutine(UI_Controler_InHand());
            }
        }
    }

    public void RenderArrows(int target) // 1 is shown, 0 is hidden
    {
        UITarget_RotationArrows = target;
        if (!UIControlerRotationArrows_Active)
        {
            UIControlerRotationArrows_Active = true;
            StartCoroutine(UI_Controler_RotationArrows());
        }
    }

    // private bool Final_InHand_Routine = false;
    public void CardNowInPlay()
    {
        UITarget_InHand = 0;
        // Final_InHand_Routine = true;
        if (!UIControlerInPlay_Active)
        {
            UIControlerInHand_Active = true;
            StartCoroutine(UI_Controler_InHand());
        }
        cost.gameObject.SetActive(false);
        initiative.gameObject.SetActive(true);
    }
}
