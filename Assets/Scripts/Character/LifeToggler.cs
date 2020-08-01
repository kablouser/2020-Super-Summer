using UnityEngine;

public class LifeToggler : MonoBehaviour
{
    public const float deathDelay = 2.0f;

    public MonoBehaviour[] aliveBehaviours;
    public GameObject deathEffect;
    public bool destroyOnDeath;

    private bool started = false;
    private bool currentAlive;

    public void SetAlive(bool isAlive)
    {
        if (started == false)
            started = true;
        else if (currentAlive == isAlive)
            return;

        currentAlive = isAlive;

        foreach(MonoBehaviour behaviour in aliveBehaviours)
            behaviour.enabled = isAlive;
        deathEffect.SetActive(!isAlive);

        if(destroyOnDeath && isAlive == false)
            Destroy(gameObject, deathDelay);
    }
}
