using UnityEngine;
using System.Collections;
using static CharacterSheet;
using static Fighter;

[CreateAssetMenu(fileName = "Melee Ability", menuName = "Ability/Melee Ability", order = 1)]
public class MeleeAbility : Ability
{
    [System.Serializable]
    public struct MeleeConfig
    {
        [Header("Combat Numbers")]
        public AttributeScaler damage;
        public int staggerThreshold;
        public int staminaCost;
        public StatusEffect activeEffect;
        [Header("Animation and Timing")]
        public string attackTrigger;
        public string ricochetTrigger;
        public float damageStartTime;
        public float damageEndTime;
        public float attackEnd;
        public float cooldown;
    }

    //This lets the inspector see the meleeConfigs fields (because unity cannot serialize generic classes)
    [System.Serializable]
    public class ConfigWrapper : HoldMethodGroup<MeleeConfig> {}

    public interface IMelee
    {
        DamageBox[] GetDamageBoxes { get; }
        Coroutine GetRoutine { get; set; }
        bool GetUsing { get; set; }
        float GetLastUse { get; set; }
    }

    public ConfigWrapper meleeConfigs;

    public override void Use(ArmamentPrefab arms, InputPhase phase, out bool isProblem)
    {
        if(phase != InputPhase.down)
        {
            isProblem = false;
            return;
        }

        IMelee meleeInterface = (IMelee)arms;
        CharacterSheet characterSheet = arms.characterSheet;
        MeleeConfig config = meleeConfigs.GetHand(arms.holdMethod);
        Animator animator = arms.fighter.animator;

        if (meleeInterface.GetUsing == false &&
            meleeInterface.GetLastUse + config.cooldown < Time.time && 
            characterSheet.ExpendResource(Resource.stamina, config.staminaCost))
        {
            isProblem = false;
            meleeInterface.GetRoutine = arms.StartCoroutine(AttackRoutine(meleeInterface, characterSheet, config, animator));
        }
        else isProblem = true;
    }

    public override void TryEndUse(ArmamentPrefab arms, out bool isProblem)
    {
        isProblem = HasEnded(arms) == false;
    }

    public override void ForceEndUse(ArmamentPrefab arms)
    {
        IMelee meleeInterface = (IMelee)arms;
        MeleeConfig config = meleeConfigs.GetHand(arms.holdMethod);

        foreach (var damageBox in meleeInterface.GetDamageBoxes)
            damageBox.StopAttack();

        arms.characterSheet.RemoveStatusEffect(config.activeEffect);

        var routine = meleeInterface.GetRoutine;
        if (routine != null)
            arms.StopCoroutine(meleeInterface.GetRoutine);

        meleeInterface.GetUsing = false;
    }

    public override bool HasEnded(ArmamentPrefab arms)
    {
        IMelee meleeInterface = (IMelee)arms;
        return meleeInterface.GetUsing == false;
    }

    private IEnumerator AttackRoutine(IMelee meleeInterface, CharacterSheet characterSheet, MeleeConfig config, Animator animator)
    {
        //windup
        float nextWait = config.damageStartTime;
        animator.SetTrigger(config.attackTrigger);
        meleeInterface.GetUsing = true;
        meleeInterface.GetLastUse = Time.time;
        yield return new WaitForSeconds(nextWait);

        //damage start
        characterSheet.AddStatusEffect(config.activeEffect);
        nextWait = config.damageEndTime - config.damageStartTime;
        foreach (var box in meleeInterface.GetDamageBoxes)
            box.StartAttack(config.damage.CalculateValue(characterSheet), config.staggerThreshold, config.ricochetTrigger);
        yield return new WaitForSeconds(nextWait);

        //damage end
        nextWait = config.attackEnd - config.damageEndTime;        
        foreach (var box in meleeInterface.GetDamageBoxes)
            box.StopAttack();
        yield return new WaitForSeconds(nextWait);

        //finally, you can attack again
        meleeInterface.GetUsing = false;
    }

    [ContextMenu("Copy Left Hand Configs")]
    public void CopyLeftHandConfigs()
    {
        meleeConfigs.bothHands = meleeConfigs.rightHand = meleeConfigs.leftHand;
    }
}
