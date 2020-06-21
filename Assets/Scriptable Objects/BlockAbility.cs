using UnityEngine;
using static CharacterSheet;
using static Fighter;

[CreateAssetMenu(fileName = "Block Ability", menuName = "Ability/Block Ability", order = 2)]
public class BlockAbility : Ability
{
    [System.Serializable]
    public struct BlockConfig
    {
        [Header("Combat Numbers")]
        public float damageReduction;
        public int ricochet;
        public StatusEffect activeEffect;
        [Header("Animation and Timing")]
        public string toggle;
    }

    //This lets the inspector see the meleeConfigs fields (because unity cannot serialize generic classes)
    [System.Serializable]
    public class ConfigWrapper : HoldMethodGroup<BlockConfig> { }

    public interface IBlock
    {
        bool SetBlocking { set; }
        ArmamentPrefab.OnAttackedHandler SetOnAttacked { set; }
        Vector3 LastHitDirection { set; }
        float SetBlockAngle { set; }
        bool GetUsing { get; set; }
    }

    public float blockAngle;
    public ConfigWrapper blockConfigs;

    private readonly WaitForFixedUpdate routineWait = new WaitForFixedUpdate();

    public override void Use(ArmamentPrefab arms, InputPhase phase, out bool isProblem)
    {
        IBlock blockInterface = (IBlock)arms;
        CharacterSheet characterSheet = arms.characterSheet;
        BlockConfig config = blockConfigs.GetHand(arms.holdMethod);
        Animator animator = arms.fighter.animator;

        isProblem = false;
        if (phase == InputPhase.hold && blockInterface.GetUsing == false)
        {
            if (0 < characterSheet.GetResource(Resource.stamina))
            {
                animator.SetBool(config.toggle, true);
                characterSheet.AddStatusEffect(config.activeEffect);

                blockInterface.SetBlockAngle = blockAngle;
                blockInterface.SetOnAttacked = BlockDamage;
                blockInterface.SetBlocking = true;
                blockInterface.GetUsing = true;
            }
            else isProblem = true;
        }
        else if (phase == InputPhase.up && blockInterface.GetUsing)
        {
            animator.SetBool(config.toggle, false);
            characterSheet.RemoveStatusEffect(config.activeEffect);

            blockInterface.SetOnAttacked = null;
            blockInterface.SetBlocking = false;
            blockInterface.GetUsing = false;
        }
    }

    public override void TryEndUse(ArmamentPrefab arms, out bool isProblem)
    {
        Use(arms, InputPhase.up, out isProblem);
    }

    public override void ForceEndUse(ArmamentPrefab arms)
    {
        Use(arms, InputPhase.up, out _);
    }

    public override bool HasEnded(ArmamentPrefab arms)
    {
        return ((IBlock)arms).GetUsing == false;
    }

    private void BlockDamage(ArmamentPrefab arms, int damage, Vector3 contactPoint, out int reduction, out int ricochet, out bool poise)
    {        
        IBlock blockInterface = (IBlock)arms;

        Movement movement = arms.fighter.GetComponent<Movement>();
        Transform modelCenter = movement.modelCenter;
        Vector3 contactDirection = contactPoint - modelCenter.position;
        blockInterface.LastHitDirection = contactDirection.normalized;

        if (blockInterface.GetUsing)
        {
            Transform model = movement.model;
            float angle = Vector3.Angle(contactDirection, model.forward);

            if (angle <= blockAngle)
            {
                BlockConfig config = blockConfigs.GetHand(arms.holdMethod);
                reduction = Mathf.RoundToInt(damage * config.damageReduction);
                ricochet = config.ricochet;
                poise = true;
                return;
            }
        }
        reduction = ricochet = 0;
        poise = false;
    }
    
    [ContextMenu("Copy Left Hand Configs")] public void CopyLeftHandConfigs()
    {
        blockConfigs.bothHands = blockConfigs.rightHand = blockConfigs.leftHand;
    }
}
