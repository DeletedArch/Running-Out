using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class GetHitSMB : EnemyStateBehaviour
{
    [SerializeField] private float flashDuration = 0.25f;
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        if (Context != null) Context.enemyMovement.Stop();
        SpriteColorFlash(Color.red, flashDuration);
    }

    void SpriteColorFlash(Color color, float duration)
    {
        if (Context == null || Context.spriteRenderer == null) return;
        Context.spriteRenderer.color = color;
        UniTask.Delay(TimeSpan.FromSeconds(duration)).ContinueWith(() =>
        {
            Context.spriteRenderer.color = Color.white;
        }).Forget();
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateUpdate(animator, stateInfo, layerIndex);
        //if (stateInfo.normalizedTime >= 0.5f)
        //{
        //animator.SetBool("isHit", false);
        //}
    }
}