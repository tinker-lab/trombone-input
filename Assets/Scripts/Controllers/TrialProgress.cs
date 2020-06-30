﻿using UnityEngine;

namespace Controller
{
#pragma warning disable 649
    public class TrialProgress : MonoBehaviour
    {
        [SerializeField]
        private UnityEngine.UI.Text counter, percent;

        [SerializeField]
        private UnityEngine.UI.Slider progressSlider;

        public (int num, int denom) trialCount
        {
            set => counter.text = $"Trial {value.num}/{value.denom}";
        }

        public float trialProgress
        {
            get => progressSlider.value;
            set
            {
                progressSlider.value = value;
                percent.text = $"{Mathf.FloorToInt(100 * value)}% Complete";
            }
        }
    }
}