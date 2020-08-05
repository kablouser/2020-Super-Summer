using UnityEngine;
using static CharacterSheet;

[CreateAssetMenu(fileName = "Status Effect", menuName = "Effects/Status Effect", order = 1)]
public class StatusEffect : Effect
{
    public AttributeGroup attributes;
    public ResourceGroup maxResources;
    public ResourceGroup regenResources;

    public override void Apply(CharacterSheet target)
    {
        target.IncreaseAttributes(attributes);
        target.IncreaseResourceMaxs(maxResources);
        target.IncreaseResourceRegens(regenResources);
    }

    public override void Remove(CharacterSheet target)
    {
        target.IncreaseAttributes(-attributes);
        target.IncreaseResourceMaxs(-maxResources);
        target.IncreaseResourceRegens(-regenResources);
    }

    public bool IsResourcesDrained(CharacterSheet characterSheet)
    {
        if (0 < regenResources.health && characterSheet.GetResource(Resource.health) <= 0)
            return true;
        else if (0 < regenResources.focus && characterSheet.GetResource(Resource.focus) <= 0)
            return true;
        else if (0 < regenResources.stamina && characterSheet.GetResource(Resource.stamina) <= 0)
            return true;
        else return false;
    }
}
