using UnityEngine;

public class EvadeSMB : EnemyStateBehaviour
{
    [Header("Dodge Tuning")]
    [SerializeField] private float evadeSpeed = 8.5f;
    [SerializeField] private float maxEvadeDistance = 1.3f;
    [SerializeField] private float maxEvadeDuration = 0.35f; // Safety fallback timeout

    private float startX;
    private float timer;
    private bool reachedMaxDistance;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        reachedMaxDistance = false;
        startX = Controller.transform.position.x;
        timer = 0f;

        Transform target = Context.Target ?? Context.perception?.CurrentTarget;
        if (target != null && Context.enemyMovement != null)
        {
            Context.enemyMovement.FaceTarget(target.position);
        }
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        timer += Time.deltaTime;

        if (reachedMaxDistance)
        {
            EndEvade(animator);
            return;
        }

        // 1. Edge check behind
        bool hasGroundBehind = Context.perception != null && Context.perception.HasGroundBehind();
        if (!hasGroundBehind)
        {
            EndEvade(animator);
            return;
        }

        // 2. Distance check
        float distanceTraveled = Mathf.Abs(Controller.transform.position.x - startX);
        if (distanceTraveled >= maxEvadeDistance)
        {
            EndEvade(animator);
            return;
        }

        // 3. Timeout check (prevents getting stuck against walls or obstacles)
        if (timer >= maxEvadeDuration)
        {
            EndEvade(animator);
            return;
        }

        Context.enemyMovement.StepBack(evadeSpeed);
    }

    private void EndEvade(Animator animator)
    {
        reachedMaxDistance = true;
        Context?.enemyMovement?.Stop();
        animator.SetBool("Evade", false);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateExit(animator, stateInfo, layerIndex);
        Context?.enemyMovement?.Stop();
        animator.SetBool("Evade", false);
    }
}