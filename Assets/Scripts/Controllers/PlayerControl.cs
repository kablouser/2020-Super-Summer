using UnityEngine;
using UnityEngine.InputSystem;
using static Fighter;

[RequireComponent(typeof(Movement))]
[RequireComponent(typeof(Equipment))]
[RequireComponent(typeof(Fighter))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerControl : MonoBehaviour
{
    private struct QueuedAction
    {
        public bool isNew;
        public AbilityIndex index;
        public bool buttonDown;
        public QueuedAction(AbilityIndex index, bool buttonDown)
        {
            isNew = true; this.index = index; this.buttonDown = buttonDown;
        }
    }

    private Movement movement;
    private Equipment equipment;
    private Fighter fighter;
    private PlayerInput playerInput;    
    
    private Vector2 currentLook;
    private Vector2 inputLook;

    private QueuedAction nextAction;

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
        if(nextAction.isNew && index < nextAction.index)
            return;

        if (phase == InputActionPhase.Started)
            nextAction = new QueuedAction(index, true);
        else if (phase == InputActionPhase.Canceled)
            nextAction = new QueuedAction(index, false);
    }
    private void Awake()
    {
        movement = GetComponent<Movement>();
        equipment = GetComponent<Equipment>();
        fighter = GetComponent<Fighter>();
        playerInput = GetComponent<PlayerInput>();
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

        if(nextAction.isNew)
        {
            nextAction.isNew = false;
            fighter.UseAbility(nextAction.index, nextAction.buttonDown, out _);
        }
    }
}
