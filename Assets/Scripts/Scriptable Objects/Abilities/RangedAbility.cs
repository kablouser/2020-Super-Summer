using UnityEngine;
using System.Collections;
using static CharacterSheet;
using static Fighter;
using static Armament;

[CreateAssetMenu(fileName = "Ranged Ability", menuName = "Abilities/Ranged Ability", order = 3)]
public class RangedAbility : AbilityCreator
{
    [System.Serializable]
    public struct RangedConfig
    {
        public DamageValue damage;
        public int heft;
        public ResourceValue[] costs;
        public EffectCreator useEffect;

        public string attackTrigger;
        public float spawnTime;
        public float attackEnd;
        public float cooldown;
    }

    public interface IRanged
    {
        Transform GetSpawnPoint { get; }
        HoldMethod GetHoldMethod { get; }
    }

    public class RangedInstance : AbilityInstance
    {
        private RangedConfig config;
        private bool isUsing;
        private float lastUse;
        private Transform spawnPoint;
        private Coroutine routine;
        private Projectile projectile;

        public RangedInstance(
            IRanged rangedInterface,
            RangedAbility ability,
            CharacterComponents characterComponents) :
            base(characterComponents, ability)
        {
            config = ability.rangedConfig;
            lastUse = Mathf.NegativeInfinity;
            spawnPoint = rangedInterface.GetSpawnPoint;
            projectile = ability.projectile;
        }

        public override bool CanUse(InputPhase phase)
        {
            return 
                phase == InputPhase.down &&
                isUsing == false &&
                lastUse + config.cooldown < Time.time &&
                characterComponents.characterSheet.HasResources(config.costs);
        }

        public override void Use(InputPhase phase)
        {
            characterComponents.characterSheet.ExpendResources(config.costs);
            routine = characterComponents.fighter.StartCoroutine(AttackRoutine());
        }

        public override void TryEndUse(out bool isProblem)
        {
            isProblem = HasEnded() == false;
        }

        public override void ForceEndUse()
        {
            characterComponents.characterSheet.RemoveEffect(config.useEffect);
            isUsing = false;
            if (routine != null)
                characterComponents.fighter.StopCoroutine(routine);
        }

        public override bool HasEnded()
        {
            return isUsing == false;
        }

        private IEnumerator AttackRoutine()
        {
            //windup
            float nextWait = config.spawnTime;
            characterComponents.animator.SetTrigger(config.attackTrigger);
            isUsing = true;
            lastUse = Time.time;
            yield return new WaitForSeconds(nextWait);

            //shoot projectile
            nextWait = config.attackEnd - config.spawnTime;
            characterComponents.characterSheet.AddEffect(
                config.useEffect,
                nextWait);
            Projectile prefab = Instantiate(
                projectile,
                spawnPoint.position,
                characterComponents.movement.head.rotation);
            prefab.damage = characterComponents.characterSheet.CalculateAttackDamage(config.damage);
            prefab.heft = config.heft;
            prefab.shooter = characterComponents.characterSheet;
            yield return new WaitForSeconds(nextWait);

            //finally, the ability has stopped
            isUsing = false;
        }
    }

    public Projectile projectile;
    public RangedConfig rangedConfig;    

    public override AbilityInstance CreateAbility(
        object interfaceObject,
        CharacterComponents characterComponents)
    {
        if (interfaceObject is IRanged rangedInterface)
            return new RangedInstance(rangedInterface, this, characterComponents);
        else
            return null;
    }
}
