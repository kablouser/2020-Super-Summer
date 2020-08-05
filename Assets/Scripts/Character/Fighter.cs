using System.Collections;
using UnityEngine;

public class Fighter : MonoBehaviour
{
    public enum InputPhase { up, down, hold }
    public enum AbilityIndex { L1, L2, R1, R2, MAX }

    public static readonly WaitForSeconds staggerWait = new WaitForSeconds(1.0f);
    public static readonly WaitForSeconds ricochetWait = new WaitForSeconds(1.5f);
    public static readonly WaitForSeconds holdWait = new WaitForSeconds(holdDuration);

    public const float basePoise = 1.0f;
    public const float poiseGrowth = 1.0f;
    public const float poiseReset = 0.5f;
    public const float holdDuration = 0.2f;

    public bool IsStaggered { get; private set; }

    public CharacterComponents characterComponents;
    public MaterialChanger modelMaterialChanger;
    public Effect staggerEffect;
    public Ability[] currentAbilities = new Ability[(int)AbilityIndex.MAX];

    [SerializeField]
    private Ability[] defaultAbilities = new Ability[(int)AbilityIndex.MAX];

    private Coroutine staggerRoutine;
    private bool isPoised;    
    private float poiseDuration;
    private Coroutine poiseRoutine;
    private bool isPoiseResetted;
    
    private CharacterSheet characterSheet;
    private Animator animator;

    //the input phases for each ability
    private InputPhase[] abilityInputs;
    private Coroutine[] switchToHoldRoutine;
    
    private Ability lastAbility;

    public void UseAbility(AbilityIndex index, bool isDown, out bool isProblem) =>
        UseAbility((int)index, isDown, out isProblem);

    public void UseAbility(int index, bool isDown, out bool isProblem)
    {
        if (isDown)
        {
            if (abilityInputs[index] != InputPhase.hold)
            {
                //switch to hold after delay
                if (switchToHoldRoutine[index] != null)
                    StopCoroutine(switchToHoldRoutine[index]);
                switchToHoldRoutine[index] = StartCoroutine(SwitchToHoldRoutine(index));
            }
        }
        //stop switching to hold
        else if (switchToHoldRoutine[index] != null)
            StopCoroutine(switchToHoldRoutine[index]);
        
        abilityInputs[index] = isDown ? InputPhase.down : InputPhase.up;
        TriggerAbility(index, abilityInputs[index], out isProblem);
    }

    ///<summary>will not switch to hold</summary>
    public void UseAbilityDontHold(int index, bool isDown, out bool isProblem)
    {
        //stop switching to hold
        if (!isDown && switchToHoldRoutine[index] != null)
            StopCoroutine(switchToHoldRoutine[index]);

        abilityInputs[index] = isDown ? InputPhase.down : InputPhase.up;
        TriggerAbility(index, abilityInputs[index], out isProblem);
    }

    public void AddAbility(int index, Ability ability)
    {
        if(ability == null)
            RemoveAbility(index);
        else
            currentAbilities[index] = ability;
    }

    public void RemoveAbility(int index)
    {
        if (index < defaultAbilities.Length)
            currentAbilities[index] = defaultAbilities[index];
        else
            currentAbilities[index] = null;
    }

    public bool RicochetStagger()
    {
        if (HasLastAbilityEnded() == false)
        {
            StartStagger(AnimationConstants.Ricochet, ricochetWait);
            return true;
        }
        else return false;
    }

    public void DamagedStagger()
    {
        if (isPoised == false)
        {
            StartStagger(AnimationConstants.Hurt, staggerWait);

            if (poiseRoutine != null)
                StopCoroutine(poiseRoutine);
            poiseRoutine = StartCoroutine(PoiseRoutine());
        }
    }

    public bool HasLastAbilityEnded()
    {
        if (lastAbility != null)
            return lastAbility.HasEnded();
        else return true;
    }

    public void TryStopLastAbility(out bool isProblem)
    {
        if (lastAbility != null)
            lastAbility.TryEndUse(out isProblem);
        else isProblem = false;
    }

    private void TriggerAbility(int index, InputPhase phase, out bool isProblem)
    {
        if (IsStaggered || currentAbilities[index] == null)
        {
            isProblem = true;
            return;
        }

        bool checkedCanUse = false;

        if(lastAbility != currentAbilities[index])
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
                if (currentAbilities[index].CanUse(phase))
                {
                    TryStopLastAbility(out isProblem);
                    if (isProblem) return;
                }
                else
                {
                    isProblem = true;
                    return;
                }
                checkedCanUse = true;
            }
        }

        if (checkedCanUse || currentAbilities[index].CanUse(phase))
        {
            lastAbility = currentAbilities[index];
            animator.SetBool(AnimationConstants.Mirror, lastAbility.Mirror);
            lastAbility.Use(phase);
            isProblem = false;
        }
        else
            isProblem = true;
    }

    private void StartStagger(int triggerId, WaitForSeconds stunDuration)
    {
        if (staggerRoutine != null)
            StopCoroutine(staggerRoutine);
        staggerRoutine = StartCoroutine(StaggerRoutine(triggerId, stunDuration));
    }

    private IEnumerator StaggerRoutine(int triggerId, WaitForSeconds stunDuration)
    {
        if (lastAbility != null)
            lastAbility.ForceEndUse();

        animator.SetTrigger(triggerId);
        characterSheet.AddEffect(staggerEffect);
        IsStaggered = true;
        yield return stunDuration;

        ResetStagger();
    }

    private IEnumerator PoiseRoutine()
    {
        //start
        if (isPoiseResetted == false)
            poiseDuration += poiseGrowth;

        isPoised = true;
        isPoiseResetted = false;
        modelMaterialChanger.Flicker();
        yield return new WaitForSeconds(poiseDuration);        

        //poise ends, if hit again then poise duration increases
        isPoised = false;
        modelMaterialChanger.StopFlicker();
        yield return new WaitForSeconds(poiseReset);

        //poise increase window is gone
        ResetPoise();
    }

    private IEnumerator SwitchToHoldRoutine(int index)
    {
        yield return holdWait;
        abilityInputs[index] = InputPhase.hold;
    }

    private void ResetStagger()
    {
        characterSheet.RemoveEffect(staggerEffect);
        IsStaggered = false;
    }

    private void ResetPoise()
    {
        poiseDuration = basePoise;
        isPoiseResetted = true;
    }

    private void Awake()
    {
        characterSheet = characterComponents.characterSheet;
        animator = characterComponents.animator;

        int defaultLength = defaultAbilities.Length;
        int abilitiesLength = Mathf.Max(defaultLength, 4);

        for(int i = 0; i < abilitiesLength && i < defaultLength; i++)
            currentAbilities[i] = defaultAbilities[i];

        abilityInputs = new InputPhase[abilitiesLength];
        switchToHoldRoutine = new Coroutine[abilitiesLength];
    }

    private void OnDisable()
    {
        if(lastAbility != null)
            lastAbility.ForceEndUse();
        //reset all coroutines
        ResetStagger();
        ResetPoise();
    }

    private void FixedUpdate()
    {
        for (int i = 0; i < abilityInputs.Length; i++)
            if (abilityInputs[i] == InputPhase.hold)
                TriggerAbility(i, InputPhase.hold, out _);
    }
}
