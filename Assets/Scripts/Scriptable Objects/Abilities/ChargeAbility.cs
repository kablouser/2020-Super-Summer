using System.Collections;
using UnityEngine;

public class ChargeAbility : Ability, CharacterSheet.IAttackListener
{
    public DamageValue damage;
    public int heft;
    public CharacterSheet.ResourceGroup cost;
    public Effect useEffect;

    public AnimationConstants.AbilityTrigger enableTrigger;
    public AnimationConstants.AbilityTrigger disableTrigger;

    [Tooltip("The velocity applied to enemies hit")]
    public float impactVelocity;
    public Timestamps timestamps;
    
    public BlockAbility.BlockConfig blockConfig;

    private Coroutine chargeRoutine;

    public override bool CanUse(Fighter.InputPhase phase)
    {
        if (phase == Fighter.InputPhase.down &&
            HasEnded() &&
            characterComponents.characterSheet.HasResources(cost))
            return true;

        else return false;
    }

    public override void ForceEndUse()
    {
        if (IsUsing)
            EndUse(true);
    }

    private void EndUse(bool interrupt)
    {
        IsUsing = false;

        if (chargeRoutine != null)
            StopCoroutine(chargeRoutine);

        characterComponents.animator.ResetTrigger(AnimationConstants.EnumToID(enableTrigger));
        characterComponents.animator.SetTrigger(AnimationConstants.EnumToID(disableTrigger));

        characterComponents.characterSheet.RemoveEffect(useEffect);
        characterComponents.characterSheet.RemoveAttackListener(this);
        if(interrupt)
            characterComponents.attackIndicator.FadeOutBlockIndicator();
        else
            characterComponents.attackIndicator.DisableBlockIndicator();
        //unlock movement
        characterComponents.movement.Unlock();
        characterComponents.movement.OnCollisionEvent -= OnChargeCollision;
    }

    public override void TryEndUse(out bool isProblem)
    {
        isProblem = HasEnded() == false;
    }

    public override void Use(Fighter.InputPhase phase)
    {
        if(phase == Fighter.InputPhase.down)
        {
            characterComponents.characterSheet.IncreaseResources(-cost);
            IsUsing = true;

            characterComponents.animator.SetTrigger(AnimationConstants.EnumToID(enableTrigger));
            characterComponents.animator.ResetTrigger(AnimationConstants.EnumToID(disableTrigger));

            if (chargeRoutine != null)
                StopCoroutine(chargeRoutine);
            chargeRoutine = StartCoroutine(ChargeRoutine());
        }
    }

    public void OnAttacked(int damage, Vector3 contactPoint, out CharacterSheet.DefenceFeedback feedback)
    {
        BlockAbility.BlockAttack(characterComponents.movement, characterComponents.attackIndicator,
            blockConfig, damage, contactPoint, out feedback);
    }

    private IEnumerator ChargeRoutine()
    {
        yield return timestamps.GetNextWait(0);

        //start charging/moving
        characterComponents.characterSheet.AddEffect(useEffect, -1);
        characterComponents.characterSheet.AddAttackListener(this);
        characterComponents.attackIndicator.EnableBlockIndicator(blockConfig.blockAngle);
        //force move forward
        characterComponents.movement.SetLockedMove(0, 1);
        characterComponents.movement.OnCollisionEvent += OnChargeCollision;

        yield return timestamps.GetNextWait(1);

        EndUse(false);
    }

    private void OnChargeCollision(Collision collision)
    {
        Rigidbody hitRigidbody = collision.rigidbody;
        if (hitRigidbody == null) return;

        CharacterSheet hitCharacter = hitRigidbody.GetComponent<CharacterSheet>();
        if (hitCharacter != null)
        {
            Vector3 directionToTarget =
                hitRigidbody.position -
                characterComponents.rigidbodyComponent.position;
            directionToTarget.Normalize();

            //ensure the target is within block angle
            if (Vector3.Dot(directionToTarget,
                characterComponents.movement.bodyRotator.forward)
                < Mathf.Cos(blockConfig.blockAngle))
                return;

            //apply damage
            int calculateDamage = characterComponents.characterSheet.
                CalculateAttackDamage(damage);
            hitCharacter.LandAttack(
                calculateDamage,
                characterComponents.CenterPosition, heft, out int ricochet);

            if (heft < ricochet)
            {
                characterComponents.fighter.RicochetStagger();
                return;
            }

            //move opponent            
            //just to prevent weird interactions
            if (directionToTarget == Vector3.zero)
                directionToTarget = Vector3.forward;
            hitRigidbody.velocity = directionToTarget * impactVelocity;

            EndUse(false);
        }
    }
}
