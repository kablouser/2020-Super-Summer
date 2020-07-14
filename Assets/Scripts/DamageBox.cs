using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class DamageBox : MonoBehaviour
{
    public Transform hiltPosition;
    public Fighter owner;    

    private int damage;
    private int heft;

    private List<CharacterSheet> targetsAttacked;
    private Collider hitbox;

    public void StartAttack(int damage, int heft)
    {
        this.damage = damage;
        this.heft = heft;

        hitbox.enabled = true;
        targetsAttacked.Clear();
    }

    public void StopAttack()
    {
        hitbox.enabled = false;
        targetsAttacked.Clear();
    }

    private void Awake()
    {
        hitbox = GetComponent<Collider>();
        hitbox.enabled = false;
        hitbox.isTrigger = true;
        targetsAttacked = new List<CharacterSheet>();
    }

    private void OnTriggerEnter(Collider other)
    {
        CharacterTag tag = other.GetComponent<CharacterTag>();
        
        if (tag && targetsAttacked.Contains(tag.attachedCharacter) == false)
        {
            targetsAttacked.Add(tag.attachedCharacter);

            tag.attachedCharacter.LandAttack(
                damage,
                other.ClosestPoint(hiltPosition.position),
                heft,
                out int ricochet);

            if (heft < ricochet)
                owner.RicochetStagger();
        }
    }
}
