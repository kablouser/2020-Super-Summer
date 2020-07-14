using UnityEngine;
using static CharacterSheet;

[CreateAssetMenu(fileName = "Status Effect", menuName = "Effects/Status Effect", order = 2)]
public class StatusEffect : AtomicEffect
{    
    public class AppliedStatusEffect : AppliedAtomicEffect
    {
        public AttributeValue[] attributes;
        public ResourceValue[] maxResources;
        public ResourceValue[] regenResources;
        private CharacterSheet target;

        public AppliedStatusEffect(
            AttributeValue[] attributes,
            ResourceValue[] maxResources,
            ResourceValue[] regenResources)
        {
            this.attributes = attributes;
            this.maxResources = maxResources;
            this.regenResources = regenResources;
        }

        public override void Apply(CharacterSheet target)
        {
            this.target = target;

            foreach (var attributeValue in attributes)
                target.IncreaseAttribute(attributeValue.attribute, attributeValue.value);
            foreach (var maxResource in maxResources)
                target.IncreaseResourceMax(maxResource.resource, maxResource.value);
            foreach (var regenResource in regenResources)
                target.IncreaseResourceRegen(regenResource.resource, regenResource.value);
        }

        public override void Remove()
        {
            foreach (var attributeValue in attributes)
                target.IncreaseAttribute(attributeValue.attribute, -attributeValue.value);
            foreach (var maxResource in maxResources)
                target.IncreaseResourceMax(maxResource.resource, -maxResource.value);
            foreach (var regenResource in regenResources)
                target.IncreaseResourceRegen(regenResource.resource, -regenResource.value);
        }
    }

    [System.Flags]
    private enum MultiResource : short
    {
        none = 0,
        health = 1,
        stamina = 2,
        focus = 4
    }

    public AttributeValue[] attributes;
    public ResourceValue[] maxResources;
    public ResourceValue[] regenResources;

    public override AppliedAtomicEffect CreateEffect() =>
        new AppliedStatusEffect(attributes, maxResources, regenResources);

    public bool IsResourcesDrained(CharacterSheet characterSheet)
    {
        var negativeRegens = MultiResource.none;

        foreach (ResourceValue resourceValue in regenResources)
            if (resourceValue.value < 0)
            {
                if (resourceValue.resource == Resource.health)
                    negativeRegens |= MultiResource.health;
                else if (resourceValue.resource == Resource.stamina)
                    negativeRegens |= MultiResource.stamina;
                else
                    negativeRegens |= MultiResource.focus;
            }

        if (negativeRegens == MultiResource.none)
            return false;

        if ((negativeRegens & MultiResource.health) != MultiResource.none)
            if (characterSheet.GetResource(Resource.health) <= 0)
                return true;
        if ((negativeRegens & MultiResource.focus) != MultiResource.none)
            if (characterSheet.GetResource(Resource.focus) <= 0)
                return true;
        if ((negativeRegens & MultiResource.stamina) != MultiResource.none)
            if (characterSheet.GetResource(Resource.stamina) <= 0)
                return true;

        return false;
    }
}
