using UnityEngine;
using System.Collections.Generic;
using static Armament;

public class Equipment : Inventory
{
    [Space]
    public CharacterComponents characterComponents;

    public Transform[] equipmentTransforms = new Transform[(int)Slot.MAX];
    public ArmamentPrefab[] equippedArms = new ArmamentPrefab[(int)Slot.MAX];

    private Fighter fighter;

    //1x 2-handed weapon = 1, 2x 1-handed of the same weapon = 2
    private Dictionary<Armament, int> equippedArmsCount = new Dictionary<Armament, int>((int)Slot.MAX);

    public bool IsSlotFree(Slot slot)
    {
        return equippedArms[(int)slot] == null;
    }

    public bool EquipArms(int index, Slot intoSlot)
    {
        if (inventory.Count <= index) return false;

        InventoryEntry entry = inventory[index];
        Armament arms = entry.item as Armament;
        if (arms == null) return false;

        if (equippedArmsCount.TryGetValue(arms, out int getCount))
        {
            if (entry.count < getCount + 1)
                return false;
            else
                equippedArmsCount[arms]++;
        }
        else
        {
            if (entry.count <= 0)
                return false;
            else
                equippedArmsCount.Add(arms, 1);
        }

        arms.EquipRequirements(this, intoSlot, out Slot[] usedSlots);
        foreach (Slot usedSlot in usedSlots)
        {
            ArmamentPrefab current = GetArms(usedSlot);
            if (current != null)
            {
                fighter.TryStopArms(current, out bool isProblem);
                if (isProblem) return false;
            }
        }

        ArmamentPrefab prefab = arms.SpawnPrefab(this, usedSlots, equipmentTransforms);
        foreach (Slot usedSlot in usedSlots)
        {
            UnequipArms(usedSlot);
            equippedArms[(int)usedSlot] = prefab;
        }
        return true;
    }

    public bool UnequipArms(Slot fromSlot, bool act = true)
    {
        ArmamentPrefab prefab = equippedArms[(int)fromSlot];
        if (prefab == null) return true;

        fighter.TryStopArms(prefab, out bool isProblem);
        if (isProblem) return false;

        if (act)
        {
            Armament arms = prefab.armsScriptable;
            arms.DespawnPrefab(prefab);

            int setCount = equippedArmsCount[arms] - 1;
            if (setCount <= 0)
                equippedArmsCount.Remove(arms);
            else
                equippedArmsCount[arms]--;

            foreach (Slot usedSlot in prefab.usedSlots)
                equippedArms[(int)usedSlot] = null;
        }
        return true;
    }

    public override bool RemoveItem(int index, int count, out int missingCount)
    {
        if(inventory.Count <= index)
        {
            missingCount = count;
            return false;
        }

        InventoryEntry entry = inventory[index];
        List<Slot> toUnequip = null;
        int numberToUnequip;

        if (entry.item is Armament arms &&
            //get the number of this arms that are currently equipped
            equippedArmsCount.TryGetValue(arms, out int equippedCount) &&
            //how many do we have to unequip?
            0 < (numberToUnequip = equippedCount + count - entry.count))
        {
            toUnequip = new List<Slot>();          

            //check if we need to unequip any arms, do we have enough?
            foreach (ArmamentPrefab armsPrefab in equippedArms)
                if (armsPrefab != null &&
                    armsPrefab.armsScriptable == arms)
                {
                    fighter.TryStopArms(armsPrefab, out bool isProblem);
                    if (isProblem)
                    {
                        missingCount = 0;
                        return false;
                    }
                    else
                    {
                        toUnequip.Add(armsPrefab.usedSlots[0]);
                        numberToUnequip--;
                        if (numberToUnequip == 0)
                            break;
                    }
                }
        }

        bool baseResult = base.RemoveItem(index, count, out missingCount);

        if (baseResult && toUnequip != null)
            foreach (Slot slot in toUnequip)
                //we just assume all these unequips have no problems
                UnequipArms(slot);
        
        return baseResult;
    }

    /// <summary>
    /// Equips everything in the inventory
    /// </summary>
    public void AutoEquip()
    {
        for(int i = 0; i < inventory.Count; i++)
            while (EquipArms(i, Slot.ANY)) ;
    }

    public ArmamentPrefab GetArms(Slot slot) => GetArms((int)slot);

    public ArmamentPrefab GetArms(int index)
    {
        if (equippedArms.Length <= index)
            return null;
        else
            return equippedArms[index];
    }

    protected override void Awake()
    {
        base.Awake();

        if (equippedArms == null || equippedArms.Length != (int)Slot.MAX)
            equippedArms = new ArmamentPrefab[(int)Slot.MAX];
        if(equipmentTransforms == null || equipmentTransforms.Length != (int)Slot.MAX)
            equipmentTransforms = new Transform[(int)Slot.MAX];
        if (equippedArmsCount == null)
            equippedArmsCount = new Dictionary<Armament, int>((int)Slot.MAX);

        fighter = characterComponents.fighter;
    }
}
