using UnityEngine;
public abstract class EnemyStateBehaviour : StateMachineBehaviour
{
    protected EnemyController Controller { get; private set; }

    protected EnemyContext Context => Controller != null ? Controller.Context : null;
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
    
        if (Controller == null)
        {
            Controller = animator.GetComponent<EnemyController>();
        }
    }
}