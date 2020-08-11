using UnityEngine;
using UnityEngine.UI;

using TMPro;

public class MenuToggle : MonoBehaviour
{
    [ContextMenuItem("Update", "UpdateState")]
    [SerializeField]
    private bool isOn;

    public Button toggleButton;

    public Image tickGraphic;
    public Sprite onTick;
    public Sprite offTick;

    public TextMeshProUGUI label;
    public string onText;
    public string offText;

    public GameObject toggleGameobject;

    public void ShowMenuButtons(bool isOn)
    {
        this.isOn = isOn;
        UpdateState();
    }

    public void ToggleButton()
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
