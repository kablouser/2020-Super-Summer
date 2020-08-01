using UnityEngine;
using UnityEngine.UI;

using TMPro;

public class MenuToggle : MonoBehaviour
{
    [ContextMenuItem("Update", "UpdateState")]
    public bool isOn;

    public Button toggleButton;

    public Image tickGraphic;
    public Sprite onTick;
    public Sprite offTick;

    public TextMeshProUGUI label;
    public string onText;
    public string offText;

    public GameObject toggleGameobject;

    private void Awake()
    {
        toggleButton.onClick.AddListener(Toggle);
    }

    private void OnDestroy()
    {
        toggleButton.onClick.RemoveListener(Toggle);
    }

    private void Toggle()
    {
        isOn = !isOn;
        UpdateState();
    }

    private void UpdateState()
    {
        tickGraphic.sprite = isOn ? onTick : offTick;
        label.SetText(isOn ? onText : offText);
        toggleGameobject.SetActive(isOn);
    }
}
