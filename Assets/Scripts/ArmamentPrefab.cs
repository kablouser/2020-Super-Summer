using UnityEngine;

using static Armament;

public class ArmamentPrefab : MonoBehaviour
{
    private static readonly Quaternion copyRotationOffset = Quaternion.Euler(-90, 0, 180);

    // these are all assigned during SpawnPrefab in Armament
    public CharacterComponents characterComponents;
    // only used for inventory management
    public Armament armsScriptable;
    public Slot[] usedSlots;
    public HoldMethod holdMethod;

    [Header("Fill in these parameters before spawning")]
    // this is should be assigned in the prefab already
    public bool flipOnLeftHand;
    public Transform flipTransform;
    public IdleAnimation idleAnimation;
    [Tooltip("Should be size 4 - unless you have a custom controls")]
    public Ability[] abilitySet = new Ability[4];

    private Transform copyTransform;

    public void Setup(CharacterComponents characterComponents, Armament armsScriptable, Slot[] usedSlots, HoldMethod holdMethod)
    {
        this.characterComponents = characterComponents;
        this.armsScriptable = armsScriptable;
        this.usedSlots = usedSlots;
        this.holdMethod = holdMethod;

        foreach (Ability ability in abilitySet)
            ability.Setup(characterComponents, holdMethod == HoldMethod.rightHand);
    }

    public virtual void MapAbilitySet()
    {
        var fighter = characterComponents.fighter;

        if (holdMethod == HoldMethod.leftHand)
        {
            fighter.AddAbility(0, abilitySet[0]);
            fighter.AddAbility(1, abilitySet[1]);
        }
        else if (holdMethod == HoldMethod.rightHand)
        {
            fighter.AddAbility(2, abilitySet[2]);
            fighter.AddAbility(3, abilitySet[3]);
        }
        else if (holdMethod == HoldMethod.bothHands)
        {
            fighter.AddAbility(0, abilitySet[0]);
            fighter.AddAbility(1, abilitySet[1]);
            fighter.AddAbility(2, abilitySet[2]);
            fighter.AddAbility(3, abilitySet[3]);
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

    public virtual void CorrectPosition(Transform setParent, Transform[] equipmentTransform)
    {
        int index = holdMethod == HoldMethod.bothHands ? (int)Slot.rightHand : (int)usedSlots[0];
        transform.SetParent(setParent, false);
        copyTransform = equipmentTransform[index];
        Update();

        if (flipOnLeftHand && holdMethod == HoldMethod.rightHand)
            flipTransform.localScale = new Vector3(-1, 1, 1);
        else
            flipTransform.localScale = Vector3.one;
    }

    private void Update()
    {
        transform.position = copyTransform.position;
        transform.rotation = copyTransform.rotation * copyRotationOffset;
    }
}
