using UnityEngine;

public class ChaseSMB : EnemyStateBehaviour
{
    [SerializeField] private float chaseSpeed = 5f;
    [SerializeField] private float attackRange = 1.5f;

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (Context == null) 
            return;

        if (Context.perception != null && Context.perception.TryFindPlayer(out Transform target))
        {
            Context.enemyMovement.FaceTarget(target.transform.position);
            float distance = Vector2.Distance(Controller.transform.position, target.transform.position);

            if (!Context.perception.HasGroundAhead())
            {
                Context.enemyMovement.Stop();
                if (distance <= attackRange)
                {
                    animator.SetTrigger("Attack");
                }
                return;
            }

            // 3. Normal Ground Chase
            if (distance <= attackRange)
            {
                Context.enemyMovement.Stop();
                animator.SetTrigger("Attack");
            }
            else
            {
                Context.enemyMovement.Move(chaseSpeed);
            }
        }
        else
        {
            animator.SetBool("PlayerSpotted", false);
        }
    }
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (Context != null) Context.enemyMovement.Stop();
    }
}