using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Item/Inventory Only Item", order = 1)]
public class Item : ScriptableObject
{
    [Header("Item")]
    public int grams;
    [Tooltip("cm^3")]
    public int volume;
    public string description;
}
