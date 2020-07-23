using UnityEngine;
using TMPro;

using static Inventory;

public class DroppedItem : MonoBehaviour
{
    public const float MaxPickupRange = 1;

    public Item GetItem { get => itemData.item; }

    public SpriteRenderer iconRenderer;
    public TextMeshPro countDisplay;

    public Transform displayRotator;
    private Transform cameraTransform;

    [SerializeField]    
    private InventoryEntry itemData;

    public void SetItem(InventoryEntry data)
    {
        itemData = data;
        UpdateVisuals();
    }

    public void Pickup(Inventory intoInventory)
    {
        if(intoInventory.AddItem(itemData.item, itemData.count, out _))
            Destroy(gameObject);
    }

    [ContextMenu("UpdateVisuals")]
    private void UpdateVisuals()
    {
        iconRenderer.sprite = itemData.item.icon;

        if (itemData.count == 1)
            countDisplay.gameObject.SetActive(false);
        else
        {
            countDisplay.gameObject.SetActive(true);
            countDisplay.SetText(itemData.count.ToString());
        }
    }

    private void Awake()
    {
        cameraTransform = Camera.main.transform;
    }

    private void FixedUpdate()
    {
        displayRotator.transform.forward = cameraTransform.forward;
    }
}
