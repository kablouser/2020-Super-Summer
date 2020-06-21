using UnityEngine;
using static Armament;
using static Fighter;

public abstract class Ability : ScriptableObject
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
                    return leftHand;
            }
        }
    }

    [Header("AI")]
    public EnemyControl.AbilityManual abilityManual;

    public abstract void Use(ArmamentPrefab arms, InputPhase phase, out bool isProblem);
    public abstract void TryEndUse(ArmamentPrefab arms, out bool isProblem);
    public abstract void ForceEndUse(ArmamentPrefab arms);
    public abstract bool HasEnded(ArmamentPrefab arms);
}