using UnityEngine;
using UnityEngine.UI;

public class InventoryPanel : Panel
{
    public enum EndDragPosition { none, original, newPosition }

    public override bool HideOnClickOutside => false;
    public override Selectable SelectOnFocus => FindSelectOnFocus();
    public override Selectable SelectOnDefocus => null;

    public ItemDisplayer GetDragged { get; private set; }

    [Header("Object References")]
    public Equipment playerEquipment;
    public ObjectPool displayersPool;
    public Transform topLayer;

    [Header("Minor Panels")]
    public InformationPanel informationPanel;
    [Header("Major Panels")]
    public EquipmentPanel equipmentPanel;
    public ItemGridPanel itemGridPanel;

    public void OnDisplayerMove(Vector2 navigation)
    {
        itemGridPanel.OnDisplayerMove(navigation);
    }

    public void SelectEntry(ItemDisplayer displayer)
    {
        var item = displayer.isInventory ?
            playerEquipment.inventory[displayer.itemIndex].item :
            playerEquipment.GetArms(displayer.itemIndex).armsScriptable;

        informationPanel.SetItem(item);
        itemGridPanel.OnSelect(displayer);
    }

    public void SubmitEntry(ItemDisplayer displayer, bool isMouse)
    {
        if (GetDragged != null)
            EndDrag();
    }

    public void BeginDrag(ItemDisplayer newDragged)
    {
        if (GetDragged != null)
            EndDrag(EndDragPosition.original);

        GetDragged = newDragged;

        int startingIndex =
            GetDragged.isInventory ?
                GetDragged.itemIndex :
                itemGridPanel.displayersGrid.transform.childCount;        

        if (EventSystemModifier.Current.IsUsingMouse)
        {
            GetDragged.transform.SetParent(topLayer);
        }
        else
        {
            GetDragged.SetNavigation(false);
            GetDragged.selectable.Select();
            GetDragged.SetBobbing(true);
        }

        itemGridPanel.BeginDrag(startingIndex);
    }

    public void EndDrag(EndDragPosition endPosition = EndDragPosition.newPosition)
    {
        if (GetDragged == null)
            return;

        var copy = GetDragged;
        GetDragged = null;

        itemGridPanel.EndDrag(copy, endPosition);
    }

    public override void Hide()
    {
        base.Hide();
        if(EventSystemModifier.Current.IsUsingMouse)
            EventSystemModifier.Current.SetMouseVisible(false);
    }

    protected override void Show(RectTransform targetPosition)
    {
        base.Show(targetPosition);
        if (EventSystemModifier.Current.IsUsingMouse)
            EventSystemModifier.Current.SetMouseVisible(true);
    }

    private void OnEnable()
    {
        Show(null);
    }

    private void OnDisable()
    {
        Hide();
    }

    private void Update()
    {
        if (GetDragged != null)
        {
            itemGridPanel.OnDrag();
        }
    }

    private Selectable FindSelectOnFocus()
    {
        return itemGridPanel.SelectOnFocus();
    }
}
