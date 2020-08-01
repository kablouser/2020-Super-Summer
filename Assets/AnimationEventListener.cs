using UnityEngine;

public class AnimationEventListener : MonoBehaviour
{
    public event System.Action OnFootstep;
    public event System.Action<int> OnAbility;

    private void Footstep() =>
        OnFootstep?.Invoke();

    private void Ability(int stage) =>
        OnAbility?.Invoke(stage);
}
