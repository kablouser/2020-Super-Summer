using UnityEngine;
using System.Collections;
using static CharacterSheet;
using static Fighter;

[CreateAssetMenu(fileName = "Ranged Ability", menuName = "Ability/Ranged Ability", order = 3)]
public class RangedAbility : Ability
{
    [System.Serializable]
    public struct RangedConfig
    {
        [Header("Combat Numbers")]
        public AttributeScaler damage;
        public ResourceValue[] costs;
        public StatusEffect activeEffect;
        [Header("Animation and Timing")]
        public string attackTrigger;
        public float spawnTime;
        public float attackEnd;
        public float cooldown;
    }

    //This lets the inspector see the meleeConfigs fields (because unity cannot serialize generic classes)
    [System.Serializable]
    public class ConfigWrapper : HoldMethodGroup<RangedConfig> { }

    public interface IRanged
    {
        Transform GetSpawnPoint { get; }
        bool GetUsing { get; set; }
        float GetLastUse { get; set; }
        Coroutine GetRoutine { get; set; }        
    }

    public Projectile projectile;
    public ConfigWrapper rangedConfigs;    

    public override void Use(ArmamentPrefab arms, InputPhase phase, out bool isProblem)
    {
        if (phase != InputPhase.down)
        {
            isProblem = false;
            return;
        }
                
        IRanged rangedInterface = (IRanged)arms;
        CharacterSheet characterSheet = arms.characterSheet;
        RangedConfig config = rangedConfigs.GetHand(arms.holdMethod);
        Animator animator = arms.fighter.animator;

        if (rangedInterface.GetUsing == false &&
            rangedInterface.GetLastUse + config.cooldown < Time.time &&
            characterSheet.ExpendResources(config.costs))
        {
            isProblem = false;
            rangedInterface.GetRoutine = arms.StartCoroutine(AttackRoutine(rangedInterface, config, characterSheet, animator,
                arms.equipment.GetComponent<Movement>().head));
        }
        else isProblem = true;
    }

    public override void TryEndUse(ArmamentPrefab arms, out bool isProblem)
    {
        isProblem = HasEnded(arms) == false;
    }

    public override void ForceEndUse(ArmamentPrefab arms)
    {
        IRanged rangedInterface = (IRanged)arms;
        RangedConfig config = rangedConfigs.GetHand(arms.holdMethod);

        arms.characterSheet.RemoveStatusEffect(config.activeEffect);
        rangedInterface.GetUsing = false;
        var routine = rangedInterface.GetRoutine;
        if (routine != null)
            arms.StopCoroutine(routine);
    }

    public override bool HasEnded(ArmamentPrefab arms)
    {
        IRanged rangedInterface = (IRanged)arms;
        return rangedInterface.GetUsing == false;
    }

    private IEnumerator AttackRoutine(IRanged rangedInterface, RangedConfig config, CharacterSheet characterSheet, Animator animator, Transform headTransform)
    {
        //windup
        float nextWait = config.spawnTime;
        animator.SetTrigger(config.attackTrigger);
        rangedInterface.GetUsing = true;
        rangedInterface.GetLastUse = Time.time;
        yield return new WaitForSeconds(nextWait);

        //shoot projectile
        nextWait = config.attackEnd - config.spawnTime;
        characterSheet.AddStatusEffect(config.activeEffect);
        Projectile prefab = Instantiate(projectile, rangedInterface.GetSpawnPoint.position, headTransform.rotation);
        prefab.damage = config.damage.CalculateValue(characterSheet);
        prefab.shooter = characterSheet;
        yield return new WaitForSeconds(nextWait);

        //finally, the ability has stopped
        rangedInterface.GetUsing = false;
    }

    [ContextMenu("Copy Left Hand Configs")]
    public void CopyLeftHandConfigs()
    {
        rangedConfigs.bothHands = rangedConfigs.rightHand = rangedConfigs.leftHand;
    }
}
