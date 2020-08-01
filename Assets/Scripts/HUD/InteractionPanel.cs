using UnityEngine;
using TMPro;

public class InteractionPanel : MonoBehaviour
{
    public TextMeshProUGUI interactionLabel;
    public TextMeshProUGUI controlHint;

    public void SetText(string text)
    {
        interactionLabel.SetText(text);
        controlHint.transform.position =
            interactionLabel.transform.position +
            new Vector3(interactionLabel.GetPreferredValues().x / 2.0f + 5, 0, 0);

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
