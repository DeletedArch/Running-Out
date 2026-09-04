using UnityEngine;

public abstract class EnemyStateBehaviour : StateMachineBehaviour
{
    protected EnemyContext Context { get; private set; }
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(Context == null)
            Context = animator.GetComponent<EnemyContext>();
    }
}
