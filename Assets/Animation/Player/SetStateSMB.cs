using UnityEngine;
using System;

public class SetStateSMB : StateMachineBehaviour
{
    [SerializeField] private string stateTag = "SetState";
    public string StateTag => stateTag;
    public static event Action<string> OnStateExited;
    public static event Action<string> OnStateEntered;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        OnStateEntered?.Invoke(stateTag);
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        OnStateExited?.Invoke(stateTag);
    }
}