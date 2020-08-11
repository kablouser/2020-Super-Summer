using UnityEngine;
using System.Collections;

using static CharacterSheet;
using static Fighter;

public class BlockAbility : Ability, IAttackListener
{
    [System.Serializable]
    public struct BlockConfig
    {
        public float blockAngle, damageReduction;
        public int ricochet, poise;
    }

    public ResourceGroup cost;

    public Effect useEffect;

    public AnimationConstants.AbilityTrigger enableTrigger;
    public AnimationConstants.AbilityTrigger disableTrigger;

    public BlockConfig blockConfig;
    
    private Coroutine routine;

    public static void BlockAttack(
        AttackIndictator attackIndicator, BlockConfig block,
        int damage, Vector3 contactPoint, out DefenceFeedback feedback)
    {
        Vector3 contactDirection = contactPoint - attackIndicator.transform.position;
        float angle = Vector3.Angle(contactDirection, attackIndicator.blockIndicator.transform.forward);

        if (angle <= block.blockAngle)
        {
            attackIndicator.SetLastHit(contactDirection.normalized, true);

            feedback = new DefenceFeedback(
                true, block.ricochet,
                Mathf.RoundToInt(damage * block.damageReduction),
                block.poise);
        }
        else
        {
            attackIndicator.SetLastHit(contactDirection.normalized, false);

            feedback = DefenceFeedback.NoDefence;
        }
    }

    public override bool CanUse(InputPhase phase)
    {
        if ((phase == InputPhase.down || phase == InputPhase.hold) &&
            HasEnded())
            return characterComponents.characterSheet.HasResources(cost) &&
                IsDrained() == false;
        else if (phase == InputPhase.up && HasEnded() == false)
            return true;
        else
            return false;
    }

    public override void Use(InputPhase phase)
    {
        if (phase == InputPhase.down || phase == InputPhase.hold)
        {
            characterComponents.animator.SetTrigger(AnimationConstants.EnumToID(enableTrigger));
            characterComponents.animator.ResetTrigger(AnimationConstants.EnumToID(disableTrigger));
            characterComponents.characterSheet.AddEffect(useEffect, -1);
            characterComponents.characterSheet.AddAttackListener(this);
            characterComponents.characterSheet.IncreaseResources(-cost);
            characterComponents.attackIndicator.EnableBlockIndicator(blockConfig.blockAngle);
            IsUsing = true;

            routine = StartCoroutine(BlockRoutine());
        }
        else if (phase == InputPhase.up)
        {
            EndUse(false);
        }
    }

    public override void TryEndUse(out bool isProblem)
    {
        if (CanUse(InputPhase.up))
            Use(InputPhase.up);

        isProblem = false;
    }

    public override void ForceEndUse()
    {
        if (IsUsing)
            EndUse(true);
    }

    public void OnAttacked(int damage, Vector3 contactPoint, CharacterComponents character, out DefenceFeedback feedback)
    {
        BlockAttack(characterComponents.attackIndicator,
            blockConfig, damage, contactPoint, out feedback);
    }

    private IEnumerator BlockRoutine()
    {
        if (useEffect != null)
        {
            do
            {
                yield return CoroutineConstants.waitFixed;
            }
            while (IsDrained() == false);

            ForceEndUse();
        }
    }

    private bool IsDrained()
    {
        if (useEffect == null) return false;

        StatusEffect statusEffect = useEffect.IsA<StatusEffect>();
        if (statusEffect == null) return false;

        return statusEffect.IsResourcesDrained(characterComponents.characterSheet);
    }

    private void EndUse(bool fade)
    {
        characterComponents.animator.ResetTrigger(AnimationConstants.EnumToID(enableTrigger));
        characterComponents.animator.SetTrigger(AnimationConstants.EnumToID(disableTrigger));
        characterComponents.characterSheet.RemoveEffect(useEffect);
        characterComponents.characterSheet.RemoveAttackListener(this);

        if (fade)
            characterComponents.attackIndicator.FadeOutBlockIndicator();
        else
            characterComponents.attackIndicator.DisableBlockIndicator();
        IsUsing = false;

        if (routine != null)
            StopCoroutine(routine);
    }
}
