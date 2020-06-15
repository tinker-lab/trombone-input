﻿using UnityEngine;

#pragma warning disable 649
public class StylusModelController : MonoBehaviour
{
    [SerializeField]
    private GameObject potentiometerIndicator;

    [SerializeField]
    private MeshRenderer frontButtonRenderer, backButtonRenderer;

    private bool highlightingFront, highlightingBack;

    [SerializeField]
    private Material highlightMaterial, defaultMaterial;

    [SerializeField]
    private Vector3 min, max;

    public float normalizedX { get; private set; }
    public float normalizedZ { get; private set; }

    public (Vector3 origin, Vector3 direction) orientation { get; private set; }

    public float? normalizedSlider
    {
        get
        {
            if (potentiometerIndicator.activeInHierarchy)
            {
                return Mathf.InverseLerp(min.y, max.y, potentiometerIndicator.transform.localPosition.y);
            }
            return null;
        }

        set
        {
            if (!value.HasValue)
            {
                potentiometerIndicator.SetActive(false);
                return;
            }

            if (!potentiometerIndicator.activeInHierarchy)
            {
                potentiometerIndicator.SetActive(true);
            }

            var pos = potentiometerIndicator.transform.localPosition;
            pos.y = Mathf.Lerp(min.y, max.y, value.Value);
            potentiometerIndicator.transform.localPosition = pos;
        }
    }

    public bool frontButtonDown
    {
        get => highlightingFront;
        set
        {
            highlightingFront = value;
            frontButtonRenderer.material =
                value ?
                    highlightMaterial :
                    defaultMaterial;
        }
    }

    public bool backButtonDown
    {
        get => highlightingBack;
        set
        {
            highlightingBack = value;
            backButtonRenderer.material =
                value ?
                    highlightMaterial :
                    defaultMaterial;
        }
    }

    private void Start()
    {
        normalizedSlider = null;
        frontButtonDown = false;
        backButtonDown = false;

        UpdateOrientation();
    }

    void Update()
    {
        Vector3 euler = transform.rotation.eulerAngles;
        var x = Utils.ModIntoRange(euler.x, -180, 180);
        var z = Utils.ModIntoRange(euler.z, -180, 180);
        normalizedX = Mathf.InverseLerp(min.x, max.x, x);
        normalizedZ = Mathf.InverseLerp(min.z, max.z, z);

        UpdateOrientation();
    }

    private void UpdateOrientation()
    {
        // TODO: when stylus is fixed, will be Vector3.forward
        var direction = transform.rotation * Vector3.down;
        orientation = (transform.position, direction);
        Debug.DrawRay(orientation.origin, orientation.direction, Color.cyan, 0.5f);
    }
}
