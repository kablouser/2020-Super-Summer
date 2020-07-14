using UnityEngine;
using static MeleeAbility;
using static BlockAbility;
using static RangedAbility;

public class StandardWeapon : ArmamentPrefab, IMelee, IBlock, IRanged
{
    public Armament.HoldMethod GetHoldMethod => holdMethod;

    DamageBox[] IMelee.GetDamageBoxes => damageBoxes;
    
    AttackIndictator IBlock.GetAttackIndicator => characterComponents.fighter.attackIndicator;

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
