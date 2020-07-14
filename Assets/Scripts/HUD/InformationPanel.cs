using UnityEngine;
using TMPro;

public class InformationPanel : MonoBehaviour
{
    public TextMeshProUGUI itemName;
    public TextMeshProUGUI gramsDisplay;
    public TextMeshProUGUI volumeDisplay;
    public TextMeshProUGUI itemDescription;

    public void SetItem(Item item)
    {
        itemName.SetText(item.name);

        gramsDisplay.SetText(item.grams + "g");
        volumeDisplay.SetText(item.volume + "cm3");

        itemDescription.SetText(item.description);
    }
}
