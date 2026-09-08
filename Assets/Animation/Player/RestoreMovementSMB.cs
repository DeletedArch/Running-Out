using UnityEngine;

public class RestoreMovementSMB : StateMachineBehaviour
{
    [SerializeField] private bool restoreMovement = true;

    private PlayerController player;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (player == null) player = animator.GetComponent<PlayerController>();
        if (player == null) return;

        if (restoreMovement) player.SetMovement(true);
    }
}