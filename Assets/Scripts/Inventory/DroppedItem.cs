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

    private bool consumed = false;

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
        if(cameraTransform != null)
            displayRotator.transform.forward = cameraTransform.forward;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (consumed) return;

        GameObject otherObject = collision.gameObject;
        DroppedItem otherItem = otherObject.GetComponent<DroppedItem>();
        if(otherItem != null && itemData.item == otherItem.itemData.item)
        {
            //choose lowest velocity
            Rigidbody rigidbody = GetComponent<Rigidbody>();
            Vector3 myVelocity = rigidbody.velocity;
            Vector3 otherVelocity = collision.rigidbody.velocity;
            if (otherVelocity.sqrMagnitude < myVelocity.sqrMagnitude)
                rigidbody.velocity = otherVelocity;

            //consume its count and destroy other            
            itemData.count += otherItem.itemData.count;
            otherItem.consumed = true;
            UpdateVisuals();
            Destroy(otherObject);
        }
    }
}
