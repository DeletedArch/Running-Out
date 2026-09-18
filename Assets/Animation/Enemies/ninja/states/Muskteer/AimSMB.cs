using UnityEngine;

public class AimSMB : EnemyStateBehaviour
{
    [Header("Aim Timing")]
    [SerializeField] private float normalAimDuration = 1.0f;
    [SerializeField] private float panicAimDuration = 0.45f;
    [SerializeField] private float lockLeadTime = 0.15f;

    float timer;
    private float currentAimDuration;
    private bool isLocked;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        Context.enemyMovement.Stop();

        // look towards the player immediately 
        Transform target = Context.Target ?? Context.perception?.CurrentTarget;
        if (target != null)
        {
            Context.enemyMovement.FaceTarget(target.position);
            Context.AimLockedPosition = target.position;
        }

        // OR Panic Aim (if the player is too close)
        bool isCornered = !Context.perception.HasGroundBehind();
        currentAimDuration = isCornered ? panicAimDuration : normalAimDuration;

        timer = 0f;
        isLocked = false;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        timer += Time.deltaTime;
        Transform target = Context.Target ?? Context.perception?.CurrentTarget;
        if (target == null) 
            return;

        // 1. tracking the player
        if (timer < (currentAimDuration - lockLeadTime))
        {
            Context.enemyMovement.FaceTarget(target.position);
            Context.AimLockedPosition = target.position;
        }

        // 2. Aim locked
        else if (!isLocked)
        {
            isLocked = true;
        }

            // 3. FIRE
        if (timer >= currentAimDuration)
        {
            animator.SetTrigger("Shoot");
        }
    }
}
