using UnityEngine;
using System;

public class RunStateSMB : StateMachineBehaviour
{  
    float VFXTimer = 0.15f;
    float VFXElapsedTime = 0f;
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var player = animator.GetComponent<PlayerController>();
        if (player == null) return;
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        VFXElapsedTime += Time.deltaTime;
        if (VFXElapsedTime >= VFXTimer)
        {
            VFXEvents.TriggerVFX("Run", animator.transform.position, Quaternion.identity, flipX: animator.transform.localScale.x < 0);
            VFXElapsedTime = 0f; // Reset the timer
        }
    }
}