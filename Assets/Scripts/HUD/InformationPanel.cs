using UnityEngine;
using TMPro;

public class InformationPanel : MonoBehaviour
{
    public const string gramsUnit = "g", volumeUnit = "cm3", emptyPlaceholder = "-";

    public TextMeshProUGUI itemName;
    public TextMeshProUGUI gramsDisplay;
    public TextMeshProUGUI volumeDisplay;
    public TextMeshProUGUI itemDescription;

    public void SetItem(Item item)
    {
        itemName.SetText(item.name);

        gramsDisplay.SetText(item.grams + gramsUnit);
        volumeDisplay.SetText(item.volume + volumeUnit);

        itemDescription.SetText(item.description);
    }

    public void Clear()
    {
        itemName.SetText(string.Empty);

        gramsDisplay.SetText(emptyPlaceholder);
        volumeDisplay.SetText(emptyPlaceholder);

        itemDescription.SetText(string.Empty);
    }
}
