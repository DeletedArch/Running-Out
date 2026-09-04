using UnityEngine;

public class PatrolSMB : EnemyStateBehaviour
{
    [SerializeField] private float patrolSpeed = 2f;
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (Context == null) 
            return;

        // check for ledge / edge of platform
        if (!Context.perception.HasGroundAhead())
        {
            Context.edgeResponse.OnEdgeDetected();
        }

        // look for the player
        if (Context.perception.TryFindPlayer(out Transform playerTransform))
        {
            //enter another state(chase /or attack)
            animator.SetBool("PlayerSpotted", true);
        }

        //move forward
        Context.enemyMovement.Move(patrolSpeed);
    }
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (Context != null)
            Context.enemyMovement.Stop();
    }
}
