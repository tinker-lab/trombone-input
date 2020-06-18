using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 649
public class LayoutDropdownController : IRaycastable
{
    public Dropdown dropdown;

    public Image image;

    [SerializeField]
    private Color highlightColor;

    private Color normalColor;


    private void Start()
    {
        normalColor = image.color;
    }

    protected override void OnRaycastFocusChange(bool value)
        => image.color = value ? highlightColor : normalColor;


}