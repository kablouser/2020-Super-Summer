using UnityEngine;
using static CharacterSheet;

[System.Serializable]
public struct AttributeScaler
{
    [System.Serializable]
    public struct Scaling
    {
        public Attribute attribute;
        public float multiplier;
    }

    public int baseValue;
    public Scaling[] scalings;

    public int CalculateValue(CharacterSheet characterAttributes)
    {
        float total = baseValue;
        foreach (Scaling scaling in scalings)
            total += characterAttributes.GetAttribute(scaling.attribute) * scaling.multiplier;
        return Mathf.FloorToInt(total);
    }
}
