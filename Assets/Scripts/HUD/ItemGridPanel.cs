using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

using static Inventory;
using static InventoryPanel;

public class ItemGridPanel : MonoBehaviour
{
    public InventoryPanel master;

    public RectTransform gridViewport;
    public ScrollRect gridScrollRect;
    public GridLayoutGroup displayersGrid;

    public Transform emptyPlaceholder;
    public Transform emptyPlaceholderStorage;

    private List<ItemDisplayer> displayers;
    private RectTransform displayersGridRect;

    private int draggedSiblingIndex;
    private bool draggedMouse;

    private bool draggedInside;

    public void OnDisplayerMove(Vector2 navigation)
    {
        if (master.GetDragged != null && draggedMouse == false)
        {
            Vector2 localPoint = master.GetDragged.RectTransform.anchoredPosition;

            //anchorPosition y is inverted
            localPoint.y = -localPoint.y;

            int x =
                GetIndexPosition(
                    displayersGrid.padding.left,
                    displayersGrid.cellSize.x,
                    displayersGrid.spacing.x,
                    displayersGridRect.rect.width,
                    localPoint.x,
                    out int xMax),
            y =
                GetIndexPosition(
                    displayersGrid.padding.top,
                    displayersGrid.cellSize.y,
                    displayersGrid.spacing.y,
                    displayersGridRect.rect.height,
                    localPoint.y,
                    out int yMax);

            int yBefore = y;

            x = Mathf.Clamp(x + Mathf.CeilToInt(navigation.x), 0, xMax);
            y = Mathf.Clamp(y - Mathf.CeilToInt(navigation.y), 0, yMax);

            draggedSiblingIndex = Mathf.Min(y * (xMax + 1) + x, displayers.Count - 1);
            master.GetDragged.transform.SetSiblingIndex(draggedSiblingIndex);

            if (yBefore != y)
                FocusScroll(master.GetDragged, y - yBefore);
        }
    }

    /// <summary>
    /// Make sure the selected item displayers is within the scroll view. Adjust the scroll view as necessary.
    /// </summary>
    public void FocusScroll(ItemDisplayer selected, int levelShift = 0)
    {
        RectTransform selectedRect = selected.RectTransform;
        bool instantSnap;
        float displayerTop;

        //anchor y positive goes up, negative down
        if (selected == master.GetDragged && draggedMouse)
        {
            if (selectedRect.position.x < displayersGridRect.position.x || 
                displayersGridRect.position.x + gridViewport.rect.width < selectedRect.anchoredPosition.x)
                return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                displayersGridRect, selectedRect.position, null, out Vector2 localPoint);

            localPoint.y += selectedRect.rect.height / 2.0f;

            float anchorY = displayersGridRect.anchoredPosition.y;

            displayerTop = Mathf.Clamp(
                anchorY + localPoint.y,
                anchorY - displayersGridRect.rect.height,
                anchorY);

            //slowly scroll towards the selected
            instantSnap = false;
        }
        else
        {
            displayerTop =
                displayersGridRect.anchoredPosition.y +
                selectedRect.anchoredPosition.y +
                selectedRect.rect.height / 2.0f -
                levelShift * (displayersGrid.cellSize.y + displayersGrid.spacing.y);

            //snap to the selected instantly
            instantSnap = true;
        }

        if (0 < displayerTop)
        {
            //selected is above the viewport

            Vector2 gridPosition = displayersGridRect.anchoredPosition;

            if (instantSnap)
                gridPosition.y -= displayerTop + displayersGrid.padding.top;
            else
                gridPosition.y -= gridScrollRect.scrollSensitivity * Time.deltaTime * displayersGrid.cellSize.y;

            displayersGridRect.anchoredPosition = gridPosition;
            return;
        }

        //this is negative
        float displayerBottom = -(displayerTop - selectedRect.rect.height);
        float viewPortHeight = gridViewport.rect.height;

        if (viewPortHeight < displayerBottom)
        {
            //selected is bellow the bottom of the viewport

            Vector2 gridPosition = displayersGridRect.anchoredPosition;

            if (instantSnap)
                gridPosition.y += displayerBottom - viewPortHeight + displayersGrid.padding.bottom;
            else
                gridPosition.y += gridScrollRect.scrollSensitivity * Time.deltaTime * displayersGrid.cellSize.y;

            displayersGridRect.anchoredPosition = gridPosition;
            return;
        }
    }

    public void BeginDrag(int startingIndex)
    {
        draggedMouse = EventSystemModifier.Current.IsUsingMouse;
        draggedInside = master.GetDragged.isInventory;

        if (draggedMouse)
        {
            emptyPlaceholder.SetParent(displayersGrid.transform);
            emptyPlaceholder.SetSiblingIndex(startingIndex);
        }

        draggedSiblingIndex = startingIndex;
    }

    public void EndDrag(ItemDisplayer draggedCopy, EndDragPosition endPosition)
    {
        if (draggedMouse)
        {
            emptyPlaceholder.SetParent(emptyPlaceholderStorage);

            if(draggedCopy.isInventory)
                draggedCopy.transform.SetParent(displayersGrid.transform);
        }
        else
        {
            draggedCopy.SetBobbing(false);
            draggedCopy.SetNavigation(true);
            draggedCopy.selectable.Select();
        }

        if (endPosition == EndDragPosition.newPosition)
        {
            var equipment = master.playerEquipment;

            if (draggedCopy.isInventory)
                //this will induce further callbacks which corrects the dragged sibling index
                equipment.Reorder(draggedCopy.itemIndex, draggedSiblingIndex);
            else if ((draggedInside &&
                    //this will induce further callbacks which corrects the position
                    equipment.UnequipArms((Armament.Slot)draggedCopy.itemIndex, draggedSiblingIndex)) 
                    == false)
                master.equipmentPanel.ResetPosition((Armament.Slot)draggedCopy.itemIndex);
        }
        else if (endPosition == EndDragPosition.original)
        {
            if (draggedCopy.isInventory)
                draggedCopy.transform.SetSiblingIndex(draggedCopy.itemIndex);
            else
                master.equipmentPanel.ResetPosition((Armament.Slot)draggedCopy.itemIndex);
        }
    }

    public void OnDrag()
    {        
        if (draggedMouse)
        {
            FocusScroll(master.GetDragged);

            Vector2 mousePosition = Mouse.current.position.ReadValue();
            master.GetDragged.transform.position = mousePosition;

            //update draggedSiblingIndex according to the mouse's current position            
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                displayersGridRect,
                mousePosition, null, out Vector2 localPoint);

            //anchorPosition y is inverted
            localPoint.y = -localPoint.y;

            int x =
                GetIndexPosition(
                    displayersGrid.padding.left,
                    displayersGrid.cellSize.x,
                    displayersGrid.spacing.x,
                    displayersGridRect.rect.width,
                    localPoint.x,
                    out int xMax),
            y =
                GetIndexPosition(
                    displayersGrid.padding.top,
                    displayersGrid.cellSize.y,
                    displayersGrid.spacing.y,
                    displayersGridRect.rect.height,
                    localPoint.y,
                    out _);

            draggedSiblingIndex = Mathf.Min(y * (xMax + 1) + x, displayers.Count - 1);
            emptyPlaceholder.SetSiblingIndex(draggedSiblingIndex);
        }
    }

    public void OnSelect(ItemDisplayer displayer)
    {   
        if(displayer.isInventory && EventSystemModifier.Current.IsUsingMouse == false)
            FocusScroll(displayers[displayer.itemIndex]);
    }

    public Selectable SelectOnFocus() =>
        0 < displayers.Count ? displayers[0].selectable : null;

    public void OnPointerEnter()
    {
        draggedInside = true;
    }

    public void OnPointerExit()
    {
        draggedInside = false;
    }

    private void Awake()
    {
        displayers = new List<ItemDisplayer>();
        displayersGridRect = (RectTransform)displayersGrid.transform;

        //allows the grid to stay constantly updated, even when disabled
        master.playerEquipment.OnInventoryInsert += InsertEntry;
        master.playerEquipment.OnInventoryUpdate += UpdateEntry;
        master.playerEquipment.OnInventoryDelete += DeleteEntry;
        master.playerEquipment.OnInventoryReorder += ReorderEntry;

        //populate grid
        var inventory = master.playerEquipment.inventory;
        for (int i = 0; i < inventory.Count; i++)
            InsertEntry(i, inventory[i]);
    }

    private void OnDestroy()
    {
        master.playerEquipment.OnInventoryInsert -= InsertEntry;
        master.playerEquipment.OnInventoryUpdate -= UpdateEntry;
        master.playerEquipment.OnInventoryDelete -= DeleteEntry;
        master.playerEquipment.OnInventoryReorder -= ReorderEntry;
    }

#if UNITY_EDITOR
    private void Update()
    {
        ValidEntries();
    }
#endif

    private void InsertEntry(int index, InventoryEntry entry)
    {
        ItemDisplayer displayer = master.displayersPool.GenerateObject(displayersGrid.transform, index)
            .GetComponent<ItemDisplayer>();
        displayers.Insert(index, displayer);
        displayer.master = master;
        displayer.isInventory = true;

        UpdateEntry(index, entry);
        UpdateInventoryIndexes(index + 1);
    }

    private void UpdateEntry(int index, InventoryEntry entry)
    {
        ItemDisplayer displayer = displayers[index];
                
        displayer.itemIndex = index;
        displayer.SetCount(entry.count);
        displayer.SetIcon(entry.item.icon);
    }

    private void DeleteEntry(int index)
    {
        var displayer = displayers[index];

        if (displayer == master.GetDragged)
            master.EndDrag(EndDragPosition.none);

        displayers.RemoveAt(index);
        master.displayersPool.RemoveObject(displayer.gameObject);

        //update item indexes in active displayers
        UpdateInventoryIndexes(index);
    }

    private void ReorderEntry(int oldIndex, int newIndex)
    {
        var displayer = displayers[oldIndex];

        if (displayer == master.GetDragged)
            master.EndDrag(EndDragPosition.none);

        displayers.RemoveAt(oldIndex);
        displayers.Insert(newIndex, displayer);

        displayer.transform.SetSiblingIndex(newIndex);

        UpdateInventoryIndexes(Mathf.Min(oldIndex, newIndex));
    }

    private void UpdateInventoryIndexes(int startIndex)
    {
        for (int i = startIndex; i < displayers.Count; i++)
            displayers[i].itemIndex = i;
    }

#if UNITY_EDITOR
    private void ValidEntries()
    {
        var inventory = master.playerEquipment.inventory;
        for (int i = 0; i < inventory.Count; i++)
        {
            var entry = inventory[i];
            if(displayers.Count <= i)
            {
                Debug.LogWarning("insufficient displayers", this);
                continue;
            }
            var myEntry = displayers[i];
            if (entry.item.icon == myEntry.iconDisplay.sprite &&
                myEntry.itemIndex == i)
            {
                if (entry.count == 1)
                {
                    if (myEntry.countDisplay.gameObject.activeSelf)
                        Debug.LogError("when count is 1, object should be disabled", myEntry);
                }
                else if (myEntry.countDisplay.gameObject.activeSelf == false ||
                    entry.count != int.Parse(myEntry.countDisplay.text))
                    Debug.LogError("count display is incorrect", myEntry);

                if (master.GetDragged == null)
                {
                    if (myEntry.transform.GetSiblingIndex() == i)
                        continue;
                    else
                        Debug.LogError(
                            "sibling index incorrect : " +
                            myEntry.transform.GetSiblingIndex() +
                            ", expected : " + i, myEntry);
                }
            }
            else
            {
                Debug.LogError(
                    "information incorrect : " +
                    myEntry.transform.GetSiblingIndex(),
                    myEntry);
            }
        }
    }
#endif

    private static int GetIndexPosition(float paddingStart, float cellSize, float spacing, float totalLength, float point, out int max)
    {
        float firstEnd = paddingStart + cellSize + spacing / 2.0f;
        float widthPerCell = cellSize + spacing;
        max = Mathf.FloorToInt((totalLength - firstEnd) / widthPerCell);

        if (point < firstEnd)
            return 0;

        float lastStart = firstEnd + widthPerCell * max;

        if (lastStart < point)
            return max;

        else
            return Mathf.CeilToInt((point - firstEnd) / widthPerCell);
    }
}
