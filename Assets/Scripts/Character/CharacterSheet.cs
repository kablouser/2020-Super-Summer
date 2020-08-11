using UnityEngine;
using System.Collections.Generic;

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
    [System.Serializable] public struct AttributeGroup
    {
        public int moveSpeed, rotateSpeed;
        public AttributeGroup(int moveSpeed, int rotateSpeed)
        {
            this.moveSpeed = moveSpeed;
            this.rotateSpeed = rotateSpeed;
        }
        public static AttributeGroup operator -(AttributeGroup a) => new AttributeGroup(-a.moveSpeed, -a.rotateSpeed);
    }
    [System.Serializable] public struct ResourceGroup
    {
        public int health, stamina, focus;
        public ResourceGroup(int health, int stamina, int focus)
        {
            this.health = health;
            this.stamina = stamina;
            this.focus = focus;
        }
        public static ResourceGroup operator -(ResourceGroup a) => new ResourceGroup(-a.health, -a.stamina, -a.focus);
    }
    public struct DefenceFeedback
    {
        public bool landedOnWeapon;
        public int ricochet, reduction, poise;
        public static DefenceFeedback NoDefence = new DefenceFeedback(false, 0, 0, 0);

        public DefenceFeedback(bool landedOnWeapon, int ricochet, int reduction, int poise)
        {
            this.landedOnWeapon = landedOnWeapon;
            this.ricochet = ricochet;
            this.reduction = reduction;
            this.poise = poise;
        }

        public DefenceFeedback(int ricochet, int reduction, int poise)
        {
            landedOnWeapon = true;
            this.ricochet = ricochet;
            this.reduction = reduction;
            this.poise = poise;
        }
    }
    public interface IAttackListener
    {
        void OnAttacked(int damage, Vector3 contactPoint, CharacterComponents character, out DefenceFeedback feedback);
    }

    public enum Attribute { moveSpeed, rotateSpeed, MAX }
    public enum Resource { health, stamina, focus, MAX }

    public CharacterComponents characterComponents;

    [ContextMenuItem("Recharge", "InspectorRecharge")]
    [SerializeField] private AttributeRecord[] attributes = new AttributeRecord[(int)Attribute.MAX];
    [ContextMenuItem("Recharge", "InspectorRecharge")]
    [SerializeField] private ResourceRecord[] resources =
        new ResourceRecord[(int)Resource.MAX] {
            new ResourceRecord(100, 0),
            new ResourceRecord(100, 2),
            new ResourceRecord(100, 2) };

    private LifeToggler lifeToggler;
    private Movement movement;
    private Fighter fighter;
    
    private float[] regenDecimals = new float[(int)Resource.MAX];

    [SerializeField]
    private List<Effect.AppliedEffect> appliedEffects;
    private List<IAttackListener> attackListeners;

    public void LandAttack(int damage, Vector3 origin, CharacterComponents attacker, int heft, out int ricochet)
    {
        ricochet = 0;
        if (enabled == false) return;

        int reduction = 0;
        int poise = 0; //we can use a base poise attribute
        bool canRicochet = false;

        foreach (var listener in attackListeners)
        {
            listener.OnAttacked(damage, origin, attacker, out DefenceFeedback feedback);

            ricochet += feedback.ricochet;
            reduction += feedback.reduction;
            poise += feedback.poise;
            canRicochet = canRicochet || feedback.landedOnWeapon;
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

    public bool HasResources(ResourceGroup hasResources)
    {
        if(resources[(int)Resource.health].current < hasResources.health)
            return false;
        else if (resources[(int)Resource.stamina].current < hasResources.stamina)
            return false;
        else if (resources[(int)Resource.focus].current < hasResources.focus)
            return false;

        return true;
    }

    public void IncreaseAttributes(AttributeGroup group)
    {
        IncreaseAttribute(Attribute.moveSpeed, group.moveSpeed);
        IncreaseAttribute(Attribute.rotateSpeed, group.rotateSpeed);
    }

    public void IncreaseResources(ResourceGroup group)
    {
        IncreaseResource(Resource.health, group.health);
        IncreaseResource(Resource.stamina, group.stamina);
        IncreaseResource(Resource.focus, group.focus);
    }

    public void IncreaseResourceMaxs(ResourceGroup group)
    {
        IncreaseResourceMax(Resource.health, group.health);
        IncreaseResourceMax(Resource.stamina, group.stamina);
        IncreaseResourceMax(Resource.focus, group.focus);
    }

    public void IncreaseResourceRegens(ResourceGroup group)
    {
        IncreaseResourceRegen(Resource.health, group.health);
        IncreaseResourceRegen(Resource.stamina, group.stamina);
        IncreaseResourceRegen(Resource.focus, group.focus);
    }

    public void SetResourceMax(Resource resource, int value)
    {
        resources[(int)resource].max = value;
        ClampResource(resource);
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

    public void AddEffect(Effect effect)
    {
        int findIndex = appliedEffects.FindIndex((applied) => applied.effect == effect);
        if (findIndex == -1)
        {
            var applied = effect.CreateAppliedEffect();
            appliedEffects.Add(applied);
            applied.effect.Apply(this);
        }
        else
        {
            var applied = appliedEffects[findIndex];
            effect.StackAppliedEffect(ref applied);
            appliedEffects[findIndex] = applied;
        }
    }

    public void AddEffect(Effect effect, float overrideDuration)
    {
        int findIndex = appliedEffects.FindIndex((applied) => applied.effect == effect);
        if (findIndex == -1)
        {
            var applied = effect.CreateAppliedEffect();
            applied.durationLeft = overrideDuration;
            appliedEffects.Add(applied);
            applied.effect.Apply(this);
        }
        else
        {
            var applied = appliedEffects[findIndex];
            //overload into this function if you want to override the duration
            effect.StackAppliedEffect(ref applied);
            appliedEffects[findIndex] = applied;
        }
    }

    public void RemoveEffect(Effect effect)
    {
        int findIndex = appliedEffects.FindIndex((applied) => applied.effect == effect);
        if (findIndex != -1)
            RemoveEffect(findIndex);
    }

    public void AddAttackListener(IAttackListener listener)
    {
        if (attackListeners == null)
            Awake();

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

        if(appliedEffects == null)
            appliedEffects = new List<Effect.AppliedEffect>();
        if (attackListeners == null)
            attackListeners = new List<IAttackListener>();
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
            var applied = appliedEffects[i];
            if (applied.durationLeft == -1)
                continue;

            float newDurationLeft = applied.durationLeft - Time.fixedDeltaTime;
            if (newDurationLeft <= 0)
            {
                RemoveEffect(i);
                i--;
            }
            else
            {
                applied.durationLeft = newDurationLeft;
                appliedEffects[i] = applied;
            }
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
        appliedEffects[index].effect.Remove(this);
        appliedEffects.RemoveAt(index);
        //perhaps the creator can reclaim/recycle the object
    }
}
