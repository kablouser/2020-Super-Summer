using UnityEngine;

[CreateAssetMenu(fileName = "Arms", menuName = "Items/Armament", order = 2)]
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
    public struct Config
    {
        public Slot[] config;
    }

    public ArmamentPrefab armsPrefab;
    [Tooltip("Cannot be empty")]
    public Config[] usableConfigs;

    /// <summary>
    /// 1. Searches for a config with empty slots containing the attempted slot.
    /// 2. If none are available it will find a config containing the attempted slot.
    /// 3. Otherwise another config is used.
    /// </summary>
    public virtual void EquipRequirements(Equipment equipment, Slot attemptSlot, out Slot[] usedSlots, out bool onlyEmpty)
    {
        usedSlots = null;

        foreach (var config in usableConfigs)
        {
            onlyEmpty = true;
            bool containsAttemptSlot = false;            
            foreach (Slot slot in config.config)
            {
                if (equipment.IsSlotFree(slot) == false)
                    onlyEmpty = false;
                if (slot == attemptSlot)
                    containsAttemptSlot = true;                
            }

            if (containsAttemptSlot)
            {
                usedSlots = config.config;
                return;
            }
            else if(attemptSlot == Slot.ANY && onlyEmpty)
            {
                usedSlots = config.config;
                return;
            }
        }

        onlyEmpty = false;
        if (usedSlots == null)
            usedSlots = usableConfigs[0].config;
    }

    public virtual ArmamentPrefab SpawnPrefab(Equipment equipment, Slot[] usedSlots, Transform[] equipmentTransforms)
    {        
        ArmamentPrefab prefab = Instantiate(armsPrefab);
        prefab.characterComponents = equipment.characterComponents;
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
        
        prefab.CorrectPosition(equipmentTransforms);
        prefab.AfterSpawn();
        prefab.MapAbilitySet();

        return prefab;
    }

    public virtual void DespawnPrefab(ArmamentPrefab prefab)
    {
        prefab.UnmapAbilitySet();
        Destroy(prefab.gameObject);
    }
}
