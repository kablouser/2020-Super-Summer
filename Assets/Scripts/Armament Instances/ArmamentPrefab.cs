using UnityEngine;

using static Armament;
using static AbilityCreator;
using static Fighter;

public class ArmamentPrefab : MonoBehaviour, IAbilityMirror
{
    // these are all assigned during SpawnPrefab in Armament
    public CharacterComponents characterComponents;
    // only used for inventory management
    public Armament armsScriptable;
    public Slot[] usedSlots;
    public HoldMethod holdMethod;

    [Space]
    // this is should be assigned in the prefab already
    public bool flipOnLeftHand;
    public IdleAnimation idleAnimation;
    [Tooltip("Should be size 4 - unless you have a custom controls")]
    public AbilityCreator[] abilitySet = new AbilityCreator[4];
    private AbilityInstance[] abilityInstances;

    public bool DoMirror => holdMethod == HoldMethod.rightHand;

    public virtual void AfterSpawn()
    {
        abilityInstances = new AbilityInstance[4];

        if (holdMethod == HoldMethod.leftHand)
        {
            abilityInstances[0] = abilitySet[0].CreateAbility(this, characterComponents);
            abilityInstances[1] = abilitySet[1].CreateAbility(this, characterComponents);
        }
        else if (holdMethod == HoldMethod.rightHand)
        {
            abilityInstances[2] = abilitySet[2].CreateAbility(this, characterComponents);
            abilityInstances[3] = abilitySet[3].CreateAbility(this, characterComponents);
        }
        else if (holdMethod == HoldMethod.bothHands)
        {
            abilityInstances[0] = abilitySet[0].CreateAbility(this, characterComponents);
            abilityInstances[1] = abilitySet[1].CreateAbility(this, characterComponents);
            abilityInstances[2] = abilitySet[2].CreateAbility(this, characterComponents);
            abilityInstances[3] = abilitySet[3].CreateAbility(this, characterComponents);
        }
    }

    public virtual void MapAbilitySet()
    {
        var fighter = characterComponents.fighter;

        if (holdMethod == HoldMethod.leftHand)
        {
            fighter.AddAbility(0, abilityInstances[0], this);
            fighter.AddAbility(1, abilityInstances[1], this);
        }
        else if (holdMethod == HoldMethod.rightHand)
        {
            fighter.AddAbility(2, abilityInstances[2], this);
            fighter.AddAbility(3, abilityInstances[3], this);
        }
        else if (holdMethod == HoldMethod.bothHands)
        {
            fighter.AddAbility(0, abilityInstances[0], this);
            fighter.AddAbility(1, abilityInstances[1], this);
            fighter.AddAbility(2, abilityInstances[2], this);
            fighter.AddAbility(3, abilityInstances[3], this);
        }
    }

    public virtual void UnmapAbilitySet()
    {
        var fighter = characterComponents.fighter;

        if (holdMethod == HoldMethod.leftHand)
        {
            fighter.RemoveAbility(0);
            fighter.RemoveAbility(1);
        }
        else if (holdMethod == HoldMethod.rightHand)
        {
			fighter.RemoveAbility(2);
            fighter.RemoveAbility(3);
        }
        else if (holdMethod == HoldMethod.bothHands)
        {
            fighter.RemoveAbility(0);
            fighter.RemoveAbility(1);
            fighter.RemoveAbility(2);
            fighter.RemoveAbility(3);
        }
    }

    public virtual void CorrectPosition(Transform[] equipmentTransform)
    {
        int index = holdMethod == HoldMethod.bothHands ? (int)Slot.rightHand : (int)usedSlots[0];
        transform.parent = equipmentTransform[index];
        transform.localPosition = Vector3.zero;

        if (flipOnLeftHand && holdMethod == HoldMethod.leftHand)
            transform.localRotation = Quaternion.Euler(-90, 180, 180);
        else
            transform.localRotation = Quaternion.Euler(-90, 0, 180);
    }
}
