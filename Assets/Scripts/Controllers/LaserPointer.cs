﻿using UnityEngine;

namespace Controller
{
#pragma warning disable 108
#pragma warning disable 649
    public class LaserPointer : MonoBehaviour
    {
        [SerializeField]
        private Stylus modelController;

        [SerializeField]
        private LineRenderer renderer;

        public bool active
        {
            get => gameObject.activeInHierarchy;
            set => gameObject.SetActive(value);
        }

        void Update()
        {
            if (!transform.hasChanged) return;

            RaycastHit? hit;
            modelController.Raycast(out hit);

            renderer.SetPosition(1, (hit?.distance ?? 50) * Vector3.forward);
        }
    }
}