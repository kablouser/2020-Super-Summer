using UnityEngine;
using System.Collections.Generic;

public class Projectile : MonoBehaviour
{
    [Header("(Assigned before spawn)")]
    public GameObject travellingEffect;
    public GameObject impactEffect;
    public float impactDuration;

    [Space()]
    public float speed;
    public float gravityModifier;
    public float range;

    [Header("(Assigned after RangedAbility.Use)")]
    public int damage;
    public CharacterSheet shooter;

    private Vector3 velocity;
    private List<Collider> insideShooterColliders;
    private float startFrame;

    private void Start()
    {
        startFrame = Time.time;
        velocity = transform.forward * speed;
    }

    private void FixedUpdate()
    {
        velocity += Physics.gravity * gravityModifier * Time.fixedDeltaTime;
        transform.rotation.SetLookRotation(velocity);

        Vector3 travelThisFrame = velocity * Time.fixedDeltaTime;
        transform.Translate(travelThisFrame, Space.World);
        range -= travelThisFrame.magnitude;

        if (range <= 0)
        {
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        CharacterTag tag = other.GetComponent<CharacterTag>();

        if (tag)
        {
            //will not impact the shooter until it travels out of it
            if (tag.attachedCharacter == shooter)
            {
                if (Time.time - startFrame < Time.deltaTime)
                {
                    if (insideShooterColliders == null)
                        insideShooterColliders = new List<Collider>(1);
                    insideShooterColliders.Add(other);
                    return;
                }
                else if (insideShooterColliders != null && 0 < insideShooterColliders.Count)
                {
                    insideShooterColliders.Add(other);
                    return;
                }
            }

            //damage!
            tag.attachedCharacter.LandAttack(damage, other.ClosestPoint(transform.position), out _);            
        }

        enabled = false;
        travellingEffect.SetActive(false);
        impactEffect.SetActive(true);
        Destroy(gameObject, impactDuration);
    }

    private void OnTriggerExit(Collider other)
    {
        if (insideShooterColliders != null && insideShooterColliders.Remove(other) &&
            insideShooterColliders.Count == 0)
        {
            //we have travelled out of the shooter,
            //allow to hit the original shooter again
            shooter = null;
        }
    }
}
