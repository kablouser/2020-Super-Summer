using UnityEngine;

public class LifeToggler : MonoBehaviour
{
    public const float deathDelay = 2.0f;

    public CharacterComponents character;

    public MonoBehaviour[] aliveBehaviours;
    public RagdollController ragdollPrefab;    
    public bool isPlayer;

    private bool started = false;
    private bool currentAlive;

    public void SetAlive(bool isAlive)
    {
        if (started == false)
            started = true;
        else if (currentAlive == isAlive)
            return;

        currentAlive = isAlive;

        foreach (MonoBehaviour behaviour in aliveBehaviours)
            behaviour.enabled = isAlive;

        if (isAlive == false)
        {
            RagdollController newRagdoll = Instantiate(ragdollPrefab);
            newRagdoll.CopyPose(character.animator, character.equipment);

            if (isPlayer && character.movement.head != null)
                //keep the camera
                character.movement.head.SetParent(null);

            Destroy(gameObject);
        }
    }
}
