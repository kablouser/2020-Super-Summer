using UnityEngine;

public class EnvironmentHazard : MonoBehaviour
{
    public bool onEnter;
    public DamageValue enterDamage;
    public int heft;
    public bool onStay;
    public Effect stayDebuff;

    private void OnCollisionEnter(Collision collision)
    {
        var rigidbody = collision.rigidbody;
        if (rigidbody == null) return;

        CharacterSheet entering = rigidbody.GetComponent<CharacterSheet>();
        if (entering != null)
        {
            if (onEnter)
                entering.LandAttack(enterDamage.baseValue, collision.GetContact(0).point,
                    null, heft, out _);

            if (onStay)
                entering.AddEffect(stayDebuff);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (onStay == false) return;

        var rigidbody = collision.rigidbody;
        if (rigidbody == null) return;

        CharacterSheet exiting = rigidbody.GetComponent<CharacterSheet>();
        if (exiting != null)
        {
            if (onStay)
                exiting.RemoveEffect(stayDebuff);
        }
    }
}
