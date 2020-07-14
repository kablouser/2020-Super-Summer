using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Movement : MonoBehaviour
{
    public const float lookUpperLimit = 40;
    public const float lookLowerLimit = -100;

    public const float moveAcceleration = 7;

    public CharacterComponents characterComponents;

    public Transform head;
    public Transform model;
    [Tooltip("used for calculating shield block angles")]
    public Transform modelCenter;
    public Collider movingCollider;
    public PhysicMaterial movingMaterial;
    public PhysicMaterial staticMaterial;
    public float movementSpeed = 4;
    public float rotateSpeed = 200;

    private Rigidbody rigidbodyComponent;

    private Quaternion headForward;
    private Quaternion modelTargetRotation;
    private Vector3 move;

    private bool grounded;
    private float groundGradient;
    private Vector3 groundNormalFoward;

    public void SetMove(float moveX, float moveZ)
    {
        move.x = moveX;
        move.z = moveZ;
        if (1 < move.sqrMagnitude)
            move.Normalize();
    }
    public void SetLook(ref float angleX, float angleY)
    {
        modelTargetRotation = Quaternion.Euler(0, angleY, 0);
        if (head != null)
        {
            angleX = Mathf.Clamp(angleX, lookLowerLimit, lookUpperLimit);
            head.eulerAngles = new Vector3(angleX, angleY, 0);
            headForward = Quaternion.Euler(0, angleY, 0);
        }
    }
    private void SetFriction(bool isEnabled)
    {
        movingCollider.material = isEnabled ? staticMaterial : movingMaterial;
    }
    private void Awake()
    {
        rigidbodyComponent = characterComponents.rigidbodyComponent;
    }
    private void OnDisable()
    {
        SetFriction(true);
    }
    private void Update()
    {
        model.rotation = Quaternion.RotateTowards(model.rotation, modelTargetRotation, rotateSpeed * Time.deltaTime);
    }
    private void FixedUpdate()
    {
        if (move == Vector3.zero)
        {
            SetFriction(true);
        }
        else
        {
            SetFriction(false);

            //rotate and scale by movementSpeed
            Vector3 targetDirection = headForward * move;
            Vector3 currentVelocity = rigidbodyComponent.velocity;
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
            Vector3 impulse = Vector3.Lerp(currentVelocity, targetDirection * speed, moveAcceleration * Time.fixedDeltaTime);

            //cancel out previous velocities
            impulse.x -= currentVelocity.x;
            impulse.y = 0; //this is only horizontal
            impulse.z -= currentVelocity.z;

            rigidbodyComponent.AddForce(impulse, ForceMode.Impulse);
        }
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
}
