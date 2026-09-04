using UnityEngine;

public class ChaseSMB : EnemyStateBehaviour
{
    [SerializeField] private float chaseSpeed = 5f;
    [SerializeField] private float attackRange = 1.3f;

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (Context == null) return;
        TargetData targetData = Context.channel != null ? Context.channel.GetBestTarget(0f) : null;
        if (targetData != null && targetData.Object != null)
        {
            Context.enemyMovement.FaceTarget(targetData.Object.transform.position);

            float distance = Vector2.Distance(Controller.transform.position, targetData.Object.transform.position);

            if (!Context.perception.HasGroundAhead())
            {
                Context.enemyMovement.Stop();
                if (distance <= attackRange)
                {
                    animator.SetBool("Attack", true);
                }

                return;
            }

            // 3. Normal Ground Chase
            if (distance <= attackRange)
            {
                Context.enemyMovement.Stop();
                animator.SetBool("Attack", true);
            }
            else
            {
                animator.SetBool("Attack", false);
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