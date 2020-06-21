using UnityEngine;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    [System.Serializable]
    public class InventoryEntry
    {
        public int count;
        public Item item;
        [SerializeField] public List<ItemModifier> modifiers;
    }

    [Header("Inventory")]
    [SerializeField] public List<InventoryEntry> inventory;
    [Tooltip("Set to -1 for no limit")]
    public int maxVolume = -1;
    public int GetTotalWeight { get { return totalWeight; } }
    public int GetTotalVolume { get { return totalVolume; } }
    [SerializeField]
    private int totalWeight;
    [SerializeField]
    private int totalVolume;

    public InventoryEntry FindItem(Item item)
    {
        return inventory.Find((entry) => entry.item == item);
    }

    public virtual bool AddItem(Item item, int count)
    {
        if (maxVolume != -1 && maxVolume < totalVolume + item.volume * count)
            return false;
        else
        {
            totalWeight += item.grams * count;
            totalVolume += item.volume * count;
        }

        InventoryEntry entry = FindItem(item);
        if (entry == null)
            inventory.Add(new InventoryEntry() { item = item, count = count });
        else
            entry.count += count;

        return true;
    }

    public virtual bool RemoveItem(Item item, int count, out int missingCount)
    {
        InventoryEntry entry = FindItem(item);
        if (entry == null)
        {
            missingCount = count;
            return false;
        }
        else
        {
            if (entry.count < count)
            {
                missingCount = count - entry.count;
                return false;
            }
            else
            {
                entry.count -= count;

                if (entry.count <= 0)
                    inventory.Remove(entry);

                missingCount = 0;
                totalWeight -= item.grams * count;
                totalVolume -= item.volume * count;
                return true;
            }
        }
    }
    protected virtual void Start()
    {
        RecalculateLoad();
    }
    [ContextMenu("Update Load Values")]
    protected void RecalculateLoad()
    {
        totalWeight = totalVolume = 0;
        foreach (var entry in inventory)
        {
            totalWeight += entry.count * entry.item.grams;
            totalVolume += entry.count * entry.item.volume;
        }
    }
}
