using UnityEngine;
using System.Collections;

using static CharacterSheet;
using static Fighter;

public class ParryAbility : Ability, IAttackListener
{
    public ResourceGroup cost;
    public Effect useEffect;
    public AnimationConstants.AbilityTrigger parryTrigger;
    public Timestamps timestamps;
    public BlockAbility.BlockConfig blockConfig;

    private bool isUsing;
    private Coroutine routine;

    public override bool CanUse(InputPhase phase) => 
        phase == InputPhase.down &&
        isUsing == false &&
        characterComponents.characterSheet.HasResources(cost);

    public override void ForceEndUse()
    {
        if (isUsing)
        {
            //this is stage 2 but with fade
            EndParry(true);
            EndAbility();
        }
    }

    public override void TryEndUse(out bool isProblem)
    {
        isProblem = HasEnded() == false;
    }

    public override void Use(InputPhase phase)
    {
        characterComponents.characterSheet.IncreaseResources(-cost);
        characterComponents.animator.SetTrigger(AnimationConstants.EnumToID(parryTrigger));
        isUsing = true;

        if (routine != null)
            StopCoroutine(routine);
        routine = StartCoroutine(ParryRoutine());
    }


    public void OnAttacked(int damage, Vector3 contactPoint, out DefenceFeedback feedback)
    {
        BlockAbility.BlockAttack(characterComponents.movement, characterComponents.attackIndicator,
            blockConfig, damage, contactPoint, out feedback);
    }

    private IEnumerator ParryRoutine()
    {
        AttackIndictator attackIndicator = characterComponents.attackIndicator;

        yield return timestamps.GetNextWait(0);

        //parry start
        characterComponents.characterSheet.AddEffect(useEffect);
        characterComponents.characterSheet.AddAttackListener(this);
        attackIndicator.EnableBlockIndicator(blockConfig.blockAngle);

        yield return timestamps.GetNextWait(1);

        //parry end
        EndParry(false);

        yield return timestamps.GetNextWait(2);

        //ability end
        EndAbility();
    }

    private void EndParry(bool interrupted)
    {
        characterComponents.characterSheet.RemoveAttackListener(this);
        if(interrupted)
            characterComponents.attackIndicator.FadeOutBlockIndicator();
        else
            characterComponents.attackIndicator.DisableBlockIndicator();
    }

    private void EndAbility()
    {
        characterComponents.characterSheet.RemoveEffect(useEffect);
        isUsing = false;
        if (routine != null)
            StopCoroutine(routine);
    }
}
