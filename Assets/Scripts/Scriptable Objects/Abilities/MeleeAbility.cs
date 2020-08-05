using UnityEngine;
using System.Collections;

using static CharacterSheet;
using static Fighter;

public class MeleeAbility : ChainableAbility
{
    public DamageValue damage;
    public ResourceGroup cost;
    public int heft;
    public Effect useEffect;
    public AnimationConstants.AbilityTrigger attackTrigger;
    public DamageBox[] damageBoxes;

    public Timestamps timestamps;

    private Coroutine routine;

    public override bool CanUse(InputPhase phase)
    {
        if (base.CanUse(phase))
            return true;

        else if (phase == InputPhase.down &&
            HasEnded() &&
            characterComponents.characterSheet.HasResources(cost))
            return true;

        else return false;
    }

    public override void Use(InputPhase phase)
    {
        if (phase == InputPhase.down)
        {
            characterComponents.characterSheet.IncreaseResources(-cost);
            IsUsing = true;
            characterComponents.animator.SetTrigger(AnimationConstants.EnumToID(attackTrigger));

            if (routine != null)
                StopCoroutine(routine);
            routine = StartCoroutine(AttackRoutine());
        }

        base.Use(phase);
        if (ChainIsUsing)
        {
            EndDamage();
            EndAbility();
        }
    }

    public override void TryEndUse(out bool isProblem)
    {
        isProblem = HasEnded() == false;
        if (isProblem == false)
            base.TryEndUse(out isProblem);
    }

    public override void ForceEndUse()
    {
        if (IsUsing)
        {
            EndDamage();
            EndAbility();
        }
        base.ForceEndUse();
    }

    public override bool HasEnded()
    {
        return IsUsing == false && base.HasEnded();
    }

    private IEnumerator AttackRoutine()
    {
        yield return timestamps.GetNextWait(0);

        //damage start
        characterComponents.characterSheet.AddEffect(useEffect);
        int calculateDamage = characterComponents.characterSheet.CalculateAttackDamage(damage);
        foreach (var box in damageBoxes)
            box.StartAttack(characterComponents.fighter, calculateDamage, heft);

        yield return timestamps.GetNextWait(1);

        //damage end
        EndDamage();

        yield return timestamps.GetNextWait(2);

        //ability end
        EndAbility();
    }

    private void EndDamage()
    {
        foreach (var box in damageBoxes)
            box.StopAttack();
    }

    private void EndAbility()
    {
        characterComponents.characterSheet.RemoveEffect(useEffect);
        IsUsing = false;

        if (routine != null)
            StopCoroutine(routine);
    }
}
