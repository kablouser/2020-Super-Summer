using UnityEngine;

public abstract class AtomicEffect : ScriptableObject
{
    public abstract class AppliedAtomicEffect
    {
        public abstract void Apply(CharacterSheet target);
        public abstract void Remove();
    }

    public abstract AppliedAtomicEffect CreateEffect();
}
