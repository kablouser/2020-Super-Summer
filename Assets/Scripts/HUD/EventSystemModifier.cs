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

    public InputActionReference FirstOptionAction;
    public InputActionReference SecondOptionAction;

    private List<RaycastResult> raycastResults;
    private PointerEventData pointerData;
    private List<Panel> panels;

    public override void Awake()
    {
        base.Awake();

        Hovered = null;

        raycastResults = new List<RaycastResult>();
        pointerData = new PointerEventData(EventSystem.current);

        panels = new List<Panel>(1);
    }

    public void EnablePanel(Panel panel) =>
        panels.Add(panel);

    public void DisablePanel(Panel panel) =>
        panels.Remove(panel);

    private void OnEnable()
    {
        UpdateDevice(playerInput.devices[0]);
        InputUser.onChange += OnChange;

        inputModule.move.action.performed += OnMove;
        inputModule.leftClick.action.performed += OnClick;
        inputModule.cancel.action.performed += OnCancel;
        inputModule.submit.action.performed += OnSubmit;
        
        FirstOptionAction.action.Enable();
        SecondOptionAction.action.Enable();
        FirstOptionAction.action.performed += OnFirstOption;
        SecondOptionAction.action.performed += OnSecondOption;
    }

    private void OnDisable()
    {
        InputUser.onChange -= OnChange;

        inputModule.move.action.performed -= OnMove;
        inputModule.leftClick.action.performed -= OnClick;
        inputModule.cancel.action.performed -= OnCancel;
        inputModule.submit.action.performed -= OnSubmit;

        FirstOptionAction.action.Disable();
        SecondOptionAction.action.Disable();
        FirstOptionAction.action.performed -= OnFirstOption;
        SecondOptionAction.action.performed -= OnSecondOption;
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
        if (panels.Count == 0)
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
        Debug.Log(string.Format("{0} {1} {2}", context.phase, context.ReadValueAsButton(), context.canceled));
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
        SetMouse(device is Mouse || device is Keyboard);
    }

    private void SetMouse(bool isUsing)
    {
        IsUsingMouse = isUsing; 

        Cursor.visible = isUsing;
        Cursor.lockState = isUsing ? CursorLockMode.None : CursorLockMode.Confined;

        if(isUsing)
        {
            //update our mouse's position
        }
        else
        {
            //unhover current hovered
            Hovered = GetHovered();
            Unhover();
        }
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
