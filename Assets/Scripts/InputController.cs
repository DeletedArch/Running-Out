using UnityEngine;
using System;

public enum InputMode
{
    Gameplay,
    UI,
    Cutscene,

}

public class InputController : MonoBehaviour
{
    internal InputSystem controls;
    internal InputMode currentInputMode = InputMode.Gameplay;
    public static event Action<Vector2> OnMoveInput;
    public static event Action OnDashInput;
    public static event Action OnSwiftDashInput;
    public static event Action OnJumpStart;
    public static event Action OnAttackStart;
    public static event Action OnAttackEnd;
    public static event Action OnBlockStart;
    public static event Action OnBlockEnd;

    // Debug Inputs
    public bool enableDebugInputs = true;
    public static event Action OnDebugInput1;
    public static event Action OnDebugInput2;
    public static event Action OnDebugInput3;

    void OnEnable()
    {
        controls?.Movement.Enable();
        controls?.Combat.Enable();
        if (enableDebugInputs)
        {
            controls?.Debug.Enable();
        }
    }

    void OnDisable()
    {
        controls?.Movement.Disable();
        controls?.Combat.Disable();
        if (enableDebugInputs)
        {
            controls?.Debug.Disable();
        }
    }

    void OnDestroy()
    {
        controls?.Dispose();
        controls = null;
    }

    void Awake()
    {
        controls = new InputSystem();
        // Movement
        controls.Movement.Jump.performed += ctx => OnJumpStart?.Invoke();
        controls.Movement.Move.performed += ctx => OnMoveInput?.Invoke(ctx.ReadValue<Vector2>());
        controls.Movement.Move.canceled += ctx => OnMoveInput?.Invoke(Vector2.zero);
        controls.Movement.Dash.performed += ctx => OnDashInput?.Invoke();
        // Combat
        controls.Combat.SwiftDash.performed += ctx => OnSwiftDashInput?.Invoke();
        controls.Combat.Attack.performed += ctx => OnAttackStart?.Invoke();
        controls.Combat.Attack.canceled += ctx => OnAttackEnd?.Invoke();
        controls.Combat.Block.performed += ctx => OnBlockStart?.Invoke();
        controls.Combat.Block.canceled += ctx => OnBlockEnd?.Invoke();
        // Debug
        controls.Debug.Debug1.performed += ctx => OnDebugInput1?.Invoke();
        controls.Debug.Debug2.performed += ctx => OnDebugInput2?.Invoke();
        controls.Debug.Debug3.performed += ctx => OnDebugInput3?.Invoke();
    }

    public void SetInputMode(InputMode mode)
    {
        currentInputMode = mode;
        if (enableDebugInputs)
        {
            controls.Debug.Enable();
        }
        switch (mode)
        {
            case InputMode.Gameplay:
                controls.Movement.Enable();
                controls.Combat.Enable();
                // controls.UI.Disable();
                break;
            case InputMode.UI:
                controls.Movement.Disable();
                controls.Combat.Disable();
                //controls.UI.Enable();
                break;
            case InputMode.Cutscene:
                controls.Movement.Disable();
                controls.Combat.Disable();
                // controls.UI.Enable();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
        }
    }
}
