using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Items/Inventory Only Item", order = 1)]
public class Item : ScriptableObject
{
    public int grams;
    [Tooltip("cm^3")]
    public int volume;
    public string description;
    public Sprite icon;
}
