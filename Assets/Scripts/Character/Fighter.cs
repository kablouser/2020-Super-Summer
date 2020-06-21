using System.Collections;
using UnityEngine;
using static CharacterSheet;
using static Armament;

public class Fighter : MonoBehaviour
{
    [System.Serializable]
    public struct AbilityContainer
    {
        public Ability ability;
        public ArmamentPrefab arms;
    }

    public enum InputPhase { down, hold, up }
    public enum AbilityIndex { L1, L2, R1, R2 }

    public static readonly WaitForSeconds staggerWait = new WaitForSeconds(staggerDuration);
    public static readonly StatusEffect staggerEffect = new StatusEffect(staggerDuration, new AttributeValue(Attribute.moveSpeed, -200));

    public const float staggerDuration = 1.0f;

    public const float basePoise = 1.0f;
    public const float poiseGrowth = 1.0f;
    public const float poiseReset = 0.5f;
    public const string damagedStaggerTrigger = "Damaged Stagger";

    public const string leftHandIdle = "Left Hand Idle";
    public const string rightHandIdle = "Right Hand Idle";

    public bool IsStaggered { get; private set; }

    public Animator animator;
    public AttackIndictator attackIndicator;
    public MaterialChanger modelMaterialChanger;
    public Material staggerPoiseMaterial;

    [HideInInspector]
    public AbilityContainer[] currentAbilities;

    [SerializeField]
    private AbilityContainer[] defaultAbilities;

    private Coroutine staggerRoutine;
    private bool isPoised;    
    private float poiseDuration;
    private Coroutine poiseRoutine;
    private bool isPoiseResetted;
    
    private CharacterSheet characterSheet;

    //abilities that have the button held down
    private bool[] heldDown;

    private int lastAbilityIndex = -1;
    private AbilityContainer lastAbility;

    public void UseAbility(AbilityIndex index, bool buttonDown, out bool isProblem) => UseAbility((int)index, buttonDown, out isProblem);
    public void UseAbility(int index, bool buttonDown, out bool isProblem)
    {
        heldDown[index] = buttonDown;
        TriggerAbility(index, buttonDown ? InputPhase.down : InputPhase.up, out isProblem);
    }

    public void TryStopArms(ArmamentPrefab prefab, out bool isProblem)
    {
        if (lastAbility.arms == prefab)
            TryStopLastAbility(out isProblem);
        else isProblem = false;
    }

    public void EquipArmament(ArmamentPrefab arms)
    {
        arms.MapAbilitySet(currentAbilities);

        if (arms.holdMethod == HoldMethod.leftHand)
            animator.SetInteger(leftHandIdle, (int)arms.idleAnimation);
        else if (arms.holdMethod == HoldMethod.rightHand)
            animator.SetInteger(rightHandIdle, (int)arms.idleAnimation);
        else if (arms.holdMethod == HoldMethod.bothHands)
        {
            animator.SetInteger(leftHandIdle, (int)arms.idleAnimation);
            animator.SetInteger(rightHandIdle, (int)arms.idleAnimation);
        }
    }

    public void UnequipArmament(ArmamentPrefab arms)
    {
        //this creates null entries on our ability set
        arms.UnmapAbilitySet(currentAbilities);
        MapNullAbilities();

        if (arms.holdMethod == HoldMethod.leftHand)
            animator.SetInteger(leftHandIdle, (int)IdleAnimation.defaultIdle);
        else if (arms.holdMethod == HoldMethod.rightHand)
            animator.SetInteger(rightHandIdle, (int)IdleAnimation.defaultIdle);
        else if (arms.holdMethod == HoldMethod.bothHands)
        {
            animator.SetInteger(leftHandIdle, (int)IdleAnimation.defaultIdle);
            animator.SetInteger(rightHandIdle, (int)IdleAnimation.defaultIdle);
        }
    }

    public void RicochetStagger(string trigger)
    {
        StartStagger(trigger);
    }

    public void DamagedStagger()
    {
        if (isPoised == false)
        {
            StartStagger(damagedStaggerTrigger);

            if (poiseRoutine != null)
                StopCoroutine(poiseRoutine);
            poiseRoutine = StartCoroutine(PoiseRoutine(staggerPoiseMaterial));
        }
    }

    public bool HasLastAbilityEnded()
    {
        if (lastAbility.ability != null)
            return lastAbility.ability.HasEnded(lastAbility.arms);
        else return true;
    }

    public void TryStopLastAbility(out bool isProblem)
    {
        if (lastAbility.ability != null)
            lastAbility.ability.TryEndUse(lastAbility.arms, out isProblem);
        else isProblem = false;
    }

    private void TriggerAbility(int index, InputPhase phase, out bool isProblem)
    {
        AbilityContainer selectedContainer = currentAbilities[index];

        if (IsStaggered || selectedContainer.ability == null)
        {
            isProblem = true;
            return;
        }

        if(lastAbilityIndex != index)
        {
            if (phase == InputPhase.hold)
            {
                if(HasLastAbilityEnded() == false)
                {
                    isProblem = true;
                    return;
                }
            }
            else
            {
                TryStopLastAbility(out isProblem);
                if (isProblem) return;
            }
        }

        lastAbilityIndex = index;
        lastAbility = selectedContainer;

        selectedContainer.ability.Use(selectedContainer.arms, phase, out isProblem);
        if (isProblem == false && phase == InputPhase.down)
            selectedContainer.ability.Use(selectedContainer.arms, InputPhase.hold, out isProblem);
    }

    private void StartStagger(string trigger)
    {
        if (staggerRoutine != null)
            StopCoroutine(staggerRoutine);
        staggerRoutine = StartCoroutine(StaggerRoutine(trigger));
    }

    private IEnumerator StaggerRoutine(string trigger)
    {
        if (lastAbility.ability != null)
            lastAbility.ability.ForceEndUse(lastAbility.arms);

        animator.SetTrigger(trigger);
        characterSheet.AddStatusEffect(staggerEffect);
        IsStaggered = true;
        yield return staggerWait;

        IsStaggered = false;
    }

    private IEnumerator PoiseRoutine(Material poisedMaterial)
    {
        //start
        if (isPoiseResetted == false)
            poiseDuration += poiseGrowth;

        isPoised = true;
        isPoiseResetted = false;
        modelMaterialChanger.Flicker(poisedMaterial);
        yield return new WaitForSeconds(poiseDuration);        

        //poise ends, if hit again then poise duration increases
        isPoised = false;
        modelMaterialChanger.StopFlicker();
        yield return new WaitForSeconds(poiseReset);

        //poise increase window is gone
        poiseDuration = basePoise;
        isPoiseResetted = true;
    }

    //maps default abilities into null entries
    private void MapNullAbilities()
    {
        for (int i = 0; i < defaultAbilities.Length && i < currentAbilities.Length; i++)
            if(currentAbilities[i].ability == null)
                //container is a struct, so its data is copied
                currentAbilities[i] = defaultAbilities[i];
    }

    private void Awake()
    {
        characterSheet = GetComponent<CharacterSheet>();

        int defaultLength = defaultAbilities.Length;
        int abilitiesLength = Mathf.Max(defaultLength, 4);

        currentAbilities = new AbilityContainer[abilitiesLength];
        MapNullAbilities();

        heldDown = new bool[abilitiesLength];
    }

    private void FixedUpdate()
    {
        for (int i = 0; i < heldDown.Length; i++)
            if (heldDown[i])
                TriggerAbility(i, InputPhase.hold, out _);
    }
}
