using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonColorChange : MonoBehaviour
{
    private Color normalColor = Color.white;
    private Color hoverColor = Color.cyan;

    private Image buttonImage;

    void Start()
    {
        buttonImage = GetComponent<Image>();
    }

    public void HoverStart(Image img)
    {
        img.color = hoverColor;
    }

    public void HoverEnd(Image img)
    {
        img.color = normalColor;
    }
}