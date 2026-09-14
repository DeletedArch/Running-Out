using UnityEngine;

public class WallHoldStateSMB : StateMachineBehaviour
{
    float originalGravityScale = 1f;
    [SerializeField] float reductionFactor = 0.75f;
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        originalGravityScale = animator.GetComponent<Rigidbody2D>().gravityScale;
        animator.GetComponent<Rigidbody2D>().gravityScale *= reductionFactor;

        int direction = animator.GetInteger("TouchingWall");
        animator.transform.localScale = new Vector3(-direction * Mathf.Abs(animator.transform.localScale.x), animator.transform.localScale.y, animator.transform.localScale.z);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponent<Rigidbody2D>().gravityScale = originalGravityScale;
    }

}
