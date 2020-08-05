using UnityEngine;

public abstract class Ability : MonoBehaviour
{
    public bool Mirror { get; private set; }
    public bool IsUsing { get; protected set; }

    [Header("Ability")]
    public EnemyControl.AbilityManual abilityManual;
    
    protected CharacterComponents characterComponents;

    public virtual void Setup(CharacterComponents characterComponents, bool mirror)
    {
        this.characterComponents = characterComponents;
        Mirror = mirror;
    }

    public abstract bool CanUse(Fighter.InputPhase phase);
    public abstract void Use(Fighter.InputPhase phase);
    public abstract void TryEndUse(out bool isProblem);
    public abstract void ForceEndUse();
    public virtual bool HasEnded() => IsUsing == false;
}