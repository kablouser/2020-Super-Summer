using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using TMPro;

using System.Collections;

using static EventSystemModifier;

public class ItemDisplayer : MonoBehaviour, 
    ISelectHandler, ISubmitHandler, IPointerClickHandler, 
    ICancelHandler, IBeginDragHandler, IScrollHandler,
    IEndDragHandler, IMoveHandler,
    ISpecialNavigation, IFirstOptionHandler, ISecondOptionHandler
{
    public InventoryPanel master;

    public Image frameDisplay;
    public Image iconDisplay;
    public TextMeshProUGUI countDisplay;
    public Selectable selectable;

    public bool isInventory;
    public int itemIndex;

    public RectTransform RectTransform { get; private set; }

    private Coroutine bobbingRoutine;

    private void Awake()
    {
        RectTransform = (RectTransform)transform;
    }

    private void OnEnable()
    {
        frameDisplay.raycastTarget = iconDisplay.raycastTarget = true;
    }

    private IEnumerator BobbingRoutine()
    {
        int increasingSize = 1;
        float increaseSpeed = 0.4f;
        float deltaSize = 0.1f;

        do
        {
            RectTransform.localScale += increasingSize * new Vector3(1, 1, 0) * Time.deltaTime * increaseSpeed;

            if (increasingSize == 1 && (1 + deltaSize) < RectTransform.localScale.x)
                increasingSize = -1;
            else if (increasingSize == -1 && RectTransform.localScale.x < (1 - deltaSize))
                increasingSize = 1;
            
            yield return null;
        }
        while (true);
    }

    public void SetIcon(Sprite spriteIcon)
    {
        iconDisplay.sprite = spriteIcon;
    }

    public void SetCount(int count)
    {
        if (count == 1)
            countDisplay.gameObject.SetActive(false);
        else
        {            
            countDisplay.SetText(count.ToString());
            countDisplay.gameObject.SetActive(true);
        }
    }

    public void SetNavigation(bool enabled)
    {
        var copy = selectable.navigation;
        copy.mode = enabled ? Navigation.Mode.Automatic : Navigation.Mode.None;

        selectable.navigation = copy;
    }

    public void SetBobbing(bool enabled)
    {
        if (bobbingRoutine != null)
            StopCoroutine(bobbingRoutine);

        if (enabled)
        {
            frameDisplay.maskable = iconDisplay.maskable = false;
            bobbingRoutine = StartCoroutine(BobbingRoutine());
        }
        else
        {
            frameDisplay.maskable = iconDisplay.maskable = true;
            RectTransform.localScale = Vector3.one;
        }
    }

    //interfaces
    //special navigation
    public bool HasNavigation() => true;

    //selection
    public void OnSelect(BaseEventData eventData)
    {
        master.SelectEntry(this);
    }

    //confirm button(s) - can either show controls or end dragging
    public void OnSubmit(BaseEventData eventData)
    {
        master.SubmitEntry(this, false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {        
        if (eventData.dragging == false &&
            eventData.button == PointerEventData.InputButton.Left)
            master.SubmitEntry(this, true);
    }

    //drag and dropping
    public void OnBeginDrag(PointerEventData eventData)
    {
        frameDisplay.raycastTarget = iconDisplay.raycastTarget = false;
        master.BeginDrag(this);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        frameDisplay.raycastTarget = iconDisplay.raycastTarget = true;
        master.EndDrag();
    }

    public void OnMove(AxisEventData eventData)
    {
        master.OnDisplayerMove(eventData.moveVector);
    }

    public void OnCancel(BaseEventData eventData)
    {
        master.EndDrag();
    }

    public void OnScroll(PointerEventData eventData)
    {
        if(isInventory)
            master.itemGridPanel.gridScrollRect.OnScroll(eventData);
    }

    public void OnFirstOption()
    {
        print("First Option");
    }

    public void OnSecondOption()
    {
        print("Second Option");
    }
}
