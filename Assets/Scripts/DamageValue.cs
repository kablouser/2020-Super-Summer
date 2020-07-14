[System.Serializable]
public struct DamageValue
{
    public enum DamageType { physical, arcane }

    public int baseValue;
    public DamageType damageType;
}
