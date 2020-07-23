using UnityEngine;

public class EquipmentPanel : MonoBehaviour
{
    public InventoryPanel master;
    public SlotArray<Transform> equipmentSlots;

    private ItemDisplayer[] itemDisplayers;

    public void BeginDrag()
    {
        master.EndDrag(InventoryPanel.EndDragPosition.original);
    }

    public void ResetPosition(Armament.Slot slot)
    {
        itemDisplayers[(int)slot].transform.SetParent(equipmentSlots[(int)slot]);
        itemDisplayers[(int)slot].transform.localPosition = Vector3.zero;
    }

    private void Awake()
    {
        Equipment playerEquipment = master.playerEquipment;

        itemDisplayers = new ItemDisplayer[(int)Armament.Slot.MAX];
        playerEquipment.OnEquipmentUpdate += OnEquipmentUpdate;

        for (int i = 0; i < (int)Armament.Slot.MAX; i++)
        {
            ArmamentPrefab prefab = playerEquipment.equippedArms.array[i];
            if (prefab == null)
                continue;

            playerEquipment.FindItem(prefab.armsScriptable, out _, out int inventoryIndex);
            OnEquipmentUpdate((Armament.Slot)i, prefab);
        }
    }

    private void OnDestroy()
    {
        Equipment playerEquipment = master.playerEquipment;

        playerEquipment.OnEquipmentUpdate -= OnEquipmentUpdate;
    }

    private void Update()
    {
        //checks for correction

        var array = master.playerEquipment.equippedArms.array;
        for(int i = 0; i < array.Length; i++)
        {
            if(array[i] == null)
            {
                if (itemDisplayers[i] != null)
                    Debug.LogError("Item shown not equipped", itemDisplayers[i]);
            }
            else
            {
                if(itemDisplayers[i] == null)
                    Debug.LogError("Item equipped not shown", array[i]);
                else
                {
                    int supposedNumber = array[i].usedSlots.Length;
                    if (supposedNumber == 1)
                    {
                        if (itemDisplayers[i].countDisplay.gameObject.activeSelf)
                            Debug.LogError("Count display incorrect - should be disabled", itemDisplayers[i].countDisplay);
                    }
                    else if (itemDisplayers[i].countDisplay.gameObject.activeSelf == false ||
                            int.Parse(itemDisplayers[i].countDisplay.text) != supposedNumber)
                        Debug.LogError("Count display incorrect", itemDisplayers[i].countDisplay);

                    if (itemDisplayers[i].iconDisplay.sprite != array[i].armsScriptable.icon)
                        Debug.LogError("Icon display incorrect", itemDisplayers[i].iconDisplay);

                    if (itemDisplayers[i].iconDisplay.sprite != array[i].armsScriptable.icon)
                        Debug.LogError("Icon display incorrect", itemDisplayers[i].iconDisplay);

                    if (itemDisplayers[i].isInventory)
                        Debug.LogError("Displayer setting isInventory is True", itemDisplayers[i]);

                    if (itemDisplayers[i].itemIndex != i)
                        Debug.LogError("Displayer index incorrect", itemDisplayers[i]);

                    if (itemDisplayers[i] != master.GetDragged)
                    {
                        if (itemDisplayers[i].transform.parent != equipmentSlots[i])
                            Debug.LogError("Displayer incorrect parent", itemDisplayers[i]);

                        if (itemDisplayers[i].iconDisplay.raycastTarget == false || itemDisplayers[i].frameDisplay.raycastTarget == false)
                            Debug.LogError("Displayer setting raycastTarget is False", itemDisplayers[i]);
                    }
                }                
            }
        }
    }

    private void OnEquipmentUpdate(Armament.Slot slot, ArmamentPrefab prefab)
    {
        ItemDisplayer displayer = itemDisplayers[(int)slot];

        if (prefab == null)
        {            
            if (displayer == null)
                //no changes needed
                return;
            else
            {
                //displayer needs to be removed
                master.displayersPool.RemoveObject(displayer.gameObject);
                itemDisplayers[(int)slot] = null;
            }
        }
        else
        {
            ItemDisplayer itemDisplayer;
            Armament arms = prefab.armsScriptable;

            if (displayer == null)
            {
                //displayer needs to be created
                itemDisplayer = itemDisplayers[(int)slot] = master.displayersPool.GenerateObject(equipmentSlots[(int)slot], 0)
                    .GetComponent<ItemDisplayer>();
                itemDisplayer.transform.localPosition = Vector3.zero;

                itemDisplayer.master = master;
                itemDisplayer.isInventory = false;
                itemDisplayer.itemIndex = (int)slot;
                itemDisplayer.SetIcon(arms.icon);
            }
            else
                itemDisplayer = itemDisplayers[(int)slot];

            itemDisplayer.SetCount(prefab.usedSlots.Length);
        }
    }
}
