using UnityEngine;
using static CharacterSheet;
using static Fighter;
using static Armament;

[CreateAssetMenu(fileName = "Melee Ability", menuName = "Abilities/Melee Ability", order = 1)]
public class MeleeAbility : AbilityCreator
{
    [System.Serializable]
    public struct MeleeConfig
    {
        public DamageValue damage;
        public int staminaCost;
        public int heft;
        public EffectCreator useEffect;
        public string attackTrigger;

        [Header("A hold input signal will chain this ability into another")]
        public AbilityCreator holdToChain;
    }

    public interface IMelee
    {
        DamageBox[] GetDamageBoxes { get; }
        HoldMethod GetHoldMethod { get; }
    }

    public class MeleeInstance : AbilityInstance
    {
        private const float maxHoldDifference = 0.1f;

        private readonly DamageBox[] damageBoxes;
        private MeleeConfig config;
        private bool isUsing;
        private AbilityInstance holdToChain;
        private float startUseTime;

        public MeleeInstance(
            IMelee meleeInterface,
            MeleeAbility ability,
            CharacterComponents characterComponents) :
            base(characterComponents, ability)
        {
            damageBoxes = meleeInterface.GetDamageBoxes;
            config = ability.meleeConfig;
            if (ability.meleeConfig.holdToChain != null)
            {
                holdToChain = ability.meleeConfig.holdToChain.CreateAbility(meleeInterface, characterComponents);
#if UNITY_EDITOR
                if (holdToChain == null)
                    Debug.LogError("Chain ability failed interface", meleeInterface as Object);
#endif
            }
        }

        public override bool CanUse(InputPhase phase)
        {
            if (phase == InputPhase.down &&
                isUsing == false &&
                (holdToChain == null || holdToChain.HasEnded()) &&
                config.staminaCost <= characterComponents.characterSheet.GetResource(Resource.stamina))
                return true;

            else if (phase == InputPhase.hold &&
                isUsing &&
                holdToChain != null &&
                (Time.time - startUseTime - holdDuration) < maxHoldDifference &&
                holdToChain.CanUse(InputPhase.down))
                return true;

            else return false;
        }

        public override void Use(InputPhase phase)
        {
            if (phase == InputPhase.down)
            {
                startUseTime = Time.time;
                characterComponents.characterSheet.IncreaseResource(Resource.stamina, -config.staminaCost);
                characterComponents.animationEventListener.OnAbility += OnAbility;
                isUsing = true;

                characterComponents.animator.SetTrigger(config.attackTrigger);
            }
            else if(phase == InputPhase.hold)
            {
                OnAbility(2);
                OnAbility(3);
                holdToChain.Use(InputPhase.down);
            }
        }

        public override void TryEndUse(out bool isProblem)
        {
            isProblem = HasEnded() == false;
            if (isProblem == false && holdToChain != null)
                holdToChain.TryEndUse(out isProblem);
        }

        public override void ForceEndUse()
        {
            OnAbility(2);
            OnAbility(3);
            if(holdToChain != null)
                holdToChain.ForceEndUse();
        }

        public override bool HasEnded()
        {
            return isUsing == false &&
                (holdToChain == null || holdToChain.HasEnded());
        }

        private void OnAbility(int stage)
        {
            if (stage == 1)
            {
                //damage start
                characterComponents.characterSheet.AddEffect(config.useEffect);

                int calculateDamage = characterComponents.characterSheet.CalculateAttackDamage(config.damage);
                foreach (var box in damageBoxes)
                    box.StartAttack(calculateDamage, config.heft);
            }
            else if(stage == 2)
            {
                //damage end
                foreach (var box in damageBoxes)
                    box.StopAttack();
            }
            else if(stage == 3)
            {
                //ability end
                characterComponents.characterSheet.RemoveEffect(config.useEffect);
                characterComponents.animationEventListener.OnAbility -= OnAbility;
                isUsing = false;
            }
        }
    }

    public MeleeConfig meleeConfig;

    public override AbilityInstance CreateAbility(
        object interfaceObject,
        CharacterComponents characterComponents)
    {
        if (interfaceObject is IMelee meleeInterface)
            return new MeleeInstance(meleeInterface, this, characterComponents);
        else
            return null;
    }
}
