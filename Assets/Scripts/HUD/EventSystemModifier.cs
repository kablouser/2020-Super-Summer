using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.InputSystem.UI;

using System.Collections.Generic;

public class EventSystemModifier : Singleton<EventSystemModifier>
{
    /// <summary>
    /// Special case navigation that can ignore some checks for resetting the selected object.
    /// Example use :: move item displayer around with arrow keys with navigation disabled but don't deselect that object.
    /// </summary>
    public interface ISpecialNavigation
    {
        bool HasNavigation();
    }

    public interface IFirstOptionHandler
    {
        void OnFirstOption();
    }

    public interface ISecondOptionHandler
    {
        void OnSecondOption();
    }

    public Selectable Hovered { get; private set; }
    public bool IsUsingMouse { get; private set; }

    public PlayerInput playerInput;
    public InputSystemUIInputModule inputModule;

    public InputActionReference firstOptionAction;
    public InputActionReference secondOptionAction;

    public InputActionReference[] disableActionsInMenu;

    private List<RaycastResult> raycastResults;
    private PointerEventData pointerData;
    private List<Panel> panels;

    private float enabledFrame;

    public override void Awake()
    {
        base.Awake();

        Hovered = null;

        raycastResults = new List<RaycastResult>();
        pointerData = new PointerEventData(EventSystem.current);

        panels = new List<Panel>(1);
    }

    public void EnablePanel(Panel panel)
    {
        enabledFrame = Time.time;
        panels.Add(panel);
    }

    public void DisablePanel(Panel panel) =>
        panels.Remove(panel);

    public void SetMouseVisible(bool isVisible)
    {
        Cursor.visible = isVisible;
        Cursor.lockState = isVisible ? CursorLockMode.None : CursorLockMode.Locked;
    }

    public void SetMenuMode(bool inMenu)
    {
        if (IsUsingMouse)
        {
            SetMouseVisible(inMenu);

            //update disableActionsInMenu
            for (int i = 0; i < disableActionsInMenu.Length; i++)
                if (disableActionsInMenu[i] != null)
                {
                    if(inMenu)
                        disableActionsInMenu[i].action.Disable();
                    else
                        disableActionsInMenu[i].action.Enable();
                }
        }
    }

    private void OnEnable()
    {
        UpdateDevice(playerInput.devices[0]);
        InputUser.onChange += OnChange;

        inputModule.move.action.performed += OnMove;
        inputModule.leftClick.action.performed += OnClick;
        inputModule.cancel.action.performed += OnCancel;
        inputModule.submit.action.performed += OnSubmit;
        
        firstOptionAction.action.Enable();
        secondOptionAction.action.Enable();
        firstOptionAction.action.performed += OnFirstOption;
        secondOptionAction.action.performed += OnSecondOption;

        enabledFrame = Time.time;
    }

    private void OnDisable()
    {
        InputUser.onChange -= OnChange;

        inputModule.move.action.performed -= OnMove;
        inputModule.leftClick.action.performed -= OnClick;
        inputModule.cancel.action.performed -= OnCancel;
        inputModule.submit.action.performed -= OnSubmit;

        firstOptionAction.action.Disable();
        secondOptionAction.action.Disable();
        firstOptionAction.action.performed -= OnFirstOption;
        secondOptionAction.action.performed -= OnSecondOption;
    }

    private void Update()
    {
        if (IsUsingMouse && Mouse.current.leftButton.isPressed == false)
        {
            //select current hovered
            Hovered = GetHovered();
            if (Hovered != null)
                EventSystem.current.SetSelectedGameObject(Hovered.gameObject);
        }
    }

    private void OnChange(InputUser user, InputUserChange change, InputDevice device)
    {
        if (change == InputUserChange.DevicePaired)
            UpdateDevice(device);
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        Unhover();

        if (panels.Count == 0)
            return;

        GameObject selected = EventSystem.current.currentSelectedGameObject;
        if (selected == null ||
            selected.activeInHierarchy == false ||
            HasNavigation(selected) == false)
        {
            Selectable trySelect = panels[panels.Count - 1].SelectOnFocus;
            if (trySelect != null)
                trySelect.Select();
        }
    }

    private void OnClick(InputAction.CallbackContext context)
    {
        if (panels.Count == 0)
            return;

        MouseRaycast();

        for (int i = 0; i < panels.Count; i++)
            if (panels[i].HideOnClickOutside &&
                raycastResults.Exists(
                result => result.gameObject == panels[i].panelObject)
                == false)
            {
                panels[i].Hide();
                i--;
            }
    }

    private void OnCancel(InputAction.CallbackContext context)
    {
        if (panels.Count == 0 || enabledFrame == Time.time)
            return;

        panels[panels.Count - 1].Hide();
    }

    private void OnSubmit(InputAction.CallbackContext context)
    {
        if (panels.Count == 0)
            return;

        panels[panels.Count - 1].OnSubmit();
    }

    private void OnFirstOption(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            GameObject selected = EventSystem.current.currentSelectedGameObject;
            if (selected != null)
            {
                IFirstOptionHandler firstOption = selected.GetComponent<IFirstOptionHandler>();
                if (firstOption != null)
                    firstOption.OnFirstOption();
            }
        }
    }

    private void OnSecondOption(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            GameObject selected = EventSystem.current.currentSelectedGameObject;
            if (selected != null)
            {
                ISecondOptionHandler secondOption = selected.GetComponent<ISecondOptionHandler>();
                if (secondOption != null)
                    secondOption.OnSecondOption();
            }
        }
    }

    private void UpdateDevice(InputDevice device)
    {
        SetIsMouseUsing(device is Mouse || device is Keyboard);
    }

    private void SetIsMouseUsing(bool isUsing)
    {
        IsUsingMouse = isUsing;

        if (isUsing == false)
        {
            //unhover current hovered
            Hovered = GetHovered();
            Unhover();
        }

        SetMouseVisible(isUsing);
    }

    private Selectable GetHovered()
    {
        MouseRaycast();

        foreach (var result in raycastResults)
        {
            Selectable selectable = result.gameObject.GetComponent<Selectable>();
            if(selectable != null && selectable.IsInteractable())
                return selectable;
        }
        
        return null;
    }

    private void MouseRaycast()
    {
        raycastResults.Clear();
        pointerData.position = Mouse.current.position.ReadValue();
        EventSystem.current.RaycastAll(pointerData, raycastResults);
    }

    private void Unhover()
    {
        if (Hovered != null)
        {
            Hovered.OnPointerExit(null);
            Hovered = null;
        }
    }

    private bool HasNavigation(GameObject selectableObject)
    {
        ISpecialNavigation specialNavigation = selectableObject.GetComponent<ISpecialNavigation>();
        if(specialNavigation != null)
            return specialNavigation.HasNavigation();

        Navigation navigation = selectableObject.GetComponent<Selectable>().navigation;

        if (navigation.mode == Navigation.Mode.Automatic)
            return true;
        if (navigation.mode == Navigation.Mode.Explicit)
            return navigation.selectOnDown != null ||
                navigation.selectOnLeft != null ||
                navigation.selectOnRight != null ||
                navigation.selectOnUp != null;
        else
            return false;
    }
}
