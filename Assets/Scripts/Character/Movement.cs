using System.Collections.ObjectModel;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Movement : MonoBehaviour
{
    public const float lookUpperLimit = 70;
    public const float lookLowerLimit = -100;

    public const float moveAcceleration = 7;
    public const float animationMoveAcceleration = 4;
    public const float maxBackPedal = -0.7f;

    public const float overSpeedLimit = 3.15f;
    public const float moveZDeadzone = 0.05f;

    public const float impulseCutoff = 0.25f;
    public const float extraCutoff = 0.7f;

    public delegate void CollisionEvent(Collision collision);

    public event CollisionEvent OnCollisionEvent;
    public float GetCurrentAngleY { get => bodyRotator.eulerAngles.y; }
    public float GetSpineAngleX { get; private set; }

    [Header("References")]
    public CharacterComponents characterComponents;

    public Transform head;
    public Transform bodyRotator;
    public Collider movingCollider;
    public PhysicMaterial movingMaterial;
    public PhysicMaterial staticMaterial;

    [Header("Parameters")]
    public float movementSpeed = 4;
    public float rotateSpeed = 200;

    public float lookUpBias = 0.5f;

    public ReadOnlyCollection<Collider> GetTouchingColliders => touchingColliders.AsReadOnly();

    private Rigidbody rigidbodyComponent;
    private Animator animator;

    private Vector3 move;
    private Quaternion bodyTargetRotation;    

    private bool locked;
    private Vector3 lockedMove;

    private bool grounded;
    private float groundGradient;
    private Vector3 groundNormalFoward;

    private Vector3 animationMove;
    private Transform spineBone;

    [SerializeField]
    private List<Collider> touchingColliders;

    public void SetMove(float moveX, float moveZ)
    {
        move.x = moveX;
        move.z = Mathf.Clamp(moveZ, maxBackPedal, 1);
        if (1 < move.sqrMagnitude)
            move.Normalize();
    }

    public void SetLockedMove(float moveX, float moveZ)
    {
        locked = true;

        lockedMove.x = moveX;
        lockedMove.z = Mathf.Clamp(moveZ, maxBackPedal, 1);
        if (1 < lockedMove.sqrMagnitude)
            lockedMove.Normalize();
    }

    public void Unlock()
    {
        locked = false;
    }

    public void SetLook(ref float angleX, float angleY)
    {
        bodyTargetRotation = Quaternion.Euler(0, angleY, 0);

        angleX = Mathf.Clamp(angleX, lookLowerLimit, lookUpperLimit);
        GetSpineAngleX = angleX < 0 ? angleX * lookUpBias : angleX;

        if (head != null)
            head.eulerAngles = new Vector3(angleX, angleY, 0);
    }

    private void SetFriction(bool isEnabled)
    {
        movingCollider.material = isEnabled ? staticMaterial : movingMaterial;
    }

    private void Awake()
    {
        rigidbodyComponent = characterComponents.rigidbodyComponent;
        animator = characterComponents.animator;
        spineBone = animator.GetBoneTransform(HumanBodyBones.Spine);

        touchingColliders = new List<Collider>();
    }

    private void OnEnable()
    {
        bodyTargetRotation = bodyRotator.rotation;
    }

    private void OnDisable()
    {
        animationMove = Vector3.zero;
        UpdateAnimationMove(Vector3.zero, Vector3.zero);
        SetFriction(true);
    }

    private void FixedUpdate()
    {
        bodyRotator.rotation = Quaternion.RotateTowards(bodyRotator.rotation, bodyTargetRotation, rotateSpeed * Time.deltaTime);

        Vector3 currentVelocity = rigidbodyComponent.velocity;

        Vector3 moveInput = locked ? lockedMove : move;

        if (moveInput == Vector3.zero)
        {
            if (animationMove != Vector3.zero)
                UpdateAnimationMove(currentVelocity, moveInput);
            SetFriction(true);
        }
        else
        {
            UpdateAnimationMove(currentVelocity, moveInput);
            SetFriction(false);

            //rotate and scale by movementSpeed
            Vector3 targetDirection = bodyTargetRotation * moveInput;
            float speed = movementSpeed;

            if (grounded)
            {
                float upHillFactor = -Vector3.Dot(groundNormalFoward.normalized, targetDirection);
                if (0 < upHillFactor)
                {
                    float gradientFactor = Mathf.Sin(groundGradient);
                    upHillFactor *= gradientFactor * gradientFactor * gradientFactor;
                    speed *= (1 - upHillFactor);
                }
                //check for grounded in OnCollisionStay
                grounded = false;
            }

            //limit acceleration
            Vector3 impulse = targetDirection * speed;

            //this is only horizontal            
            impulse.y = 0;
            //cancel out previous velocities to create impulse
            impulse.x -= currentVelocity.x;
            impulse.z -= currentVelocity.z;

            //apply cutoff
            if (speed * speed < currentVelocity.sqrMagnitude)
                //overspeeding, apply extra cutoff
                impulse = impulse.normalized * impulseCutoff * extraCutoff;
            else if (impulseCutoff * impulseCutoff < impulse.sqrMagnitude)
                //just apply normal cutoff
                impulse = impulse.normalized * impulseCutoff;

            rigidbodyComponent.AddForce(impulse, ForceMode.Impulse);
        }
    }
    private void LateUpdate()
    {
        //rotate spine bone's position around ...
        spineBone.RotateAround(spineBone.position,
            //the x axis of bodyRotator
            bodyRotator.rotation * Vector3.right,
            GetSpineAngleX);
        //we don't rotate around the x axis of spineBone
    }

    private void OnCollisionEnter(Collision collision)
    {
        OnCollisionEvent?.Invoke(collision);

        var colider = collision.collider;
        if (touchingColliders.Contains(colider) == false)
            touchingColliders.Add(colider);
    }

    private void OnCollisionStay(Collision collisionInfo)
    {
        if (grounded)
            return;

        foreach (ContactPoint contact in collisionInfo.contacts)
            if (0 < contact.normal.y)
            {
                grounded = true;
                groundNormalFoward = contact.normal;
                groundNormalFoward.y = 0;
                groundGradient = Mathf.Deg2Rad * (90 - Vector3.Angle(contact.normal, groundNormalFoward));
                break;
            }
    }

    private void OnCollisionExit(Collision collision)
    {
        touchingColliders.Remove(collision.collider);
    }

    private void UpdateAnimationMove(Vector3 currentVelocity, Vector3 moveInput)
    {
        //lerp it to smooth out animation
        animationMove.x = Mathf.Lerp(animationMove.x, moveInput.x, animationMoveAcceleration * Time.fixedDeltaTime);
        animationMove.z = Mathf.Lerp(animationMove.z, moveInput.z, animationMoveAcceleration * Time.fixedDeltaTime);

        animator.SetFloat(AnimationConstants.MoveX, animationMove.x);

        if(-moveZDeadzone < animationMove.z && animationMove.z < 0)
            animator.SetFloat(AnimationConstants.MoveZ, 0);
        else
            animator.SetFloat(AnimationConstants.MoveZ, animationMove.z);

        float overSpeed = currentVelocity.magnitude - overSpeedLimit;
        animator.SetFloat(AnimationConstants.MoveMultiplier, 0 < overSpeed ? (1 + overSpeed/overSpeedLimit) : 1);
    }
}
