using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

using static Armament;
using static AbilityCreator;

public class Fighter : MonoBehaviour
{
    public interface IAbilityMirror
    {
        bool DoMirror { get; }
    }
    [System.Serializable]
    public struct AbilityContainer
    {
        public AbilityInstance ability;
        public MonoBehaviour interfaceObject;
    }
    [System.Serializable]
    public struct DefaultAbility
    {        
        public AbilityCreator creator;
        public MonoBehaviour interfaceObject;
        [HideInInspector]
        public AbilityInstance instance;
    }

    public enum InputPhase { down, hold, up }
    public enum AbilityIndex { L1, L2, R1, R2 }

    public static readonly WaitForSeconds staggerWait = new WaitForSeconds(1.0f);
    public static readonly WaitForSeconds ricochetWait = new WaitForSeconds(1.5f);
    public static readonly WaitForSeconds holdWait = new WaitForSeconds(holdDuration);

    public const float basePoise = 1.0f;
    public const float poiseGrowth = 1.0f;
    public const float poiseReset = 0.5f;
    public const float holdDuration = 0.2f;

    public bool IsStaggered { get; private set; }

    public CharacterComponents characterComponents;

    public AttackIndictator attackIndicator;
    public MaterialChanger modelMaterialChanger;
    public Material staggerPoiseMaterial;

    public EffectCreator staggerEffect;

    //[HideInInspector]
    public AbilityContainer[] currentAbilities;

    [SerializeField]
    private DefaultAbility[] defaultAbilities = null;

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

    private int lastAbilityIndex = -1;
    private AbilityContainer lastAbility;

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

    public void TryStopArms(ArmamentPrefab prefab, out bool isProblem)
    {
        if (lastAbility.interfaceObject == prefab)
            TryStopLastAbility(out isProblem);
        else isProblem = false;
    }

    public void AddAbility(int index, AbilityInstance ability, MonoBehaviour interfaceObject)
    {
        if(ability == null)
            RemoveAbility(index);
        else
        {
            currentAbilities[index].ability = ability;
            currentAbilities[index].interfaceObject = interfaceObject;
        }
    }

    public void RemoveAbility(int index)
    {
        if (index < defaultAbilities.Length)
        {
            currentAbilities[index].ability = defaultAbilities[index].instance;
            currentAbilities[index].interfaceObject = defaultAbilities[index].interfaceObject;
        }
        else
        {
            currentAbilities[index].ability = null;
            currentAbilities[index].interfaceObject = null;
        }
    }

    public bool RicochetStagger()
    {
        if (lastAbility.ability.HasEnded() == false &&
           lastAbility.interfaceObject is ArmamentPrefab prefab &&
           prefab.holdMethod != HoldMethod.none)
        {
            StartStagger(AnimationConstants.Ricochet, ricochetWait);
            //if(prefab.holdMethod == HoldMethod.bothHands)
            //  use a different animation
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
            poiseRoutine = StartCoroutine(PoiseRoutine(staggerPoiseMaterial));
        }
    }

    public bool HasLastAbilityEnded()
    {
        if (lastAbility.ability != null)
            return lastAbility.ability.HasEnded();
        else return true;
    }

    public void TryStopLastAbility(out bool isProblem)
    {
        if (lastAbility.ability != null)
            lastAbility.ability.TryEndUse(out isProblem);
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

        bool checkedCanUse = false;

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
                if (selectedContainer.ability.CanUse(phase))
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

        if (checkedCanUse || selectedContainer.ability.CanUse(phase))
        {
            lastAbilityIndex = index;
            lastAbility = selectedContainer;

            animator.SetBool(AnimationConstants.Mirror,
                selectedContainer.interfaceObject is IAbilityMirror abilityMirror &&
                abilityMirror.DoMirror);

            selectedContainer.ability.Use(phase);
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
        if (lastAbility.ability != null)
            lastAbility.ability.ForceEndUse();

        animator.SetTrigger(triggerId);
        characterSheet.AddEffect(staggerEffect);
        IsStaggered = true;
        yield return stunDuration;

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

    private IEnumerator SwitchToHoldRoutine(int index)
    {
        yield return holdWait;
        abilityInputs[index] = InputPhase.hold;
    }

    private void Awake()
    {
        characterSheet = characterComponents.characterSheet;
        animator = characterComponents.animator;

        int defaultLength = defaultAbilities.Length;
        int abilitiesLength = Mathf.Max(defaultLength, 4);

        currentAbilities = new AbilityContainer[abilitiesLength];
        for(int i = 0; i < abilitiesLength && i < defaultLength; i++)
        {
            var newAbility = defaultAbilities[i].creator.CreateAbility(
                defaultAbilities[i].interfaceObject, 
                characterComponents);

            currentAbilities[i].ability = defaultAbilities[i].instance = newAbility;
            currentAbilities[i].interfaceObject = defaultAbilities[i].interfaceObject;
        }

        abilityInputs = new InputPhase[abilitiesLength];
        switchToHoldRoutine = new Coroutine[abilitiesLength];
    }

    private void OnDisable()
    {
        if(lastAbility.ability != null)
            lastAbility.ability.ForceEndUse();
    }

    private void FixedUpdate()
    {
        for (int i = 0; i < abilityInputs.Length; i++)
            if (abilityInputs[i] == InputPhase.hold)
                TriggerAbility(i, InputPhase.hold, out _);
    }
}
