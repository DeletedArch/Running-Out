using UnityEngine;

public class FloorController : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private MeshRenderer backgroundMaterial;
    [SerializeField] private float parallaxEffect = 0.25f;
    private Transform initialTransform;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        initialTransform = transform;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 deltaMovement = cameraTransform.position - initialTransform.position;
        Vector3 newPosition = (transform.position + deltaMovement) * parallaxEffect;
        newPosition = new Vector3(newPosition.x, 0, newPosition.z);
        backgroundMaterial.material.mainTextureOffset = (Vector2)newPosition;
        transform.position = new Vector3(cameraTransform.position.x, transform.position.y, transform.position.z);

    }
}
