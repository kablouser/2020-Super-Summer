using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Items/Inventory Only Item", order = 1)]
public class Item : ScriptableObject
{
    public int grams;
    [Tooltip("cm^3")]
    public int volume;
    public Sprite icon;
    [TextArea(1,5)]
    public string description;
}
