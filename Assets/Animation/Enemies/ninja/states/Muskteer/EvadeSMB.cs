using UnityEngine;

public class EvadeSMB : EnemyStateBehaviour
{
    [SerializeField] private float evadeSpeed = 4.0f;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        Transform target = Context.Target ?? Context.perception?.CurrentTarget;
        if (target != null && Context.enemyMovement != null)
        {
            Context.enemyMovement.FaceTarget(target.position);
        }
    }
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

        // THE ONLY CONDITION YOU CARE ABOUT:
        // Is there ground behind his back foot
        bool hasGroundBehind = Context.perception != null && Context.perception.HasGroundBehind();

        if (!hasGroundBehind)
        {
            Context.enemyMovement.Stop();
            return;
        }
        Context.enemyMovement.StepBack(evadeSpeed);
    }


    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateExit(animator, stateInfo, layerIndex);
        Context?.enemyMovement?.Stop();
    }
}
