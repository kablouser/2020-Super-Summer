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
    public ItemControlPanel itemControlPanel;
    public DropControlPanel dropControlPanel;
    [Header("Major Panels")]
    public EquipmentPanel equipmentPanel;
    public ItemGridPanel itemGridPanel;

    private void Start()
    {
        Show();
    }

    public void Show()
    {
        base.Show(null);
    }

    public void OnDisplayerMove(Vector2 navigation)
    {
        itemGridPanel.OnDisplayerMove(navigation);
    }

    public void SelectEntry(ItemDisplayer displayer)
    {
        var item = playerEquipment.inventory[displayer.itemIndex].item;
        informationPanel.SetItem(item);
        itemGridPanel.OnSelect(displayer);
    }

    public void SubmitEntry(ItemDisplayer displayer, bool isMouse)
    {
        if (GetDragged == null)
            ShowControls(displayer, isMouse);
        else
            EndDrag();
    }

    public void BeginDrag(ItemDisplayer newDragged)
    {
        if (GetDragged != null)
            EndDrag(EndDragPosition.original);

        GetDragged = newDragged;
        itemGridPanel.BeginDrag();
    }

    public void EndDrag(EndDragPosition endPosition = EndDragPosition.newPosition)
    {
        if (GetDragged == null)
            return;

        var copy = GetDragged;
        GetDragged = null;

        itemGridPanel.EndDrag(copy, endPosition);
    }

    public void ShowControls(ItemDisplayer displayer, bool isMouse)
    {
        itemControlPanel.Show(
            displayer,
            new ItemControlPanel.ShowOptions()
            {
                showMove = true
            });
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
