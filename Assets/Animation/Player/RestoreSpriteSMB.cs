using UnityEngine;

public class RestoreSpriteSMB : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        PlayerController player = animator.GetComponent<PlayerController>();
        if (player != null)
        {
            PlayerContext context = player.Context;
            context.playerSpriteRenderer.transform.localPosition = new Vector3(0f, context.playerSpriteRenderer.transform.localPosition.y, context.playerSpriteRenderer.transform.localPosition.z);
        }
    }
}