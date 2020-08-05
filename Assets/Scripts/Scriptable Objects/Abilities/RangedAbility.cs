using UnityEngine;
using System.Collections;

using static CharacterSheet;
using static Fighter;

public class RangedAbility : Ability
{
    public DamageValue damage;
    public int heft;
    public ResourceGroup cost;
    public Effect useEffect;
    public AnimationConstants.AbilityTrigger attackTrigger;
    public float cooldown;

    public Transform spawnPoint;
    public Projectile projectile;

    public Timestamps timestamps;

    private bool onCooldown;
    private Coroutine routine;

    public override bool CanUse(InputPhase phase)
    {
        return
            phase == InputPhase.down &&
            IsUsing == false &&
            onCooldown == false &&
            characterComponents.characterSheet.HasResources(cost);
    }

    public override void Use(InputPhase phase)
    {
        characterComponents.characterSheet.IncreaseResources(-cost);
        characterComponents.animator.SetTrigger(AnimationConstants.EnumToID(attackTrigger));
        IsUsing = true;
        onCooldown = true;

        if (routine != null)
            StopCoroutine(routine);
        routine = StartCoroutine(RangedRoutine());
    }

    public override void TryEndUse(out bool isProblem)
    {
        isProblem = HasEnded() == false;
    }

    public override void ForceEndUse()
    {
        if(IsUsing)
            EndAbility();
    }

    private IEnumerator RangedRoutine()
    {
        yield return timestamps.GetNextWait(0);

        //shoot projectile
        characterComponents.characterSheet.AddEffect(useEffect);
        Projectile newProjectile = Instantiate(
            projectile,
            spawnPoint.position,
            characterComponents.movement.head.rotation);
        newProjectile.Setup(characterComponents.characterSheet.CalculateAttackDamage(damage),
            heft,
            characterComponents.characterSheet);

        yield return timestamps.GetNextWait(1);

        EndAbility();

        yield return timestamps.GetNextWait(2);

        EndCooldown();
    }

    private void EndAbility()
    {
        IsUsing = false;
        characterComponents.characterSheet.RemoveEffect(useEffect);
    }

    public void EndCooldown()
    {
        onCooldown = false;
    }

    private void OnDisable()
    {
        onCooldown = false;
    }
}
