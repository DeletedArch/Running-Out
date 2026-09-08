using UnityEngine;

public class WallHoldStateSMB : StateMachineBehaviour
{
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        int direction = animator.GetInteger("TouchingWall");
        animator.transform.localScale = new Vector3(-direction * Mathf.Abs(animator.transform.localScale.x), animator.transform.localScale.y, animator.transform.localScale.z);
    }

}
