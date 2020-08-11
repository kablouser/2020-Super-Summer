using UnityEngine;
using UnityEngine.InputSystem;
using static Fighter;

public class PlayerControl : MonoBehaviour
{
    public const float InteractionRange = 1f;

    private struct QueuedAction
    {
        public bool isNew;
        public AbilityIndex index;
        public QueuedAction(AbilityIndex index)
        {
            isNew = true; this.index = index;
        }
    }
    private enum InteractType { droppedItem }
    private struct InteractCache
    {
        public InteractType type;
        public Object interactObject;
        public InteractCache(InteractType type, Object interactObject)
        {
            this.type = type;
            this.interactObject = interactObject;
        }
    }

    public PlayerComponents playerComponents;
    public PlayerHUDLinker.HUDInterface hudObjects;
    public LayerMask interactionMask;
    public ChaseCamera chaseCamera;

    private Movement movement;
    private Equipment equipment;
    private Fighter fighter;
    private PlayerInput playerInput;
    private CharacterSheet characterSheet;
    
    private Vector2 currentLook;
    private Vector2 inputLook;

    private QueuedAction downAction;

    private InteractCache interactCache;

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
    public void Interact(InputAction.CallbackContext context)
    {        
        if (context.phase != InputActionPhase.Performed)
            return;

        if (interactCache.interactObject == null)
            return;

        switch(interactCache.type)
        {
            default:
                Debug.LogWarning("Unimplemented interaction type", this);
                return;
            case InteractType.droppedItem:
                ((DroppedItem)interactCache.interactObject).Pickup(equipment);
                return;

            //add other sorts of interactions here ...
        }
    }
    public void Menu(InputAction.CallbackContext context)
    {
        if (context.phase != InputActionPhase.Performed)
            return;

        SetAllPanels(!hudObjects.EscapePanel.activeSelf);
    }

    private void ProcessHandInput(AbilityIndex index, InputActionPhase phase)
    {
        if (downAction.isNew && index < downAction.index)
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
        characterSheet = playerComponents.characterSheet;
        hudObjects.resourceBars.Setup(characterSheet);
    }
    private void Start()
    {
        equipment.AutoEquip();
    }
    private void OnEnable()
    {
        playerInput.enabled = true;
        currentLook.y = movement.GetCurrentAngleY;
        SetAllPanels(false);
    }
    private void OnDisable()
    {
        movement.SetMove(0, 0);
        playerInput.enabled = false;
        //make sure resource bars are correct when killed
        hudObjects.resourceBars.UpdateVisuals();

        //close inventory when killed
        SetEscapePanel(false);
        //auto open menu panel though
        SetMenuPanel(true, true);
        EventSystemModifier.Current.SetMenuMode(true);
    }
    private void Update()
    {
        currentLook += inputLook;
        movement.SetLook(ref currentLook.x, currentLook.y);

        if (downAction.isNew)
        {
            downAction.isNew = false;
            fighter.UseAbility(downAction.index, true, out _);
        }
    }
    private void FixedUpdate()
    {
        hudObjects.resourceBars.UpdateVisuals();

        if (Physics.Raycast(chaseCamera.transform.position, chaseCamera.transform.forward,
            out RaycastHit hitInfo, InteractionRange + chaseCamera.CurrentDistance, interactionMask) == false)
        {
            interactCache.interactObject = null;
            hudObjects.interactionPanel.Hide();
            return;
        }

        Collider hitCollider = hitInfo.collider;
        DroppedItem droppedItem;
        if (droppedItem = hitCollider.GetComponent<DroppedItem>())
        {
            InteractCache newCache = new InteractCache(InteractType.droppedItem, droppedItem);
            if (HasCacheChanged(newCache) == false)
                return;
            else
                interactCache = newCache;

            hudObjects.interactionPanel.SetText("Pick Up "+droppedItem.GetItem.name);
        }

        //add other sorts of interactions here ...
    }

    private bool HasCacheChanged(InteractCache newCache) =>
        newCache.type != interactCache.type || newCache.interactObject != interactCache.interactObject;

    private void SetAllPanels(bool isActive)
    {
        SetEscapePanel(isActive);
        SetMenuPanel(isActive, false);
        EventSystemModifier.Current.SetMenuMode(isActive);
    }

    private void SetEscapePanel(bool isActive)
    {
        if(hudObjects.EscapePanel != null)
            hudObjects.EscapePanel.SetActive(isActive);
    }

    private void SetMenuPanel(bool isActive, bool showMenuButtons)
    {
        if (hudObjects.MenuPanel != null)
        {
            hudObjects.MenuPanel.gameObject.SetActive(isActive);
            hudObjects.MenuPanel.ShowMenuButtons(showMenuButtons);
        }
    }
}
