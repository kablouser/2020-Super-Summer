using UnityEngine;
using System.Collections;
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
        public float damageStartTime;
        public float damageEndTime;
        public float attackEnd;
    }

    //This lets the inspector see the meleeConfigs fields (because unity cannot serialize generic classes)
    //but alpha version fixes this problem
    [System.Serializable]
    public class ConfigWrapper : HoldMethodGroup<MeleeConfig> {}

    public interface IMelee
    {
        DamageBox[] GetDamageBoxes { get; }
        HoldMethod GetHoldMethod { get; }
    }

    public class MeleeInstance : AbilityInstance
    {
        private readonly DamageBox[] damageBoxes;
        private MeleeConfig config;
        private Coroutine routine;
        private bool isUsing;

        public MeleeInstance(
            IMelee meleeInterface,
            MeleeAbility ability,
            CharacterComponents characterComponents) :
            base(characterComponents, ability)
        {
            damageBoxes = meleeInterface.GetDamageBoxes;
            config = ability.meleeConfigs.GetHand(meleeInterface.GetHoldMethod);
        }

        public override bool CanUse(InputPhase phase)
        {
            return
                phase == InputPhase.down &&
                isUsing == false &&
                config.staminaCost <= characterComponents.characterSheet.GetResource(Resource.stamina);
        }

        public override void Use(InputPhase phase)
        {
            characterComponents.characterSheet.IncreaseResource(Resource.stamina, -config.staminaCost);
            routine = characterComponents.fighter.StartCoroutine(AttackRoutine());
        }

        public override void TryEndUse(out bool isProblem)
        {
            isProblem = HasEnded() == false;
        }

        public override void ForceEndUse()
        {
            foreach (var damageBox in damageBoxes)
                damageBox.StopAttack();

            characterComponents.characterSheet.RemoveEffect(config.useEffect);

            if (routine != null)
            {
                characterComponents.fighter.StopCoroutine(routine);
                routine = null;
            }

            isUsing = false;
        }

        public override bool HasEnded()
        {
            return isUsing == false;
        }

        private IEnumerator AttackRoutine()
        {
            //windup
            float nextWait = config.damageStartTime;
            characterComponents.animator.SetTrigger(config.attackTrigger);
            isUsing = true;
            yield return new WaitForSeconds(nextWait);

            //damage start
            characterComponents.characterSheet.AddEffect(
                config.useEffect,
                config.attackEnd - config.damageStartTime);
            nextWait = config.damageEndTime - config.damageStartTime;
            foreach (var box in damageBoxes)
                box.StartAttack(
                    characterComponents.characterSheet.CalculateAttackDamage(config.damage),
                    config.heft);
            yield return new WaitForSeconds(nextWait);

            //damage end
            nextWait = config.attackEnd - config.damageEndTime;
            foreach (var box in damageBoxes)
                box.StopAttack();
            yield return new WaitForSeconds(nextWait);

            //finally, you can attack again
            isUsing = false;
        }
    }

    public ConfigWrapper meleeConfigs;

    public override AbilityInstance CreateAbility(
        object interfaceObject,
        CharacterComponents characterComponents)
    {
        if (interfaceObject is IMelee meleeInterface)
            return new MeleeInstance(meleeInterface, this, characterComponents);
        else
            return null;
    }

    [ContextMenu("Copy Left Hand Configs")] public void CopyLeftHandConfigs()
    {
        meleeConfigs.bothHands = meleeConfigs.rightHand = meleeConfigs.leftHand;
    }
}
