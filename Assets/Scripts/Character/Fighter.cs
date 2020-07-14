using System.Collections;
using UnityEngine;

using static Armament;
using static AbilityCreator;

public class Fighter : MonoBehaviour
{
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

    public static readonly WaitForSeconds staggerWait = new WaitForSeconds(staggerDuration);

    public const float staggerDuration = 1.0f;

    public const float basePoise = 1.0f;
    public const float poiseGrowth = 1.0f;
    public const float poiseReset = 0.5f;
    public const string damagedStaggerTrigger = "Damaged Stagger";

    public const string leftHandIdle = "Left Hand Idle";
    public const string rightHandIdle = "Right Hand Idle";

    public const string leftRicochet = "Left Ricochet";
    public const string rightRicochet = "Right Ricochet";
    public const string bothRicochet = "Both Ricochet";

    public bool IsStaggered { get; private set; }

    public CharacterComponents characterComponents;

    public AttackIndictator attackIndicator;
    public MaterialChanger modelMaterialChanger;
    public Material staggerPoiseMaterial;

    public EffectCreator staggerEffect;

    //[HideInInspector]
    public AbilityContainer[] currentAbilities;

    [SerializeField]
    private DefaultAbility[] defaultAbilities;

    private Coroutine staggerRoutine;
    private bool isPoised;    
    private float poiseDuration;
    private Coroutine poiseRoutine;
    private bool isPoiseResetted;
    
    private CharacterSheet characterSheet;
    private Animator animator;

    //abilities that have the button held down
    private bool[] heldDown;

    private int lastAbilityIndex = -1;
    private AbilityContainer lastAbility;

    public void UseAbility(AbilityIndex index, bool buttonDown, out bool isProblem) =>
        UseAbility((int)index, buttonDown, out isProblem);

    public void UseAbility(int index, bool buttonDown, out bool isProblem)
    {
        heldDown[index] = buttonDown;
        TriggerAbility(index, buttonDown ? InputPhase.down : InputPhase.up, out isProblem);
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
            if (prefab.holdMethod == HoldMethod.leftHand)
                StartStagger(leftRicochet);
            else if (prefab.holdMethod == HoldMethod.rightHand)
                StartStagger(rightRicochet);
            else
                StartStagger(bothRicochet);
            return true;
        }
        else return false;
    }

    public void DamagedStagger(bool doAnimation = true)
    {
        if (isPoised == false)
        {
            if(doAnimation)
                StartStagger(damagedStaggerTrigger);

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

            selectedContainer.ability.Use(phase);
            isProblem = false;
        }
        else
            isProblem = true;
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
            lastAbility.ability.ForceEndUse();

        animator.SetTrigger(trigger);
        characterSheet.AddEffect(staggerEffect);
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

        heldDown = new bool[abilitiesLength];
    }

    private void OnDisable()
    {
        if(lastAbility.ability != null)
            lastAbility.ability.ForceEndUse();
    }

    private void FixedUpdate()
    {
        for (int i = 0; i < heldDown.Length; i++)
            if (heldDown[i])
                TriggerAbility(i, InputPhase.hold, out _);
    }
}
