using UnityEngine;
using static Armament;
using static Fighter;
public abstract class ArmamentPrefab : MonoBehaviour
{
    [Header("(Assigned before spawn)")]
    public bool flipOnLeftHand;
    // this is should be assigned in the prefab already
    public IdleAnimation idleAnimation;
    [Tooltip("Should be size 4 - unless you have a custom controls")]
    public Ability[] abilitySet = new Ability[4];

    [Header("(Assigned after Armament.SpawnPrefab)")]
    // these are all assigned during SpawnPrefab in Armament
    public Equipment equipment;
    public CharacterSheet characterSheet;
    public Fighter fighter;
    // only used for inventory management
    public Armament armsScriptable;
    public Slot[] usedSlots;
    public HoldMethod holdMethod;

    public virtual void MapAbilitySet(AbilityContainer[] fighterAbilitySet)
    {
        if (holdMethod == HoldMethod.leftHand)
        {
            fighterAbilitySet[0] = new AbilityContainer() { arms = this, ability = abilitySet[0] };
            fighterAbilitySet[1] = new AbilityContainer() { arms = this, ability = abilitySet[1] };
        }
        else if (holdMethod == HoldMethod.rightHand)
        {
            fighterAbilitySet[2] = new AbilityContainer() { arms = this, ability = abilitySet[2] };
            fighterAbilitySet[3] = new AbilityContainer() { arms = this, ability = abilitySet[3] };
        }
        else if (holdMethod == HoldMethod.bothHands)
        {
            fighterAbilitySet[0] = new AbilityContainer() { arms = this, ability = abilitySet[0] };
            fighterAbilitySet[1] = new AbilityContainer() { arms = this, ability = abilitySet[1] };
            fighterAbilitySet[2] = new AbilityContainer() { arms = this, ability = abilitySet[2] };
            fighterAbilitySet[3] = new AbilityContainer() { arms = this, ability = abilitySet[3] };
        }
    }

    public virtual void UnmapAbilitySet(AbilityContainer[] fighterAbilitySet)
    {
        AbilityContainer empty = new AbilityContainer();
        if (holdMethod == HoldMethod.leftHand)
        {
            fighterAbilitySet[0] = empty;
            fighterAbilitySet[1] = empty;
        }
        else if (holdMethod == HoldMethod.rightHand)
        {
            fighterAbilitySet[2] = empty;
            fighterAbilitySet[3] = empty;
        }
        else if (holdMethod == HoldMethod.rightHand)
        {
            fighterAbilitySet[0] = empty;
            fighterAbilitySet[1] = empty;
            fighterAbilitySet[2] = empty;
            fighterAbilitySet[3] = empty;
        }
    }

    public virtual void CorrectPosition(Transform[] equipmentTransform)
    {
        int index = holdMethod == HoldMethod.bothHands ? (int)Slot.rightHand : (int)usedSlots[0];
        transform.parent = equipmentTransform[index];
        transform.localPosition = Vector3.zero;

        if (flipOnLeftHand && holdMethod == HoldMethod.leftHand)
            transform.localRotation = Quaternion.Euler(0, 180, 0);
        else
            transform.localRotation = Quaternion.identity;
    }

    public delegate void OnAttackedHandler(ArmamentPrefab arms, int damage, Vector3 contactPoint, out int reduction, out int ricochet, out bool poise);
    public virtual void OnAttacked(int damage, Vector3 contactPoint, out int reduction, out int ricochet, out bool poise)
    {
        reduction = ricochet = 0;
        poise = false;
    }

    //required to start some scripts
    public virtual void AfterSpawned() { }
}
