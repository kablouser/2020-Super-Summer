using UnityEngine;
using UnityEngine.UI;

using static CharacterSheet;

public class ResourceBars : MonoBehaviour
{
    private static Transform mainCamera;

    public Slider healthBar;
    public Slider staminaBar;
    public Slider focusBar;
    public bool faceCamera;
    public bool autoHide;

    private CharacterSheet characterSheet;

    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main.transform;
    }

    public void Setup(CharacterSheet characterSheet) =>
        this.characterSheet = characterSheet;

    public void UpdateVisuals()
    {
        UpdateBar(Resource.health, healthBar);
        UpdateBar(Resource.stamina, staminaBar);
        UpdateBar(Resource.focus, focusBar);
    }

    private void FixedUpdate()
    {
        if(faceCamera && mainCamera != null)
            transform.LookAt(2 * transform.position - mainCamera.position);
    }

    private void UpdateBar(Resource resourceType, Slider bar)
    {
        int current = characterSheet.GetResource(resourceType);
        int max = characterSheet.GetResourceMax(resourceType);
        if (autoHide)
        {
            bool setActive = current != max;
            bar.gameObject.SetActive(setActive);
            if (setActive == false)
                return;
        }

        float barFill = current / (float)max;
        if (bar.value != barFill)
            bar.value = barFill;
    }
}
