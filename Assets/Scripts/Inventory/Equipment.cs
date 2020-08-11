using UnityEngine;
using static Armament;

public class Equipment : Inventory
{
    public readonly Vector3 droppedOffset = new Vector3(0, 1, 1);

    public delegate void EquipmentUpdate(Slot slot, ArmamentPrefab prefab);
    public event EquipmentUpdate OnEquipmentUpdate;

    [Space]
    public CharacterComponents characterComponents;
    /// <summary>
    /// equipped items will be parented to this, for organisation
    /// </summary>
    public Transform equipmentParent;
    /// <summary>
    /// equipped items will copy positions from these transforms
    /// </summary>
    public SlotArray<Transform> equipmentTransforms;
    public SlotArray<ArmamentPrefab> equippedArms;    
    
    private Fighter fighter;

    public bool IsSlotFree(Slot slot)
    {
        return equippedArms.array[(int)slot] == null;
    }

    public bool EquipArms(int inventoryIndex, Slot intoSlot)
    {
        if (inventory.Count <= inventoryIndex) return false;

        if (inventory[inventoryIndex].item is Armament arms &&
            EquipArms(arms, intoSlot))
        {
            RemoveItem(inventoryIndex, 1, out _, false);
            return true;
        }
        return false;
    }

    public bool EquipArms(Armament arms, Slot intoSlot)
    {
        //get the equipped into slots
        arms.EquipRequirements(this, intoSlot, out Slot[] usedSlots, out _);
        //try to stop all arms in those slots
        foreach (Slot usedSlot in usedSlots)
        {
            ArmamentPrefab current = GetArms(usedSlot);
            if (current != null && CanUnequip() == false)
                return false;
        }

        //finally, spawn/despawn prefab and update references
        ArmamentPrefab prefab =
            arms.SpawnPrefab(this, equipmentParent, usedSlots, equipmentTransforms);
        foreach (Slot usedSlot in usedSlots)
        {
            UnequipArms(usedSlot);
            equippedArms[(int)usedSlot] = prefab;

            OnEquipmentUpdate?.Invoke(usedSlot, prefab);
        }
        
        return true;
    }

    public bool UnequipArms(Slot fromSlot, int targetIndex = -1, bool discard = false, bool drop = false)
    {
        ArmamentPrefab prefab = equippedArms[(int)fromSlot];
        if (prefab == null) return true;

        if (CanUnequip() == false) return false;

        Armament arms = prefab.armsScriptable;
        arms.DespawnPrefab(prefab);

        foreach (Slot usedSlot in prefab.usedSlots)
        {
            equippedArms[(int)usedSlot] = null;

            OnEquipmentUpdate?.Invoke(usedSlot, null);
        }

        if (discard) return true;

        //add into inventory
        if (drop == false && AddItem(arms, 1, out int addedIndex))
        {
            if (targetIndex != -1 && 1 == inventory[addedIndex].count)
                Reorder(addedIndex, targetIndex);
        }
        else
        {
            //drop onto the floor
            SpawnDroppedItem(new InventoryEntry(1, arms));
        }
        return true;
    }

    /// <summary>
    /// Equips everything in the inventory
    /// </summary>
    public void AutoEquip()
    {
        for (int i = 0; i < inventory.Count; i++)
        {
            bool onlyEmpty;

            do
            {
                if (inventory[i].item is Armament arms)
                {
                    arms.EquipRequirements(this, Slot.ANY, out _, out onlyEmpty);

                    if (onlyEmpty && EquipArms(i, Slot.ANY) && inventory.Count <= i)
                        return;
                }
                else break;
            }
            while (onlyEmpty);
        }
    }

    public ArmamentPrefab GetArms(Slot slot) => GetArms((int)slot);

    public ArmamentPrefab GetArms(int equipmentIndex)
    {
        if (equippedArms.array.Length <= equipmentIndex)
            return null;
        else
            return equippedArms[equipmentIndex];
    }

    public bool CanUnequip()
    {
        fighter.TryStopLastAbility(out bool isProblem);
        return isProblem == false;
    }

    public void UnpackEquippedIntoWorld()
    {
        foreach (var equipped in equippedArms.array)
            if(equipped != null)
                equipped.UnpackIntoWorld();
    }

    protected virtual void Awake()
    {
        fighter = characterComponents.fighter;
    }

    protected override void SpawnDroppedItem(InventoryEntry entry)
    {
        Instantiate(droppedItemPrefab,
            transform.position + characterComponents.movement.bodyRotator.rotation * droppedOffset,
            Quaternion.identity)
            .SetItem(entry);
    }
}
