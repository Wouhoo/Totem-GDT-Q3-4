using UnityEngine;
using TMPro;
using System.Linq;

public class RenderElement : MonoBehaviour
{
    private MeshRenderer meshRenderer;
    [SerializeField] private TextMeshProUGUI TMPgui;
    [SerializeField] private Vector3 shownPosition;
    [SerializeField] private Vector3 shownScale;
    [SerializeField] float shownAlpha;
    [SerializeField] private Vector3 hiddenPosition;
    [SerializeField] private Vector3 hiddenScale;
    [SerializeField] private float hiddenAlpha;
    Vector3 midpointMax;
    private Vector3 directionPosition;
    private Vector3 directionScale;
    private float directionAlpha;
    private Color color;

    void Awake()
    {
        midpointMax = (shownPosition + hiddenPosition) / 2 + new Vector3(0, 0, 0); // (0,2,0)
        meshRenderer = GetComponent<MeshRenderer>();

        directionPosition = shownPosition - hiddenPosition;
        directionScale = shownScale - hiddenScale;
        directionAlpha = shownAlpha - hiddenAlpha;

        color = meshRenderer.material.color;
    }

    public void RevealAmount(float t)
    {
        transform.localPosition = hiddenPosition + directionPosition * t; // + midpointMax * 4 * t * (1 - t);
        transform.localScale = hiddenScale + t * directionScale;
        meshRenderer.material.color = new Color(color.r, color.g, color.b, hiddenAlpha + t * directionAlpha);
    }

    public void RenderText(string newText)
    {
        TMPgui.text = newText;
    }
}
