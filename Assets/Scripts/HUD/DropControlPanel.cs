using UnityEngine;
using UnityEngine.UI;

using TMPro;

public class DropControlPanel : Panel
{
    public override bool HideOnClickOutside => true;
    public override Selectable SelectOnFocus => slider;
    public override Selectable SelectOnDefocus => controlling.selectable;    

    public InventoryPanel master;
    public TMP_InputField inputField;
    public Slider slider;
    public DroppedItem droppedItemPrefab;
    public Vector3 droppedOffset;

    private ItemDisplayer controlling;

    public void Show(ItemDisplayer target)
    {
        controlling = target;

        int count = master.playerEquipment.inventory[target.itemIndex].count;
        inputField.SetTextWithoutNotify(count.ToString());
        slider.value = slider.maxValue = count;

        Show(target.RectTransform);
    }

    public override void Hide()
    {        
        base.Hide();
        controlling = null;
    }

    public void OnInputChanged()
    {
        if (int.TryParse(inputField.text, out int setInteger) == false)
            return;

        int max = master.playerEquipment.inventory[controlling.itemIndex].count;

        if (setInteger < 0)
        {
            setInteger = 1;
            inputField.SetTextWithoutNotify(setInteger.ToString());
        }
        else if(max < setInteger)
        {
            setInteger = max;
            inputField.SetTextWithoutNotify(setInteger.ToString());
        }

        slider.value = setInteger;
    }

    public void OnSliderChanged()
    {
        int setInteger = (int)slider.value;
        inputField.SetTextWithoutNotify(setInteger.ToString());
    }

    public void DropAndHide(bool nextFrame = false)
    {
        DropItem((int)slider.value, controlling.itemIndex);

        if (nextFrame)
            HideNextFrame();
        else
            Hide();
    }

    public override void OnSubmit()
    {
        if (IsShown)
            DropAndHide(true);
    }

    public void DropItem(int count, int inventoryIndex)
    {
        var item = master.playerEquipment.inventory[inventoryIndex].item;
        if (master.playerEquipment.RemoveItem(inventoryIndex, count, out _))
        {
            var droppedItem = Instantiate(droppedItemPrefab,
                master.playerEquipment.transform.position + droppedOffset,
                Quaternion.identity);

            droppedItem.SetItem(new Inventory.InventoryEntry(count, item));
        }
    }
}
