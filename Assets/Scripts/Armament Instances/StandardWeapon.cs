using UnityEngine;
using static MeleeAbility;
using static BlockAbility;
using static RangedAbility;
using static ParryAbility;

public class StandardWeapon : ArmamentPrefab, IMelee, IBlock, IRanged, IParry
{
    public Armament.HoldMethod GetHoldMethod => holdMethod;
    public AttackIndictator GetAttackIndicator => characterComponents.fighter.attackIndicator;

    DamageBox[] IMelee.GetDamageBoxes => damageBoxes;
    Transform IRanged.GetSpawnPoint => projectileSpawn;    

    public DamageBox[] damageBoxes;
    public Transform projectileSpawn;

    public override void AfterSpawn()
    {
        base.AfterSpawn();

        foreach (var box in damageBoxes)
            box.owner = characterComponents.fighter;
    }
}
