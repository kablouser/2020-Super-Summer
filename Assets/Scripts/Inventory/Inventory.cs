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
    
    public int GetTotalWeight { get { return totalWeight; } }
    public int GetTotalVolume { get { return totalVolume; } }

    public event System.Action<int, InventoryEntry> OnInventoryInsert;
    public event System.Action<int, InventoryEntry> OnInventoryUpdate;
    public event System.Action<int> OnInventoryDelete;
    public event System.Action<int, int> OnInventoryReorder;

    [SerializeField] public List<InventoryEntry> inventory;
    [Tooltip("Set to -1 for no limit")]
    public int maxVolume = -1;
    public DroppedItem droppedItemPrefab;

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

    public virtual bool AddItem(Item item, int count, out int addedIndex)
    {
        if (maxVolume != -1 && maxVolume < totalVolume + item.volume * count)
        {
            addedIndex = -1;
            return false;
        }
        else
        {
            totalWeight += item.grams * count;
            totalVolume += item.volume * count;
        }
        
        if(FindItem(item, out InventoryEntry result, out addedIndex))
        {
            result.count += count;
            inventory[addedIndex] = result;

            OnInventoryUpdate?.Invoke(addedIndex, inventory[addedIndex]);
        }
        else
        {
            addedIndex = inventory.Count;
            inventory.Add(new InventoryEntry() { item = item, count = count });
            
            OnInventoryInsert?.Invoke(addedIndex, inventory[addedIndex]);
        }

        return true;
    }

    public virtual bool RemoveItem(int index, int count, out int missingCount, bool dropToFloor = true)
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
                    OnInventoryDelete?.Invoke(index);
                }
                else
                {
                    inventory[index] = entry;
                    OnInventoryUpdate?.Invoke(index, entry);
                }

                missingCount = 0;
                totalWeight -= entry.item.grams * count;
                totalVolume -= entry.item.volume * count;

                if(dropToFloor)
                    SpawnDroppedItem(new InventoryEntry(count, entry.item));
                return true;
            }
        }
        else
        {
            missingCount = count;
            return false;
        }
    }

    public bool RemoveItem(Item item, int count, out int missingCount, bool dropToFloor = true)
    {
        if(FindItem(item, out _, out int index))
            return RemoveItem(index, count, out missingCount, dropToFloor);
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

            OnInventoryReorder?.Invoke(oldIndex, newIndex);
        }
    }

    protected virtual void Start()
    {
        RecalculateLoad();
    }

    protected virtual void SpawnDroppedItem(InventoryEntry entry)
    {
        Instantiate(droppedItemPrefab, transform.position, Quaternion.identity)
            .SetItem(entry);
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
