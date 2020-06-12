﻿using UnityEngine;
using UnityEngine.UI;

public class BinnedKeyController : AbstractBinnedController<CustomInput.BinnedKey>
{
    public override void SetSlant(bool forward)
    {
        var children = transform.GetComponentsInChildren<SimpleKeyController>();
        for (int i = 0; i < children.Length; i++)
        {
            var child = children[i];
            switch (i % 3)
            {
                case 0:
                    child.alignment = forward ? TextAnchor.UpperCenter : TextAnchor.LowerCenter;
                    break;
                case 1:
                    child.alignment = TextAnchor.MiddleCenter;
                    break;
                case 2:
                    child.alignment = forward ? TextAnchor.LowerCenter : TextAnchor.UpperCenter;
                    break;
            }
        }
    }
}

public abstract class AbstractBinnedController<T> : KeyController<T>
    where T : CustomInput.BinnedKey
{
    public Color highlightColor;

    protected Color normalColor;
    protected bool highlighting = false;
    public Image background;

    public override void SetHighlight(bool h)
    {
        if (h)
        {
            normalColor = background.color;

            background.color = highlightColor;
        }
        else if (highlighting)
        {
            background.color = normalColor;
        }

        highlighting = h;
    }

    public bool useAlternate
    {
        set
        {
            foreach (var controller in gameObject.GetComponentsInChildren<AbstractSimpleKeyController>())
            {
                controller.useAlternate = value;
            }
        }
    }

    public void AddChild(GameObject g)
    {
        g.transform.SetParent(transform);
    }

    public abstract void SetSlant(bool forward);
}