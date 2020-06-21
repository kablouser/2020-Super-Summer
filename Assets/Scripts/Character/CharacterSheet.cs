using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(LifeToggler))]
[RequireComponent(typeof(Movement))]
[RequireComponent(typeof(Equipment))]
[RequireComponent(typeof(Fighter))]
public class CharacterSheet : MonoBehaviour
{    
    [System.Serializable] public class StatusEffect
    {
        public string name;
        [Tooltip("Set to -1 for indefinite duration")]
        public float duration;
        public AttributeValue[] attributes;
        public ResourceValue[] maxResources;
        public ResourceValue[] regenResources;
        public StatusEffect(float duration, params AttributeValue[] attributes)
        {
            this.duration = duration;
            this.attributes = attributes;
            maxResources = regenResources = new ResourceValue[0];
        }
        public StatusEffect(float duration, bool isMax, ResourceValue[] resources)
        {
            this.duration = duration;
            attributes = new AttributeValue[0];
            if (isMax)
            {
                maxResources = resources;
                regenResources = new ResourceValue[0];
            }
            else
            {
                maxResources = new ResourceValue[0];
                regenResources = resources;
            }
        }
    }    
    [System.Serializable] public struct AttributeValue
    {
        public Attribute attribute;
        public int value;
        public AttributeValue(Attribute attribute, int value)
        {
            this.attribute = attribute;
            this.value = value;
        }
    }
    [System.Serializable] public struct ResourceValue
    {
        public Resource resource;
        public int value;
        public ResourceValue(Resource resource, int value)
        {
            this.resource = resource;
            this.value = value;
        }
    }
    [System.Serializable] public struct AttributeRecord
    {
        public int current;
        public int additional;
        public AttributeRecord(int current) { this.current = current; additional = 0; }
    }
    [System.Serializable] public struct ResourceRecord
    {
        public int current;
        public int max;
        public int additionalMax;
        public int regen;
        public int additionalRegen;
        public ResourceRecord(int max, int regen)
        {
            current = this.max = max;
            additionalMax = 0;
            this.regen = regen;
            additionalRegen = 0;
        }
    }
    private struct AppliedEffect
    {
        public float durationLeft;
        public StatusEffect statusEffect;
        public AppliedEffect(float durationLeft, StatusEffect statusEffect) { this.durationLeft = durationLeft; this.statusEffect = statusEffect; }
    }

    public enum Attribute { moveSpeed, rotateSpeed, MAX }
    public enum Resource { health, stamina, focus, MAX }

    //used to check if there are enough resources to expend, reduces object creation
    private static int[] workingResourceValues;

    private LifeToggler lifeToggler;
    private Movement movement;
    private Equipment equipment;
    private Fighter fighter;
    
    [Header("0:moveSpeed, 1:rotateSpeed")]
    [SerializeField] private AttributeRecord[] attributes = new AttributeRecord[(int)Attribute.MAX];
    [Header("0:health, 1:stamina, 2:focus")]
    [SerializeField] private ResourceRecord[] resources = new ResourceRecord[(int)Resource.MAX] { new ResourceRecord(100, 0), new ResourceRecord(100, 2), new ResourceRecord(100, 2) };
    private float[] regenDecimals = new float[(int)Resource.MAX];

    private List<AppliedEffect> appliedEffects;

    public void LandAttack(int damage, Vector3 contactPoint, out int ricochet)
    {
        int combinedReduction = 0;
        bool poiseAttack = false;
        ricochet = 0;
        foreach (var arms in equipment.equippedArms)
            if (arms != null)
            {
                arms.OnAttacked(damage, contactPoint, out int reduction, out int outStagger, out bool poise);
                combinedReduction += reduction;
                ricochet += outStagger;
                poiseAttack = poiseAttack || poise;
            }
        damage -= combinedReduction;
        //play soundfx/vfx for hit confirmation
        if (poiseAttack == false)
            fighter.DamagedStagger();
        IncreaseResource(Resource.health, -damage);
    }
    public int GetAttribute(Attribute attribute) => attributes[(int)attribute].current + attributes[(int)attribute].additional;
    public void GetAttributeDivided(Attribute attribute, out int current, out int additional)
    {
        current = attributes[(int)attribute].current;
        additional = attributes[(int)attribute].additional;
    }
    public void SetAttribute(Attribute attribute, int current)
    {
        attributes[(int)attribute].current = current;
        UpdateAttribute(attribute);
    }
    public int GetResource(Resource resource) => resources[(int)resource].current;
    public int GetResourceMax(Resource resource) => resources[(int)resource].max + resources[(int)resource].additionalMax;
    public int GetResourceRegen(Resource resource) => resources[(int)resource].regen + resources[(int)resource].additionalRegen;
    public void SetResource(Resource resource, int value)
    {
        resources[(int)resource].current = value;
        ClampResource(resource);
    }
    public void IncreaseResource(Resource resource, int increase)
    {
        resources[(int)resource].current += increase;
        ClampResource(resource);
    }
    public bool ExpendResources(ResourceValue[] resourceValues)
    {
        if (workingResourceValues == null)
            workingResourceValues = new int[resources.Length];
        else for (int i = 0; i < workingResourceValues.Length; i++)
            workingResourceValues[i] = 0;

        foreach (ResourceValue resourceValue in resourceValues)
        {
            int i = (int)resourceValue.resource;
            workingResourceValues[i] -= resourceValue.value;
            if (resources[i].current + workingResourceValues[i] < 0)
                return false;
        }

        for (int i = 0; i < workingResourceValues.Length; i++)
            IncreaseResource((Resource)i, workingResourceValues[i]);
        return true;
    }
    public bool ExpendResource(Resource resource, int cost)
    {
        int newCurrent = resources[(int)resource].current - cost;
        if (newCurrent < 0)
            return false;
        else
        {
            IncreaseResource(resource, -cost);
            return true;
        }
    }
    /// <summary>
    /// updates the max resources, and fills them up to max
    /// </summary>
    public void Recharge()
    {
        for (int i = 0; i < (int)Attribute.MAX; i++)
            UpdateAttribute((Attribute)i);
        for (int i = 0; i < (int)Resource.MAX; i++)
            SetResource((Resource)i, GetResourceMax((Resource)i));
    }
    public void AddStatusEffect(StatusEffect effect)
    {
        int findIndex = appliedEffects.FindIndex((applied) => applied.statusEffect == effect);
        if (findIndex == -1)
        {
            appliedEffects.Add(new AppliedEffect(effect.duration, effect));
            foreach(var attributeValue in effect.attributes)
                IncreaseAttribute(attributeValue.attribute, attributeValue.value);
            foreach (var maxResource in effect.maxResources)
                IncreaseResourceMax(maxResource.resource, maxResource.value);
            foreach (var regenResource in effect.regenResources)
                IncreaseResourceRegen(regenResource.resource, regenResource.value);
        }
        else
        {
            print("duped status effect "+effect.name);
            if (appliedEffects[findIndex].durationLeft == -1)
                return;
            else
            {
                AppliedEffect applied = appliedEffects[findIndex];
                applied.durationLeft = effect.duration;
                appliedEffects[findIndex] = applied;
            }
        }
    }
    public void RemoveStatusEffect(StatusEffect effect)
    {
        int findIndex = appliedEffects.FindIndex((applied) => applied.statusEffect == effect);
        if (findIndex == -1)
            print("effect to remove not found "+effect.name);
        else
            RemoveStatusIndex(findIndex);
    }
    private void Awake()
    {
        lifeToggler = GetComponent<LifeToggler>();
        movement = GetComponent<Movement>();
        equipment = GetComponent<Equipment>();
        fighter = GetComponent<Fighter>();

        if (attributes == null || attributes.Length != (int)Attribute.MAX)
            attributes = new AttributeRecord[(int)Attribute.MAX];
        if (resources == null || resources.Length != (int)Resource.MAX)
            resources = new ResourceRecord[(int)Resource.MAX];
        if (regenDecimals == null || regenDecimals.Length != (int)Resource.MAX)
            regenDecimals = new float[(int)Resource.MAX];

        appliedEffects = new List<AppliedEffect>();

        CheckAlive();
    }
    private void FixedUpdate()
    {
        //regeneration
        for (int i = 0; i < (int)Resource.MAX; i++)
        {
            //can be negative
            regenDecimals[i] += Time.fixedDeltaTime * GetResourceRegen((Resource)i);
            if(1 <= Mathf.Abs(regenDecimals[i]))
            {
                int increaseInteger;
                if(0 < regenDecimals[i])
                {
                    increaseInteger = Mathf.FloorToInt(regenDecimals[i]);
                    regenDecimals[i] -= increaseInteger;                            
                }
                else
                {
                    increaseInteger = Mathf.CeilToInt(regenDecimals[i]);
                    regenDecimals[i] -= increaseInteger;
                }
                IncreaseResource((Resource)i, increaseInteger);
            }
        }
        //check if status effects have ended
        for(int i = 0; i < appliedEffects.Count; i++)
        {
            AppliedEffect applied = appliedEffects[i];
            if (applied.durationLeft == -1)
                continue;

            float newDurationLeft = applied.durationLeft - Time.fixedDeltaTime;
            if (newDurationLeft <= 0)
            {
                RemoveStatusIndex(i);
                i--;
            }
            else
            {
                applied.durationLeft = newDurationLeft;
                appliedEffects[i] = applied;
            }
        }
        
    }
    private void IncreaseAttribute(Attribute attribute, int increase)
    {
        attributes[(int)attribute].additional += increase;
        UpdateAttribute(attribute);
    }
    private void IncreaseResourceMax(Resource resource, int increase)
    {
        resources[(int)resource].additionalMax += increase;
        ClampResource(resource);
    }
    private void IncreaseResourceRegen(Resource resource, int increase)
    {
        resources[(int)resource].additionalRegen += increase;
    }
    private void UpdateAttribute(Attribute attribute)
    {
        float calculateSpeed;
        switch (attribute)
        {
            case Attribute.moveSpeed:
                calculateSpeed = 3.5f * (100 + GetAttribute(Attribute.moveSpeed)) / 100f;
                movement.movementSpeed = 0 < calculateSpeed ? calculateSpeed : 0;
                break;
            case Attribute.rotateSpeed:
                calculateSpeed = 200 * (100 + GetAttribute(Attribute.rotateSpeed)) / 100f;
                movement.rotateSpeed = 0 < calculateSpeed ? calculateSpeed : 0;
                break;
        }
    }
    private void SetResourceMax(Resource resource, int value)
    {
        resources[(int)resource].max = value;
        ClampResource(resource);
    }
    private void ClampResource(Resource resource)
    {
        resources[(int)resource].current = Mathf.Clamp(resources[(int)resource].current, 0, GetResourceMax(resource));
        if (resource == Resource.health)
            CheckAlive();
    }
    private void CheckAlive()
    {
        lifeToggler.SetAlive(0 < GetResource(Resource.health));
    }    
    [ContextMenu("Recharge")] private void InspectorRecharge()
    {
        //initialise references
        Awake();
        Recharge();
    }
    private void RemoveStatusIndex(int index)
    {
        StatusEffect effect = appliedEffects[index].statusEffect;
        appliedEffects.RemoveAt(index);

        foreach (var attributeValue in effect.attributes)
            IncreaseAttribute(attributeValue.attribute, -attributeValue.value);
        foreach (var maxResource in effect.maxResources)
            IncreaseResourceMax(maxResource.resource, -maxResource.value);
        foreach (var regenResource in effect.regenResources)
            IncreaseResourceRegen(regenResource.resource, -regenResource.value);
    }
}
