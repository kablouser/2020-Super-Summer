using UnityEngine;
using System.Collections.Generic;

using static EffectCreator;

public class CharacterSheet : MonoBehaviour
{
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
    public interface IAttackListener
    {
        void OnAttacked(int damage, Vector3 contactPoint,
            out int ricochet, out int reduction, out int poise, out bool canRicochet);
    }

    public enum Attribute { moveSpeed, rotateSpeed, MAX }
    public enum Resource { health, stamina, focus, MAX }

    public CharacterComponents characterComponents;

    [SerializeField] private AttributeRecord[] attributes = new AttributeRecord[(int)Attribute.MAX];
    [SerializeField] private ResourceRecord[] resources = new ResourceRecord[(int)Resource.MAX] { new ResourceRecord(100, 0), new ResourceRecord(100, 2), new ResourceRecord(100, 2) };

    //used to check if there are enough resources to expend, reduces object creation
    private static int[] workingResourceValues;

    private LifeToggler lifeToggler;
    private Movement movement;
    private Fighter fighter;
    
    private float[] regenDecimals = new float[(int)Resource.MAX];

    private List<AppliedEffect> appliedEffects;
    private List<IAttackListener> attackListeners;

    public void LandAttack(int damage, Vector3 contactPoint, int heft, out int ricochet)
    {
        ricochet = 0;
        if (enabled == false) return;

        int reduction = 0;
        int poise = 0; //we can use a base poise attribute
        bool canRicochet = false;

        foreach (var listener in attackListeners)
        {
            listener.OnAttacked(damage, contactPoint,
                out int addRicochet, out int addReduction, out int addPoise, out bool canThisRicochet);

            ricochet += addRicochet;
            reduction += addReduction;
            poise += addPoise;
            canRicochet = canRicochet || canThisRicochet;
        }

        damage -= reduction;
        //play soundfx/vfx for hit confirmation
        if (poise < heft)
        {
            if (canRicochet)
                fighter.RicochetStagger();
            else if (0 < damage)
                fighter.DamagedStagger();
        }
        if (0 < damage)
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
    public void IncreaseAttribute(Attribute attribute, int increase)
    {
        attributes[(int)attribute].additional += increase;
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
    public void IncreaseResourceMax(Resource resource, int increase)
    {
        resources[(int)resource].additionalMax += increase;
        ClampResource(resource);
    }
    public void IncreaseResourceRegen(Resource resource, int increase)
    {
        resources[(int)resource].additionalRegen += increase;
    }
    public void IncreaseResource(Resource resource, int increase)
    {
        resources[(int)resource].current += increase;
        ClampResource(resource);
    }

    public bool HasResources(ResourceValue[] resourceValues)
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

        return true;
    }

    public void ExpendResources(ResourceValue[] resourceValues)
    {
        foreach (ResourceValue resourceValue in resourceValues)
            IncreaseResource(resourceValue.resource, -resourceValue.value);
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

    public void AddEffect(EffectCreator creator)
    {
        int findIndex = appliedEffects.FindIndex((applied) => applied.creator == creator);
        if (findIndex == -1)
            appliedEffects.Add(creator.CreateEffect(this));
        else
            appliedEffects[findIndex].Stack(creator.defaultDuration);
    }

    public void AddEffect(EffectCreator creator, float overrideDuration)
    {
        int findIndex = appliedEffects.FindIndex((applied) => applied.creator == creator);
        if (findIndex == -1)
            appliedEffects.Add(creator.CreateEffect(this, overrideDuration));
        else
            appliedEffects[findIndex].Stack(overrideDuration);
    }

    public void RemoveEffect(EffectCreator creator)
    {
        int findIndex = appliedEffects.FindIndex((applied) => applied.creator == creator);
        if (findIndex != -1)
            RemoveEffect(findIndex);
    }

    public void AddAttackListener(IAttackListener listener)
    {
        if (attackListeners.Contains(listener) == false)
            attackListeners.Add(listener);
        else
            Debug.LogError("listener is already listenning", listener as Object);
    }

    public void RemoveAttackListener(IAttackListener listener)
    {
        attackListeners.Remove(listener);
    }

    public int CalculateAttackDamage(DamageValue damageValue)
    {
        //add in damage buffs here
        return damageValue.baseValue;
    }

    private void Awake()
    {
        lifeToggler = characterComponents.lifeToggler;
        movement = characterComponents.movement;
        fighter = characterComponents.fighter;

        if (attributes == null || attributes.Length != (int)Attribute.MAX)
            attributes = new AttributeRecord[(int)Attribute.MAX];
        if (resources == null || resources.Length != (int)Resource.MAX)
            resources = new ResourceRecord[(int)Resource.MAX];
        if (regenDecimals == null || regenDecimals.Length != (int)Resource.MAX)
            regenDecimals = new float[(int)Resource.MAX];

        appliedEffects = new List<AppliedEffect>();
        attackListeners = new List<IAttackListener>();

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
        //check if effects have ended
        for(int i = 0; i < appliedEffects.Count; i++)
        {
            AppliedEffect applied = appliedEffects[i];
            if (applied.durationLeft == -1)
                continue;

            float newDurationLeft = applied.durationLeft - Time.fixedDeltaTime;
            if (newDurationLeft <= 0)
            {
                RemoveEffect(i);
                i--;
            }
            else
                applied.durationLeft = newDurationLeft;
        }
        
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

    private void RemoveEffect(int index)
    {
        appliedEffects[index].Remove();
        appliedEffects.RemoveAt(index);
        //perhaps the creator can reclaim/recycle the object
    }
}
