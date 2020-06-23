using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Arms", menuName = "Item/Armament", order = 2)]
public class Armament : Item
{
    public enum Slot { head, body, arms, legs, feets, leftHand, rightHand,
        ///<summary>the number of slots</summary>
        MAX,
        ///<summary>represents any of the slots</summary>
        ANY}
    public enum HoldMethod { none, leftHand, rightHand, bothHands};
    public enum IdleAnimation { defaultIdle }

    [System.Serializable]
    public struct SlotArray
    {
        public Slot[] array;
    }

    //This wrapper allows the inspector to see the rectangular array
    [System.Serializable]    
    public struct SlotConfigurations
    {
        [HideInInspector]
        public SlotArray[] configs;
        [SerializeField]
        public SimpleArray test;
    }

    [Space]
    [Header("Armament")]
    public ArmamentPrefab armsPrefab;
    [Tooltip("Cannot be empty")]    
    public SlotConfigurations usableConfigs;

    /// <summary>
    /// 1. Searches for a config with empty slots containing the attempted slot.
    /// 2. If none are available it will find a config containing the attempted slot.
    /// 3. Otherwise another config is used.
    /// </summary>
    public virtual void EquipRequirements(Equipment equipment, Slot attemptSlot, out Slot[] usedSlots)
    {
        usedSlots = null;

        foreach (var config in usableConfigs.configs)
        {
            bool allFree = true;
            bool containsAttemptSlot = false;            
            foreach (Slot slot in config.array)
            {
                if (equipment.IsSlotFree(slot) == false)
                    allFree = false;
                if (slot == attemptSlot)
                    containsAttemptSlot = true;                
            }

            if (allFree && containsAttemptSlot)
            {
                usedSlots = config.array;
                return;
            }
            if(usedSlots == null && allFree)
            {
                usedSlots = config.array;
            }
        }

        if (usedSlots == null)
            usedSlots = usableConfigs.configs[0].array;
    }

    public virtual ArmamentPrefab SpawnPrefab(Equipment equipment, Slot[] usedSlots, Transform[] equipmentTransforms)
    {        
        ArmamentPrefab prefab = Instantiate(armsPrefab);
        prefab.equipment = equipment;
        prefab.characterSheet = equipment.characterSheet;
        prefab.fighter = equipment.fighter;
        prefab.armsScriptable = this;
        prefab.usedSlots = usedSlots;
        if(usedSlots.Length == 1)
        {
            if (usedSlots[0] == Slot.leftHand)
                prefab.holdMethod = HoldMethod.leftHand;
            else if (usedSlots[0] == Slot.rightHand)
                prefab.holdMethod = HoldMethod.rightHand;
        }
        else if(usedSlots.Length == 2)
        {
            if ((usedSlots[0] == Slot.leftHand && usedSlots[1] == Slot.rightHand) ||
                (usedSlots[1] == Slot.leftHand && usedSlots[0] == Slot.rightHand))
                prefab.holdMethod = HoldMethod.bothHands;
        }

        prefab.AfterSpawned();
        prefab.CorrectPosition(equipmentTransforms);

        return prefab;
    }

    public virtual void DespawnPrefab(ArmamentPrefab armInstance)
    {
        Destroy(armInstance);
    }
}
