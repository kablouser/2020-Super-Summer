using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

using static Inventory;

public class EquipmentDisplayer : Panel, IUpdateInventory
{
    public override bool HideOnClickOutside => false;
    public override Selectable SelectOnFocus =>
        0 < displayers.Count ? displayers[0].selectable : null;
    public override Selectable SelectOnDefocus => null;

    public Equipment playerEquipment;

    public RectTransform gridViewport;
    public GridLayoutGroup displayersGrid;
    public ObjectPool displayersPool;
    public InformationPanel informationPanel;
    public Transform emptyPlaceholder;
    public Transform draggedTempParent;
    public ScrollRect gridScrollRect;

    public ItemControlPanel itemControlPanel;
    public DropControlPanel dropControlPanel;

    private List<ItemDisplayer> displayers;
    private RectTransform displayersGridRect;

    private ItemDisplayer dragged;
    private int draggedSiblingIndex;
    private bool draggedMouse;

    private void Awake()
    {
        displayers = new List<ItemDisplayer>();
        displayersGridRect = (RectTransform)displayersGrid.transform;
    }

    private void Start()
    {
        PopulateDisplayGrid();
        playerEquipment.updateInterfaces.Add(this);
    }

    public void OnDisplayerMove(Vector2 navigation)
    {
        if (dragged != null && draggedMouse == false)
        {
            Vector2 localPoint = dragged.rectTransform.anchoredPosition;

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
            dragged.transform.SetSiblingIndex(draggedSiblingIndex);

            if (yBefore != y)
                FocusScroll(dragged, y - yBefore);
        }
    }

    public void PopulateDisplayGrid()
    {
        for(int i = 0; i < playerEquipment.inventory.Count; i++)
            InsertEntry(i, playerEquipment.inventory[i]);
    }

    public void SelectEntry(int index)
    {
        var item = playerEquipment.inventory[index].item;
        informationPanel.SetItem(item);

        if (item == dragged && draggedMouse == false)
            EndDrag();
    }

    /// <summary>
    /// Make sure the selected item displayers is within the scroll view. Adjust the scroll view as necessary.
    /// </summary>
    public void FocusScroll(ItemDisplayer selected, int levelShift = 0)
    {   
        RectTransform selectedRect = selected.rectTransform;
        bool instantSnap;
        float displayerTop;

        //anchor y positive goes up, negative down
        if (selected == dragged && draggedMouse)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                displayersGridRect, selectedRect.position, null, out Vector2 localPoint);

            localPoint.y += selectedRect.rect.height / 2.0f;

            float anchorY = displayersGridRect.anchoredPosition.y;

            displayerTop = Mathf.Clamp(
                anchorY + localPoint.y,
                anchorY - displayersGridRect.rect.height,
                anchorY);

            instantSnap = false;
        }
        else
        {
            displayerTop = 
                displayersGridRect.anchoredPosition.y + 
                selectedRect.anchoredPosition.y + 
                selectedRect.rect.height / 2.0f -
                levelShift * (displayersGrid.cellSize.y + displayersGrid.spacing.y);
            instantSnap = true;
        }
        
        if (0 < displayerTop)
        {
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
            Vector2 gridPosition = displayersGridRect.anchoredPosition;

            if(instantSnap)
                gridPosition.y += displayerBottom - viewPortHeight + displayersGrid.padding.bottom;
            else
                gridPosition.y += gridScrollRect.scrollSensitivity * Time.deltaTime * displayersGrid.cellSize.y;

            displayersGridRect.anchoredPosition = gridPosition;
            return;
        }
    }

    public void InsertEntry(int index, InventoryEntry entry)
    {
        ItemDisplayer displayer = displayersPool.GenerateObject(displayersGrid.transform, index)
            .GetComponent<ItemDisplayer>();
        displayers.Insert(index, displayer);
        displayer.master = this;

        UpdateEntry(index, entry);
        UpdateInventoryIndexes(index + 1);
    }

    public void UpdateEntry(int index, InventoryEntry entry)
    {
        ItemDisplayer displayer = displayers[index];

        displayer.inventoryIndex = index;
        displayer.SetCount(entry.count);
        displayer.SetIcon(entry.item.icon);
    }

    public void DeleteEntry(int index)
    {
        var displayer = displayers[index];

        if (displayer == dragged)
            EndDrag(false, false);

        displayers.RemoveAt(index);
        displayersPool.RemoveObject(displayer.gameObject);

        //update item indexes in active displayers
        UpdateInventoryIndexes(index);
    }

    public void ReorderEntry(int oldIndex, int newIndex)
    {
        var displayer = displayers[oldIndex];

        if (displayer == dragged)
            EndDrag(false, false);

        displayers.RemoveAt(oldIndex);
        displayers.Insert(newIndex, displayer);

        displayer.transform.SetSiblingIndex(newIndex);

        UpdateInventoryIndexes(Mathf.Min(oldIndex, newIndex));
    }

    public void BeginDrag(int index, bool isMouse)
    {
        if (dragged != null)
            //cancel the current dragged            
            EndDrag(false, true);
        
        dragged = displayers[index];
        draggedMouse = isMouse;

        if (draggedMouse)
        {
            dragged.transform.SetParent(draggedTempParent);
            emptyPlaceholder.SetParent(displayersGrid.transform);
            emptyPlaceholder.SetSiblingIndex(dragged.inventoryIndex);
        }
        else
        {
            dragged.SetNavigation(false);
            dragged.selectable.Select();
            dragged.SetBobbing(true);
        }

        draggedSiblingIndex = dragged.inventoryIndex;        
    }

    public void EndDrag(bool changePosition = true, bool resetPosition = true)
    {
        if (dragged == null)
            return;

        var copy = dragged;
        dragged = null;

        if (draggedMouse)
        {
            emptyPlaceholder.SetParent(draggedTempParent);
            copy.transform.SetParent(displayersGrid.transform);
            if (changePosition)
            {
                //this will induce further callbacks which reparents the dragged
                playerEquipment.Reorder(copy.inventoryIndex, draggedSiblingIndex);
            }
            else if (resetPosition)
            {
                copy.transform.SetSiblingIndex(copy.inventoryIndex);
            }
        }
        else
        {
            copy.SetBobbing(false);
            int finalPosition = -1;
            if (changePosition)
            {
                //this will induce further callbacks which reparents the dragged
                playerEquipment.Reorder(copy.inventoryIndex, draggedSiblingIndex);
                finalPosition = draggedSiblingIndex;
            }
            else if (resetPosition)
            {
                copy.transform.SetSiblingIndex(copy.inventoryIndex);
                finalPosition = copy.inventoryIndex;
            }

            //the copy has been recycled
            copy.SetNavigation(true);
            if (finalPosition != -1)
                displayers[finalPosition].selectable.Select();
        }
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

    public void DisplayerOnSubmit(ItemDisplayer displayer, bool isMouse)
    {
        if (dragged == null)
            ShowControls(displayer, isMouse);
        else
            EndDrag();
    }

    private void UpdateInventoryIndexes(int startIndex)
    {
        for (int i = startIndex; i < displayers.Count; i++)
            displayers[i].inventoryIndex = i;
    }

    private void Update()
    {
        ValidEntries();
        if (dragged != null && draggedMouse)
        {
            FocusScroll(dragged);

            Vector2 mousePosition = Mouse.current.position.ReadValue();
            dragged.transform.position = mousePosition;

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

    private void ValidEntries()
    {
        for (int i = 0; i < playerEquipment.inventory.Count; i++)
        {
            var entry = playerEquipment.inventory[i];
            var myEntry = displayers[i];
            if (entry.item.icon == myEntry.iconDisplay.sprite &&
                myEntry.inventoryIndex == i)
            {
                if (entry.count == 1)
                {
                    if (myEntry.countDisplay.gameObject.activeSelf)
                        Debug.LogError("when count is 1, object should be disabled", myEntry);
                }
                else if (myEntry.countDisplay.gameObject.activeSelf == false ||
                    entry.count != int.Parse(myEntry.countDisplay.text))
                    Debug.LogError("count display is incorrect", myEntry);

                if (dragged == null)
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
