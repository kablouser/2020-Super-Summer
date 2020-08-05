using UnityEngine;

public class CharacterComponents : MonoBehaviour
{
    public Vector3 CenterPosition => attackIndicator.transform.position;
    
    public Rigidbody rigidbodyComponent;
    public Movement movement;
    public LifeToggler lifeToggler;
    public Equipment equipment;
    public CharacterSheet characterSheet;
    public Fighter fighter;
    public Animator animator;
    public AnimationEventListener animationEventListener;
    public AttackIndictator attackIndicator;
}
