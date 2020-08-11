using UnityEngine;

public class PlayerHUDLinker : MonoBehaviour
{
    [System.Serializable]
    public struct HUDInterface
    {
        public InteractionPanel interactionPanel;
        public MenuToggle MenuPanel;
        public GameObject EscapePanel;        
        public ResourceBars resourceBars;
    }

    [ContextMenuItem("Link Player", "LinkPlayer")]
    public HUDInterface hudObjects;
    public InventoryPanel inventoryPanel;

    [ContextMenu("Link Player")]
    private void LinkPlayer()
    {
        PlayerComponents player = FindObjectOfType<PlayerComponents>();

        if(player == null)
        {
            Debug.LogWarning("Player could not be found, not linked", gameObject);
        }
        else
        {
            //link player to HUD
            player.playerControl.hudObjects = hudObjects;

            //link HUD to player
            EventSystemModifier.Current.playerInput = player.playerInput;
            inventoryPanel.playerEquipment = player.equipment;

            Debug.Log("Player linked", gameObject);
        }
    }
}
