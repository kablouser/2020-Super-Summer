using UnityEngine;

using static Armament;
using static CharacterSheet;
using static Fighter;

[CreateAssetMenu(fileName = "Parry Ability", menuName = "Abilities/Parry Ability", order = 3)]
public class ParryAbility : AbilityCreator
{
    [System.Serializable]
    public struct ParryConfig
    {
        public float blockAngle;
        public int staminaCost;
        public float damageReduction;
        public int ricochet;
        public int poise;
        public EffectCreator useEffect;

        public string parryTrigger;
    }

    public interface IParry
    {
        HoldMethod GetHoldMethod { get; }
        AttackIndictator GetAttackIndicator { get; }
    }

    public class ParryInstance : AbilityInstance, IAttackListener
    {
        private ParryConfig config;
        private bool isUsing;
        private IParry parryInterface;

        public ParryInstance(IParry parryInterface,
            ParryAbility parryAbility,
            CharacterComponents characterComponents)
            : base(characterComponents, parryAbility)
        {
            this.parryInterface = parryInterface;
            config = parryAbility.config;
        }

        public override bool CanUse(InputPhase phase) => 
            phase == InputPhase.down &&
            isUsing == false &&
            config.staminaCost <= characterComponents.characterSheet.GetResource(Resource.stamina);

        public override void ForceEndUse()
        {
            if (isUsing)
            {
                //this is stage 2 but with fade
                characterComponents.characterSheet.RemoveAttackListener(this);
                parryInterface.GetAttackIndicator.FadeOutBlockIndicator();
                OnAbility(3);
            }
        }

        public override bool HasEnded() => isUsing == false;

        public override void TryEndUse(out bool isProblem)
        {
            isProblem = HasEnded() == false;
        }

        public override void Use(InputPhase phase)
        {
            characterComponents.characterSheet.IncreaseResource(Resource.stamina, -config.staminaCost);
            characterComponents.animationEventListener.OnAbility += OnAbility;
            characterComponents.animator.SetTrigger(config.parryTrigger);
            isUsing = true;
        }

        public void OnAttacked(int damage, Vector3 contactPoint, out int ricochet, out int reduction, out int poise, out bool canRicochet)
        {
            Movement movement = characterComponents.movement;
            Vector3 contactDirection = contactPoint - parryInterface.GetAttackIndicator.transform.position;
            
            Transform model = movement.bodyRotator;
            float angle = Vector3.Angle(contactDirection, model.forward);

            if (angle <= config.blockAngle)
            {
                parryInterface.GetAttackIndicator.SetLastHit(contactDirection.normalized, true);

                reduction = Mathf.RoundToInt(damage * config.damageReduction);
                ricochet = config.ricochet;
                poise = config.poise;
                canRicochet = true;
            }
            else
            {
                parryInterface.GetAttackIndicator.SetLastHit(contactDirection.normalized, false);

                ricochet = reduction = poise = 0;
                canRicochet = false;
            }
        }

        private void OnAbility(int stage)
        {
            if(stage == 1)
            {
                //parry start
                characterComponents.characterSheet.AddEffect(config.useEffect);
                characterComponents.characterSheet.AddAttackListener(this);
                parryInterface.GetAttackIndicator.blockAngle = config.blockAngle;
                parryInterface.GetAttackIndicator.SetBlockIndicator(true);
            }
            else if(stage == 2)
            {
                //parry end
                characterComponents.characterSheet.RemoveAttackListener(this);
                parryInterface.GetAttackIndicator.SetBlockIndicator(false);
            }
            else if(stage == 3)
            {
                //ability end
                characterComponents.characterSheet.RemoveEffect(config.useEffect);
                isUsing = false;
                characterComponents.animationEventListener.OnAbility -= OnAbility;
            }
        }
    }

    public ParryConfig config;

    public override AbilityInstance CreateAbility(object interfaceObject, CharacterComponents characterComponents)
    {
        if (interfaceObject is IParry parryInterface)
            return new ParryInstance(parryInterface, this, characterComponents);
        else
            return null;
    }
}
