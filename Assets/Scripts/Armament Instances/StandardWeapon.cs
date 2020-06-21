using UnityEngine;
using static MeleeAbility;
using static BlockAbility;
using static RangedAbility;

public class StandardWeapon : ArmamentPrefab, IMelee, IBlock, IRanged
{
    public bool GetUsing { get; set; }
    public float GetLastUse { get; set; }
    public Coroutine GetRoutine { get; set; }

    DamageBox[] IMelee.GetDamageBoxes => damageBoxes;

    bool IBlock.SetBlocking { set => attackIndicator.SetBlockIndicator(value); }
    OnAttackedHandler IBlock.SetOnAttacked { set => onAttackedHandler = value; }
    Vector3 IBlock.LastHitDirection { set => attackIndicator.SetLastHit(value); }
    float IBlock.SetBlockAngle { set => attackIndicator.blockAngle = value; }

    Transform IRanged.GetSpawnPoint => projectileSpawn;

    [Header("Standard Weapon")]
    public DamageBox[] damageBoxes;
    public Transform projectileSpawn;

    private OnAttackedHandler onAttackedHandler;
    private AttackIndictator attackIndicator;

    public override void AfterSpawned()
    {
        foreach (var box in damageBoxes)
            box.owner = fighter;
        attackIndicator = fighter.attackIndicator;
        GetLastUse = Time.time;
    }

    public override void OnAttacked(int damage, Vector3 contactPoint, out int reduction, out int ricochet, out bool poise)
    {
        if (onAttackedHandler == null)
            base.OnAttacked(damage, contactPoint, out reduction, out ricochet, out poise);
        else
            onAttackedHandler.Invoke(this, damage, contactPoint, out reduction, out ricochet, out poise);
    }
}
