using UnityEngine;

public class AnimationEventListener : MonoBehaviour
{
    public event System.Action OnFootstep;

    private void Footstep() =>
        OnFootstep?.Invoke();
}