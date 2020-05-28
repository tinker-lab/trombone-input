﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class DisplayController : MonoBehaviour
{
    public GameObject basicItem, blockItem;

    readonly List<GameObject> childMap = new List<GameObject>(64);

    void Start()
    {
        fillItems();

        foreach (var item in items)
        {
            var newChild = item.representation(transform, blockItem, basicItem);

            var blockController = newChild.GetComponent<BlockDisplayItemController>();

            if (blockController)
            {
                blockController.item = item;
            }

            for (int i = 0; i < item.size(); i++)
            {
                childMap.Add(newChild);
            }
        }

        ResizeAll();
    }

    float lastWidth = -1;

    private void Update()
    {
        var width = gameObject.GetComponent<RectTransform>().rect.width;

        if (lastWidth == width) return;

        ResizeAll();
    }

    private void ResizeAll()
    {
        var width = gameObject.GetComponent<RectTransform>().rect.width;
        var unitWidth = width / 64.0f;

        foreach (var child in gameObject.GetComponentsInChildren<AbstractDisplayItemController>())
        {
            child.Resize(unitWidth);
        }
    }

    public BasicLayoutItem ExactItemAt(int index)
    {
        Assert.IsFalse(index < 0);

        int remaining = index;
        foreach (LayoutItem item in items)
        {
            if (remaining < item.size())
            {
                return item.ItemAt(remaining);
            }
            else
            {
                remaining -= item.size();
            }
        }

        return null;
    }

    public GameObject ChildAt(int index)
    {
        return childMap.Count <= index ? null : childMap[index];
    }

    // Auto-generated 
    private LayoutItem[] items;
    private void fillItems()
    {
        var basicItem0 = ScriptableObject.CreateInstance<BasicLayoutItem>();
        basicItem0.init('Q', 2);
        var basicItem1 = ScriptableObject.CreateInstance<BasicLayoutItem>();
        basicItem1.init('A', 3);
        var basicItem2 = ScriptableObject.CreateInstance<BasicLayoutItem>();
        basicItem2.init('Z', 2);
        var blockItem3 = ScriptableObject.CreateInstance<BlockLayoutItem>();
        blockItem3.init(true, basicItem0, basicItem1, basicItem2);

        var basicItem4 = ScriptableObject.CreateInstance<BasicLayoutItem>();
        basicItem4.init('W', 2);
        var basicItem5 = ScriptableObject.CreateInstance<BasicLayoutItem>();
        basicItem5.init('S', 3);
        var basicItem6 = ScriptableObject.CreateInstance<BasicLayoutItem>();
        basicItem6.init('X', 2);
        var blockItem7 = ScriptableObject.CreateInstance<BlockLayoutItem>();
        blockItem7.init(true, basicItem4, basicItem5, basicItem6);

        var basicItem8 = ScriptableObject.CreateInstance<BasicLayoutItem>();
        basicItem8.init('E', 2);
        var basicItem9 = ScriptableObject.CreateInstance<BasicLayoutItem>();
        basicItem9.init('D', 3);
        var basicItem10 = ScriptableObject.CreateInstance<BasicLayoutItem>();
        basicItem10.init('C', 2);
        var blockItem11 = ScriptableObject.CreateInstance<BlockLayoutItem>();
        blockItem11.init(true, basicItem8, basicItem9, basicItem10);

        var basicItem12 = ScriptableObject.CreateInstance<BasicLayoutItem>();
        basicItem12.init('R', 2);
        var basicItem13 = ScriptableObject.CreateInstance<BasicLayoutItem>();
        basicItem13.init('F', 3);
        var basicItem14 = ScriptableObject.CreateInstance<BasicLayoutItem>();
        basicItem14.init('V', 2);
        var blockItem15 = ScriptableObject.CreateInstance<BlockLayoutItem>();
        blockItem15.init(true, basicItem12, basicItem13, basicItem14);

        var basicItem16 = ScriptableObject.CreateInstance<BasicLayoutItem>();
        basicItem16.init('T', 2);
        var basicItem17 = ScriptableObject.CreateInstance<BasicLayoutItem>();
        basicItem17.init('G', 3);
        var basicItem18 = ScriptableObject.CreateInstance<BasicLayoutItem>();
        basicItem18.init('B', 2);
        var blockItem19 = ScriptableObject.CreateInstance<BlockLayoutItem>();
        blockItem19.init(true, basicItem16, basicItem17, basicItem18);

        var basicItem20 = ScriptableObject.CreateInstance<BasicLayoutItem>();
        basicItem20.init('U', 2);
        var basicItem21 = ScriptableObject.CreateInstance<BasicLayoutItem>();
        basicItem21.init('H', 3);
        var basicItem22 = ScriptableObject.CreateInstance<BasicLayoutItem>();
        basicItem22.init('Y', 2);
        var blockItem23 = ScriptableObject.CreateInstance<BlockLayoutItem>();
        blockItem23.init(false, basicItem20, basicItem21, basicItem22);

        var basicItem24 = ScriptableObject.CreateInstance<BasicLayoutItem>();
        basicItem24.init('N', 2);
        var basicItem25 = ScriptableObject.CreateInstance<BasicLayoutItem>();
        basicItem25.init('J', 3);
        var basicItem26 = ScriptableObject.CreateInstance<BasicLayoutItem>();
        basicItem26.init('I', 2);
        var blockItem27 = ScriptableObject.CreateInstance<BlockLayoutItem>();
        blockItem27.init(false, basicItem24, basicItem25, basicItem26);

        var basicItem28 = ScriptableObject.CreateInstance<BasicLayoutItem>();
        basicItem28.init('M', 2);
        var basicItem29 = ScriptableObject.CreateInstance<BasicLayoutItem>();
        basicItem29.init('K', 3);
        var basicItem30 = ScriptableObject.CreateInstance<BasicLayoutItem>();
        basicItem30.init('O', 2);
        var blockItem31 = ScriptableObject.CreateInstance<BlockLayoutItem>();
        blockItem31.init(false, basicItem28, basicItem29, basicItem30);

        // var basicItem32 = ScriptableObject.CreateInstance<BasicLayoutItem>();
        // basicItem32.init('.', 2);
        var basicItem33 = ScriptableObject.CreateInstance<BasicLayoutItem>();
        basicItem33.init('L', 3);
        var basicItem34 = ScriptableObject.CreateInstance<BasicLayoutItem>();
        basicItem34.init('P', 2);
        var blockItem35 = ScriptableObject.CreateInstance<BlockLayoutItem>();
        blockItem35.init(false, /* basicItem32,*/ basicItem33, basicItem34);
        items = new LayoutItem[] {
            blockItem3,
            blockItem7,
            blockItem11,
            blockItem15,
            blockItem19,
            blockItem23,
            blockItem27,
            blockItem31,
            blockItem35
        };
    }

}