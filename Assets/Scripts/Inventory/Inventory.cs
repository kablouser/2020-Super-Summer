using UnityEngine;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    [System.Serializable]
    public struct InventoryEntry
    {
        public int count;
        public Item item;
        public List<ItemModifier> modifiers;

        public InventoryEntry(int count, Item item)
        {
            this.count = count;
            this.item = item;
            modifiers = null;
        }
    }

    public interface IUpdateInventory
    {
        //CRUD
        void InsertEntry(int index, InventoryEntry entry);
        void UpdateEntry(int index, InventoryEntry entry);
        void DeleteEntry(int index);
        void ReorderEntry(int oldIndex, int newIndex);
    }

    [SerializeField] public List<InventoryEntry> inventory;
    [Tooltip("Set to -1 for no limit")]
    public int maxVolume = -1;
    public int GetTotalWeight { get { return totalWeight; } }
    public int GetTotalVolume { get { return totalVolume; } }

    public List<IUpdateInventory> updateInterfaces;

    [SerializeField]
    private int totalWeight;
    [SerializeField]
    private int totalVolume;

    public bool FindItem(Item item, out InventoryEntry entry, out int index)
    {
        index = inventory.FindIndex((findEntry) => findEntry.item == item);
        if (index == -1)
        {
            entry = default;
            return false;
        }
        else
        {
            entry = inventory[index];            
            return true;
        }
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
        
        if(FindItem(item, out InventoryEntry result, out int index))
        {
            result.count += count;
            inventory[index] = result;

            foreach (var updateInterface in updateInterfaces)
                updateInterface.UpdateEntry(index, inventory[index]);
        }
        else
        {
            inventory.Add(new InventoryEntry() { item = item, count = count });

            foreach (var updateInterface in updateInterfaces)
                updateInterface.InsertEntry(inventory.Count - 1, inventory[inventory.Count - 1]);
        }

        return true;
    }

    public virtual bool RemoveItem(int index, int count, out int missingCount)
    {
        if (index < inventory.Count)
        {
            InventoryEntry entry = inventory[index];

            if (entry.count < count)
            {
                missingCount = count - entry.count;
                return false;
            }
            else
            {
                entry.count -= count;

                if (entry.count <= 0)
                {
                    inventory.RemoveAt(index);
                    foreach (var updateInterface in updateInterfaces)
                        updateInterface.DeleteEntry(index);
                }
                else
                {
                    inventory[index] = entry;
                    foreach (var updateInterface in updateInterfaces)
                        updateInterface.UpdateEntry(index, entry);
                }

                missingCount = 0;
                totalWeight -= entry.item.grams * count;
                totalVolume -= entry.item.volume * count;
                return true;
            }
        }
        else
        {
            missingCount = count;
            return false;
        }
    }

    public bool RemoveItem(Item item, int count, out int missingCount)
    {
        if(FindItem(item, out _, out int index))
            return RemoveItem(index, count, out missingCount);
        else
        {
            missingCount = count;
            return false;
        }
    }

    public virtual void Reorder(int oldIndex, int newIndex)
    {
        if (oldIndex < inventory.Count)
        {
            var copy = inventory[oldIndex];
            inventory.RemoveAt(oldIndex);
            inventory.Insert(newIndex, copy);

            foreach (var updateInterface in updateInterfaces)
                updateInterface.ReorderEntry(oldIndex, newIndex);
        }
    }

    protected virtual void Awake()
    {
        updateInterfaces = new List<IUpdateInventory>();
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
