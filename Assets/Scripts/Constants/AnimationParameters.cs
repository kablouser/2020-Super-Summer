using UnityEngine;
using static UnityEngine.Animator;

public static class AnimationConstants
{
    /// <summary>
    /// Animation fps.
    /// </summary>
    public const float FramesPerSecond = 60.0f;

    public static readonly int MoveX = StringToHash("MoveX");
    public static readonly int MoveZ = StringToHash("MoveZ");
    public static readonly int MoveMultiplier = StringToHash("MoveMultiplier");
    public static readonly int Mirror = StringToHash("Mirror");
    public static readonly int Hurt = StringToHash("Hurt");
    public static readonly int Ricochet = StringToHash("Ricochet");

    public static readonly int SwordAttack1 = StringToHash("SwordAttack1");
    public static readonly int SwordAttack2 = StringToHash("SwordAttack2");
    public static readonly int SwordParry = StringToHash("SwordParry");

    public static readonly int ShieldAttack1 = StringToHash("ShieldAttack1");
    public static readonly int ShieldAttack2On = StringToHash("ShieldAttack2On");
    public static readonly int ShieldAttack2Off = StringToHash("ShieldAttack2Off");
    public static readonly int ShieldBlockOn = StringToHash("ShieldBlockOn");
    public static readonly int ShieldBlockOff = StringToHash("ShieldBlockOff");

    public enum AbilityTrigger
    {
        SwordAttack1, SwordAttack2, SwordParry,
        ShieldAttack1, ShieldAttack2On, ShieldAttack2Off, ShieldBlockOn, ShieldBlockOff
    };

    public static int EnumToID(AbilityTrigger enumTrigger)
    {
        switch(enumTrigger)
        {
            case AbilityTrigger.SwordAttack1:
                return SwordAttack1;
            case AbilityTrigger.SwordAttack2:
                return SwordAttack2;
            case AbilityTrigger.SwordParry:
                return SwordParry;
            case AbilityTrigger.ShieldAttack1:
                return ShieldAttack1;
            case AbilityTrigger.ShieldAttack2On:
                return ShieldAttack2On;
            case AbilityTrigger.ShieldAttack2Off:
                return ShieldAttack2Off;
            case AbilityTrigger.ShieldBlockOn:
                return ShieldBlockOn;
            case AbilityTrigger.ShieldBlockOff:
                return ShieldBlockOff;

            default:
                //is this the best way of dealing with this??
                UnityEngine.Debug.LogWarning("Passed invalid AbilityTrigger " + enumTrigger);
                return -1;
        }
    }
}
