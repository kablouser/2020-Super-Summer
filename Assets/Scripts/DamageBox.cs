using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class DamageBox : MonoBehaviour
{
    public Transform hiltPosition;
    [Header("please initialise this in Start")]
    public Fighter owner;    

    private int damage;
    private int staggerThreshold;
    private string ricochetTrigger;
    private HashSet<CharacterSheet> targetsAttacked;
    private Collider hitbox;

    public void StartAttack(int damage, int staggerThreshold, string ricochetTrigger)
    {
        this.damage = damage;
        this.staggerThreshold = staggerThreshold;
        this.ricochetTrigger = ricochetTrigger;
        hitbox.enabled = true;
        targetsAttacked.Clear();
    }

    public void StopAttack()
    {
        hitbox.enabled = false;
    }

    private void Awake()
    {
        hitbox = GetComponent<Collider>();
        hitbox.enabled = false;
        hitbox.isTrigger = true;
        targetsAttacked = new HashSet<CharacterSheet>();
    }

    private void OnTriggerEnter(Collider other)
    {
        CharacterTag tag = other.GetComponent<CharacterTag>();
        
        if (tag && targetsAttacked.Contains(tag.attachedCharacter) == false)
        {
            tag.attachedCharacter.LandAttack(damage, other.ClosestPoint(hiltPosition.position), out int staggerAttacker);            
            targetsAttacked.Add(tag.attachedCharacter);
            if(staggerThreshold <= staggerAttacker)
            {
                StopAttack();
                owner.RicochetStagger(ricochetTrigger);
            }
        }
    }
}
