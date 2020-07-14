using UnityEngine;
using UnityEngine.InputSystem;
using static Fighter;

public class PlayerControl : MonoBehaviour
{
    private struct QueuedAction
    {
        public bool isNew;
        public AbilityIndex index;
        public QueuedAction(AbilityIndex index)
        {
            isNew = true; this.index = index;
        }
    }

    public PlayerComponents playerComponents;

    private Movement movement;
    private Equipment equipment;
    private Fighter fighter;
    private PlayerInput playerInput;
    
    private Vector2 currentLook;
    private Vector2 inputLook;

    private QueuedAction downAction;

    public void Move(InputAction.CallbackContext context)
    {
        var readVector = context.ReadValue<Vector2>();
        movement.SetMove(readVector.x, readVector.y);
    }
    public void Look(InputAction.CallbackContext context)
    {
        var readVector = context.ReadValue<Vector2>();
        inputLook.x = -readVector.y;
        inputLook.y = readVector.x;
    }
    public void L2(InputAction.CallbackContext context)
    {
        ProcessHandInput(AbilityIndex.L2, context.phase);
    }
    public void L1(InputAction.CallbackContext context)
    {
        ProcessHandInput(AbilityIndex.L1, context.phase);
    }
    public void R2(InputAction.CallbackContext context)
    {
        ProcessHandInput(AbilityIndex.R2, context.phase);
    }
    public void R1(InputAction.CallbackContext context)
    {
        ProcessHandInput(AbilityIndex.R1, context.phase);
    }    
    private void ProcessHandInput(AbilityIndex index, InputActionPhase phase)
    {
        if(downAction.isNew && index < downAction.index)
            return;

        if (phase == InputActionPhase.Started)
            downAction = new QueuedAction(index);
        else if (phase == InputActionPhase.Canceled)
            fighter.UseAbility(index, false, out _);
    }
    private void Awake()
    {
        movement = playerComponents.movement;
        equipment = playerComponents.equipment;
        fighter = playerComponents.fighter;
        playerInput = playerComponents.playerInput;
    }
    private void Start()
    {
        equipment.AutoEquip();
    }
    private void OnEnable()
    {
        playerInput.enabled = true;
    }
    private void OnDisable()
    {
        movement.SetMove(0, 0);
        playerInput.enabled = false;
    }
    private void Update()
    {
        currentLook += inputLook;
        movement.SetLook(ref currentLook.x, currentLook.y);

        if(downAction.isNew)
        {
            downAction.isNew = false;
            fighter.UseAbility(downAction.index, true, out _);
        }
    }
}
