using UnityEngine;
using System.Collections.Generic;
using static Armament;

[RequireComponent(typeof(CharacterSheet))]
[RequireComponent(typeof(Fighter))]
public class Equipment : Inventory
{
    [Space]
    [Header("Equipment")]
    [Header("0:head, 1:body, 2:arms, 3:legs, 4:feets, 5:leftHand, 6:rightHand")]
    public Transform[] equipmentTransforms = new Transform[(int)Slot.MAX];
    public ArmamentPrefab[] equippedArms = new ArmamentPrefab[(int)Slot.MAX];

    [HideInInspector]
    public CharacterSheet characterSheet;
    [HideInInspector]
    public Fighter fighter;
    private int currentEquipSlow;

    //1x 2-handed weapon = 1, 2x 1-handed of the same weapon = 2
    private Dictionary<Armament, int> equippedArmsCount = new Dictionary<Armament, int>((int)Slot.MAX);

    public bool IsSlotFree(Slot slot)
    {
        return equippedArms[(int)slot] == null;
    }

    public bool EquipArms(Armament arms, Slot intoSlot)
    {
        InventoryEntry entry = FindItem(arms);
        if (entry == null) return false;

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
        fighter.EquipArmament(prefab);
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

    public override bool RemoveItem(Item item, int count, out int missingCount)
    {
        Armament arms = item as Armament;
        InventoryEntry findItem;
        List<Slot> toUnequip = null;

        if (arms != null && (findItem = FindItem(arms)) != null)
        {
            int restriction = findItem.count;
            List<ArmamentPrefab> matches = new List<ArmamentPrefab>();
            toUnequip = new List<Slot>();

            foreach (ArmamentPrefab armsPrefab in equippedArms)
                if (armsPrefab != null && armsPrefab.armsScriptable == arms && !matches.Contains(armsPrefab))
                {
                    if (restriction < matches.Count + 1)
                    {
                        fighter.TryStopArms(armsPrefab, out bool isProblem);
                        if (isProblem)
                        {
                            missingCount = 0;
                            return false;
                        }
                        else
                            toUnequip.Add(armsPrefab.usedSlots[0]);
                    }
                    matches.Add(armsPrefab);
                }
        }

        bool baseResult = base.RemoveItem(item, count, out missingCount);

        if (baseResult && toUnequip != null)
            foreach (Slot slot in toUnequip)
                //we just assume all these unequips have problems
                UnequipArms(slot);

        return baseResult;
    }

    /// <summary>
    /// Equips everything in the inventory
    /// </summary>
    public void AutoEquip()
    {
        foreach (InventoryEntry entry in inventory)
        {
            Armament arms = entry.item as Armament;
            if (arms)
            {
                while (EquipArms(arms, Slot.ANY)) ;
            }
        }            
    }

    public ArmamentPrefab GetArms(Slot slot)
    {
        return equippedArms[(int)slot];
    }

    protected virtual void Awake()
    {
        if (equippedArms == null || equippedArms.Length != (int)Slot.MAX)
            equippedArms = new ArmamentPrefab[(int)Slot.MAX];
        if(equipmentTransforms == null || equipmentTransforms.Length != (int)Slot.MAX)
            equipmentTransforms = new Transform[(int)Slot.MAX];
        if (equippedArmsCount == null)
            equippedArmsCount = new Dictionary<Armament, int>((int)Slot.MAX);

        characterSheet = GetComponent<CharacterSheet>();
        fighter = GetComponent<Fighter>();
    }
}
