using UnityEngine;

public class DieSMB : EnemyStateBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        if (Context != null)
        {
            Context.enemyMovement.Stop();
            // 1. Freeze Rigidbody
            if (Context.rb != null)
            {
                Context.rb.linearVelocity = Vector2.zero;
                Context.rb.bodyType = RigidbodyType2D.Kinematic;
            }
            // 2. Disable colliders so player dashes freely through the dead body
            Collider2D[] colliders = animator.GetComponentsInChildren<Collider2D>();
            foreach (var col in colliders) col.enabled = false;
            // 3. Disappear after death animation
            Destroy(animator.gameObject, 2.5f);
        }
    }
}
