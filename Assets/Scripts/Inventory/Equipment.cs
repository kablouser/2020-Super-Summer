using UnityEngine;
using static Armament;

public class Equipment : Inventory
{
    /// <summary>
    /// inventoryIndex is only given on EquipArms, otherwise its -1
    /// </summary>
    public delegate void EquipmentUpdate(Slot slot, ArmamentPrefab prefab);

    [Space]
    public CharacterComponents characterComponents;

    public SlotArray<Transform> equipmentTransforms;
    public SlotArray<ArmamentPrefab> equippedArms;

    public event EquipmentUpdate OnEquipmentUpdate;

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
            RemoveItem(inventoryIndex, 1, out _);
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
            if (current != null && IsUnequippable(current) == false)
                return false;
        }

        //finally, spawn/despawn prefab and update references
        ArmamentPrefab prefab = arms.SpawnPrefab(this, usedSlots, equipmentTransforms);
        foreach (Slot usedSlot in usedSlots)
        {
            UnequipArms(usedSlot);
            equippedArms[(int)usedSlot] = prefab;

            OnEquipmentUpdate?.Invoke(usedSlot, prefab);
        }
        
        return true;
    }

    public bool UnequipArms(Slot fromSlot, int targetIndex = -1, bool discard = false)
    {
        ArmamentPrefab prefab = equippedArms[(int)fromSlot];
        if (prefab == null) return true;

        if (IsUnequippable(prefab) == false) return false;

        Armament arms = prefab.armsScriptable;
        arms.DespawnPrefab(prefab);

        foreach (Slot usedSlot in prefab.usedSlots)
        {
            equippedArms[(int)usedSlot] = null;

            OnEquipmentUpdate?.Invoke(usedSlot, null);
        }

        if (discard) return true;

        //add into inventory
        if (AddItem(arms, 1, out int addedIndex))
        {
            if (targetIndex != -1 && 1 == inventory[addedIndex].count)
                Reorder(addedIndex, targetIndex);
        }
        else
        {
            //drop onto the floor
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

                    if (onlyEmpty)
                        EquipArms(i, Slot.ANY);
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

    public bool IsUnequippable(ArmamentPrefab prefab)
    {
        fighter.TryStopArms(prefab, out bool isProblem);
        return isProblem == false;
    }

    protected virtual void Awake()
    {
        fighter = characterComponents.fighter;
    }
}
