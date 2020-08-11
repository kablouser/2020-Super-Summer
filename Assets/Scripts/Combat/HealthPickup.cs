using UnityEngine;
using System.Collections;

public class HealthPickup : MonoBehaviour
{
    //1 rotation every 360/180 seconds
    public const float rotationRate = 180f;
    //1 bounce every 1/0.5 seconds
    public const float bounceRate = 2 * Mathf.PI * 0.5f;
    public const float bounceHeight = 0.3f;
    //shrink in 0.6 seconds
    public const float shrinkRate = 1.7f;

    public bool playerOnly = true;
    public int healAmount;
    public GameObject modelParent;
    public ParticleSystem pickUpFX;

    private bool consumed = false;

    private void Update()
    {
        modelParent.transform.Rotate(0, Time.deltaTime * rotationRate, 0);
        modelParent.transform.localPosition = new Vector3(0, bounceHeight * Mathf.Sin(Time.time * bounceRate), 0);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (consumed) return;

        var rigidbody = other.attachedRigidbody;
        if (rigidbody == null) return;

        var player = rigidbody.GetComponent<PlayerComponents>();
        if (player == null) return;

        player.characterSheet.IncreaseResource(CharacterSheet.Resource.health, healAmount);
        consumed = true;
        pickUpFX.gameObject.SetActive(true);
        StartCoroutine(ShrinkModel());
        Destroy(gameObject, pickUpFX.main.duration);        
    }

    private IEnumerator ShrinkModel()
    {
        float setScale = 1.0f;
        do
        {
            yield return null;
            setScale -= Time.deltaTime * shrinkRate;
            transform.localScale = Vector3.one * setScale;
        }
        while (0 < setScale);
    }
}
