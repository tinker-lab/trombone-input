﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CustomInput.Layout;

namespace CustomInput
{

    [Serializable]
    public enum LayoutOption
    {
        LinearABCDE,
        StylusBinnedABCDE,
        TwoRotBinnedABCDE,
        RaycastQWERTY,
    }

#pragma warning disable 649
    public class LayoutManager : MonoBehaviour
    {
        private LayoutOption _layout;
        public LayoutOption layout
        {
            get => _layout;
            set
            {
                _layout = value;
                ActivateLayout();
            }
        }

        #region EditorSet
        [SerializeField]
        private Controller.LayoutDropdown dropdownController;

        private Dropdown dropdown
            => dropdownController.dropdown;

        [SerializeField]
        private LinearABCDE linearABCDE;

        [SerializeField]
        private StylusBinnedABCDE stylusBinnedABCDE;

        [SerializeField]
        private TwoRotationABCDE twoRotationABCDE;

        [SerializeField]
        private RaycastQWERTY raycastQWERTY;
        #endregion

        public AbstractLayout currentLayout => fromOption(layout);

        public AbstractLayout fromOption(LayoutOption option)
        {
            switch (option)
            {
                case LayoutOption.LinearABCDE:
                    return linearABCDE;

                case LayoutOption.StylusBinnedABCDE:
                    return stylusBinnedABCDE;

                case LayoutOption.TwoRotBinnedABCDE:
                    return twoRotationABCDE;

                case LayoutOption.RaycastQWERTY:
                    return raycastQWERTY;
            }

            throw new ArgumentException($"unknown layout option: {option.ToString()} in fromOption");
        }

        public void Start()
        {
            dropdown.ClearOptions();
            dropdown.AddOptions(new List<string>(Enum.GetNames(typeof(LayoutOption))));
            dropdown.value = (int)layout;
            ActivateLayout();
        }

        // used in editor!
        public void DropdownValueSelected(int index)
        {
            layout = (LayoutOption)index;
            dropdown.value = (int)layout;
            ActivateLayout();
        }

        private void ActivateLayout()
        {
            foreach (var layoutOption in System.Enum.GetValues(typeof(LayoutOption)))
            {
                var layout = fromOption((LayoutOption)layoutOption);
                if (layout.gameObject.activeInHierarchy)
                {
                    layout.gameObject.SetActive(false);
                }
            }

            currentLayout.gameObject.SetActive(true);
        }
    }
}