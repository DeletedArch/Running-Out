using UnityEngine;

public class OnEdgeResponse : MonoBehaviour, IEdgeResponse
{
    [SerializeField] private Animator animator;
    private void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
    }
    public void OnEdgeDetected()
    {
        // When edge is detected, enter Idle to pause!
        animator.SetBool("isIdle", true);
    }
}
