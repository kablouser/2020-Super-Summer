using UnityEngine;
using System.Collections.Generic;
using static Armament;
using static Fighter;

public abstract class AbilityCreator : ScriptableObject
{
    [System.Serializable]
    public class HoldMethodGroup<T>
    {
        public T leftHand;
        public T rightHand;
        public T bothHands;
        public T GetHand(HoldMethod holdMethod)
        {
            switch(holdMethod)
            {
                case HoldMethod.leftHand:
                    return leftHand;
                case HoldMethod.rightHand:
                    return rightHand;
                case HoldMethod.bothHands:
                    return bothHands;
                default:
                    return rightHand;
            }
        }
    }

    public abstract class AbilityInstance
    {
        public CharacterComponents characterComponents;
        public AbilityCreator creator;

        protected AbilityInstance(CharacterComponents characterComponents, AbilityCreator creator)
        {
            this.characterComponents = characterComponents;
            this.creator = creator;
        }

        public abstract bool CanUse(InputPhase phase);
        public abstract void Use(InputPhase phase);
        public abstract void TryEndUse(out bool isProblem);
        public abstract void ForceEndUse();
        public abstract bool HasEnded();
    }

    public EnemyControl.AbilityManual abilityManual;

    /**
     * <summary>Creates a instance of ability to be used.</summary>
     * <param name="interfaceObject">An interface used to initialise the instance.</param>
     * <returns>An object containing temporary variables to allow the ability to work.
     * Returns null if interfaceObject isn't satisfied.</returns>
     */
    public abstract AbilityInstance CreateAbility(
        object interfaceObject,
        CharacterComponents characterComponents);
    
    public static void ChooseArmament<T>(Equipment equipment, List<T> chosen)
    {
        for (int i = (int)Slot.MAX - 1; 0 <= i; i--)
        {
            ArmamentPrefab findArmament = equipment.GetArms((Slot)i);            
            if(findArmament is T t && chosen.Contains(t) == false)
                chosen.Add(t);
        }
    }
}