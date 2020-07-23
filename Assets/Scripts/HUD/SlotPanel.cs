using UnityEngine;
using UnityEngine.EventSystems;

public class SlotPanel : MonoBehaviour, IDropHandler
{
    public InventoryPanel master;
    public Armament.Slot slot;

    public void OnDrop(PointerEventData eventData)
    {
        ItemDisplayer dragged = master.GetDragged;
        if (dragged != null)
        {
            master.EndDrag(InventoryPanel.EndDragPosition.original);

            if (dragged.isInventory)
            {
                master.playerEquipment.EquipArms(dragged.itemIndex, slot);
            }
            else
            {
                var equipment = master.playerEquipment;
                Armament.Slot draggedFromSlot = (Armament.Slot)dragged.itemIndex;
                ArmamentPrefab prefab = equipment.GetArms(draggedFromSlot);

                if (prefab == null)
                    Debug.LogError("dragged equipment slot is invalid", this);

                foreach (var equippedSlot in prefab.usedSlots)
                    if (equippedSlot == slot)
                        return;

                prefab.armsScriptable.EquipRequirements(equipment, slot, out _, out bool onlyEmpty);
                if (onlyEmpty && equipment.IsUnequippable(prefab))
                {
                    ArmamentPrefab previous = equipment.GetArms(slot);
                    bool hasPrevious = previous != null;
                    Armament previousArms = null;
                    if (hasPrevious)
                        previousArms = previous.armsScriptable;

                    if (equipment.EquipArms(prefab.armsScriptable, slot))
                    {
                        equipment.UnequipArms(draggedFromSlot, -1, true);
                        if(hasPrevious)
                        {
                            equipment.FindItem(previousArms, out _, out int previousIndex);
                            equipment.EquipArms(previousIndex, draggedFromSlot);                            
                        }
                    }
                }
            }
        }
    }
}
