using UnityEngine;
using UnityEngine.Events;

public class TriggerBox : MonoBehaviour
{
    public bool isPlayerOnly = true;
    public UnityEvent OnTriggerEnterEvent;

    private void OnTriggerEnter(Collider other)
    {
        var rigidbody = other.attachedRigidbody;
        if (rigidbody == null) return;

        if(isPlayerOnly)
        {
            var player = rigidbody.GetComponent<PlayerComponents>();
            if (player == null) return;
        }

        OnTriggerEnterEvent.Invoke();
    }
}
