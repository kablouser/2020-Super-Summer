using UnityEngine;
using TMPro;

public class InteractionPanel : MonoBehaviour
{
    public TextMeshProUGUI interactionLabel;
    public TextMeshProUGUI controlHint;
    public void OnGUI()
    {
        var rect = interactionLabel.rectTransform.rect;
        var position = interactionLabel.rectTransform.position;
        var text = interactionLabel.textBounds;

        rect.x = position.x - text.extents.x;
        rect.y = Screen.height - position.y - text.extents.y;
        rect.width = text.extents.x * 2;
        rect.height = text.extents.y * 2;

        GUI.color = Color.magenta;
        GUI.Box(rect, GUIContent.none);
    }
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
