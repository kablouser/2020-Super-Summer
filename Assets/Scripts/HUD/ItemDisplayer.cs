using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using TMPro;

using System.Collections;

public class ItemDisplayer : MonoBehaviour, 
    ISelectHandler, ISubmitHandler, IPointerClickHandler, 
    IPointerEnterHandler, IDragHandler, ICancelHandler,
    IEndDragHandler, IDeselectHandler, IMoveHandler
{
    public EquipmentDisplayer master;

    public int inventoryIndex;
    public Image frameDisplay;
    public Image iconDisplay;
    public TextMeshProUGUI countDisplay;
    public Selectable selectable;   

    public RectTransform rectTransform { get; private set; }

    private Coroutine bobbingRoutine;

    private void Awake()
    {
        rectTransform = (RectTransform)transform;
    }
    
    private IEnumerator BobbingRoutine()
    {
        int increasingSize = 1;
        float increaseSpeed = 0.4f;
        float deltaSize = 0.1f;

        do
        {
            rectTransform.localScale += increasingSize * new Vector3(1, 1, 0) * Time.deltaTime * increaseSpeed;

            if (increasingSize == 1 && (1 + deltaSize) < rectTransform.localScale.x)
                increasingSize = -1;
            else if (increasingSize == -1 && rectTransform.localScale.x < (1 - deltaSize))
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
            rectTransform.localScale = Vector3.one;
        }
    }

    //interfaces

    //selection
    public void OnSelect(BaseEventData eventData)
    {
        master.FocusScroll(this);
        master.SelectEntry(inventoryIndex);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        master.SelectEntry(inventoryIndex);
    }

    //confirm button(s) - can either show controls or end dragging
    public void OnSubmit(BaseEventData eventData)
    {
        master.DisplayerOnSubmit(this, false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {        
        if (eventData.dragging == false &&
            eventData.button == PointerEventData.InputButton.Left)
            master.DisplayerOnSubmit(this, true);
    }

    //drag and dropping
    public void OnDrag(PointerEventData eventData)
    {
        master.BeginDrag(inventoryIndex, true);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        master.EndDrag();
    }

    //deselect
    public void OnDeselect(BaseEventData eventData)
    {
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
}
