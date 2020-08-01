using UnityEngine;

public class CharacterComponents : MonoBehaviour
{
    public Rigidbody rigidbodyComponent;
    public Movement movement;
    public LifeToggler lifeToggler;
    public Equipment equipment;
    public CharacterSheet characterSheet;
    /**
     * <summary>Ability coroutines go here</summary>
     */
    public Fighter fighter;
    public Animator animator;
    public AnimationEventListener animationEventListener;
}
