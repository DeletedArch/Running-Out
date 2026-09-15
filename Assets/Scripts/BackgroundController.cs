using UnityEngine;

public class BackgroundController : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private MeshRenderer backgroundMaterial;
    [SerializeField] private float parallaxEffect = 0.25f;
    private Transform initialTransform;
    void Start()
    {
        initialTransform = cameraTransform;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 deltaMovement = cameraTransform.position - initialTransform.position;
        Vector3 newPosition = (transform.position + deltaMovement) * parallaxEffect;
        newPosition = new Vector3(newPosition.x, 0, newPosition.z);
        backgroundMaterial.material.mainTextureOffset = (Vector2)newPosition;
    }
}
