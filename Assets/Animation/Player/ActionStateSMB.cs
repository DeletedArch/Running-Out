using UnityEngine;

public class ActionStateSMB : StateMachineBehaviour
{
    [Header("Player Capabilities")]
    [SerializeField] private bool lockMovement = false;
    [SerializeField] private bool isInvincible = false;
    [SerializeField, Range(0f, 1f)] private float invincibilityDuration = 0.75f;

    [Header("Cancel / Buffer Window")]
    [SerializeField] private bool hasCancelWindow = false;
    [Range(0f, 1f)]
    [SerializeField] private float cancelWindowStart = 0.5f;

    private PlayerController player;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (player == null) player = animator.GetComponent<PlayerController>();
        if (player == null) return;

        if (lockMovement) player.SetMovement(true);
        if (isInvincible) player.SetInvincible(true);
        if (hasCancelWindow) player.SetCanCancel(false);
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (player == null) return;

        if (hasCancelWindow && stateInfo.normalizedTime >= cancelWindowStart)
        {
            player.SetCanCancel(true);
        }

        if (isInvincible && stateInfo.normalizedTime >= invincibilityDuration)
        {
            player.SetInvincible(false);
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (player == null) return;

        if (lockMovement) player.SetMovement(false);
        if (isInvincible) player.SetInvincible(false);
        if (hasCancelWindow) player.SetCanCancel(false);
    }
}