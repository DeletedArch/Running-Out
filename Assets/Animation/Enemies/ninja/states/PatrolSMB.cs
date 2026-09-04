using UnityEngine;

public class PatrolSMB : EnemyStateBehaviour
{
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private EnemyController controller;
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        controller = animator.GetComponent<EnemyController>();
    }
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //if (Context == null)
            //return;
        // Look for player
        if (Context.channel.GetBestTarget(0f)!=null)
        {
            animator.SetBool("PlayerSpotted", true);
            return;
        }
        // Check for ledge / edge of platform
        if (!Context.perception.HasGroundAhead())
        {
            if (Context.edgeResponse != null)
            {
                Context.edgeResponse.OnEdgeDetected();
            }
            return;
        }
   
        Context.enemyMovement.Move(patrolSpeed);
    }
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (Context != null)
            Context.enemyMovement.Stop();
    }
}
