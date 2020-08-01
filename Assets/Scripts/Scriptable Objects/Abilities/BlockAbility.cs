using UnityEngine;
using System.Collections;

using static CharacterSheet;
using static Fighter;
using static Armament;

[CreateAssetMenu(fileName = "Block Ability", menuName = "Abilities/Block Ability", order = 2)]
public class BlockAbility : AbilityCreator
{
    [System.Serializable]
    public struct BlockConfig
    {
        public float damageReduction;
        public int ricochet;
        public int poise;
        public EffectCreator useEffect;

        public string toggle;
    }

    public interface IBlock
    {
        AttackIndictator GetAttackIndicator { get; }
        HoldMethod GetHoldMethod { get; }
    }

    public class BlockInstance : AbilityInstance, IAttackListener
    {
        private readonly AttackIndictator attackIndicator;
        private readonly float blockAngle;
        private BlockConfig config;
        private bool isUsing;
        private Coroutine routine;
        private StatusEffect drainEffect;

        public BlockInstance(
            IBlock blockInterface,
            BlockAbility ability,
            CharacterComponents characterComponents) :
            base(characterComponents, ability)
        {
            attackIndicator = blockInterface.GetAttackIndicator;
            blockAngle = ability.blockAngle;
            config = ability.blockConfig;
            drainEffect = config.useEffect.FindAtomic<StatusEffect>();
        }

        public override bool CanUse(InputPhase phase)
        {
            if ((phase == InputPhase.down || phase == InputPhase.hold) &&
                isUsing == false)
                return IsDrained() == false;
            else if (phase == InputPhase.up && isUsing)
                return true;
            else
                return false;
        }

        public override void Use(InputPhase phase)
        {
            if (phase == InputPhase.down || phase == InputPhase.hold)
            {
                characterComponents.animator.SetBool(config.toggle, true);
                characterComponents.characterSheet.AddEffect(config.useEffect, -1);
                characterComponents.characterSheet.AddAttackListener(this);

                attackIndicator.blockAngle = blockAngle;
                attackIndicator.SetBlockIndicator(true);
                isUsing = true;

                routine = characterComponents.fighter.StartCoroutine(BlockRoutine());
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
            if(isUsing)
                EndUse(true);
        }

        public override bool HasEnded()
        {
            return isUsing == false;
        }

        public void OnAttacked(int damage, Vector3 contactPoint,
            out int ricochet, out int reduction, out int poise, out bool canRicochet)
        {
            Movement movement = characterComponents.movement;
            Vector3 contactDirection = contactPoint - attackIndicator.transform.position;
            
            if (isUsing)
            {
                Transform model = movement.bodyRotator;
                float angle = Vector3.Angle(contactDirection, model.forward);

                if (angle <= blockAngle)
                {
                    attackIndicator.SetLastHit(contactDirection.normalized, true);

                    reduction = Mathf.RoundToInt(damage * config.damageReduction);
                    ricochet = config.ricochet;
                    poise = config.poise;
                    canRicochet = true;
                    return;
                }
            }
            ricochet = reduction = poise = 0;
            canRicochet = false;

            attackIndicator.SetLastHit(contactDirection.normalized, false);
        }

        private IEnumerator BlockRoutine()
        {
            if (drainEffect != null)
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
            if (drainEffect == null)
                return false;
            else
                return drainEffect.IsResourcesDrained(characterComponents.characterSheet);
        }

        private void EndUse(bool fade)
        {
            characterComponents.animator.SetBool(config.toggle, false);
            characterComponents.characterSheet.RemoveEffect(config.useEffect);
            characterComponents.characterSheet.RemoveAttackListener(this);

            if (fade)
                attackIndicator.FadeOutBlockIndicator();
            else
                attackIndicator.SetBlockIndicator(false);
            isUsing = false;

            if (routine != null)
                characterComponents.fighter.StopCoroutine(routine);
        }
    }


    public float blockAngle;
    public BlockConfig blockConfig;

    public override AbilityInstance CreateAbility(
        object interfaceObject, 
        CharacterComponents characterComponents)
    {
        if (interfaceObject is IBlock blockInterface)
            return new BlockInstance(blockInterface, this, characterComponents);
        else
            return null;
    }
}
