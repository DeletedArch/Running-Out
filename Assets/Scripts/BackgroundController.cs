using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct BackgroundParallaxLayer
{
    public MeshRenderer layerRenderer;
    public float parallaxEffect;
}

public class BackgroundController : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private List<BackgroundParallaxLayer> parallaxLayers;
    private Transform initialTransform;
    void Start()
    {
        initialTransform = cameraTransform;
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < parallaxLayers.Count; i++)
        {
            UpdateParallaxLayer(parallaxLayers[i]);
        }
    }

    void UpdateParallaxLayer(BackgroundParallaxLayer layer)
    {
        Vector3 deltaMovement = cameraTransform.position - initialTransform.position;
        Vector3 newPosition = (transform.position + deltaMovement) * layer.parallaxEffect;
        newPosition = new Vector3(newPosition.x, 0, newPosition.z);
        layer.layerRenderer.material.mainTextureOffset = (Vector2)newPosition;
    }
}
