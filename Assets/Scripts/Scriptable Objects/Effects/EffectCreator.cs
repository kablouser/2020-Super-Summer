using UnityEngine;
using static AtomicEffect;

/// <summary>
/// This class handles the timing, activation and expiration.
/// Contains smaller atomic effects that controls the gameplay changes.
/// </summary>
[CreateAssetMenu(fileName = "Effect Creator", menuName = "Effects/Effect Creator", order = 1)]
public class EffectCreator : ScriptableObject
{
    public class AppliedEffect
    {
        public EffectCreator creator;
        [Tooltip("Set to -1 for infinite duration")]
        public float durationLeft;

        private readonly AppliedAtomicEffect[] appliedAtomicEffects;

        public AppliedEffect(EffectCreator creator, float durationLeft, AtomicEffect[] atomicEffects)
        {
            this.creator = creator;
            this.durationLeft = durationLeft;

            appliedAtomicEffects = new AppliedAtomicEffect[atomicEffects.Length];
            for (int i = 0; i < appliedAtomicEffects.Length; i++)
                appliedAtomicEffects[i] = atomicEffects[i].CreateEffect();
        }

        public virtual void Apply(CharacterSheet target)
        {
            for (int i = 0; i < appliedAtomicEffects.Length; i++)
                appliedAtomicEffects[i].Apply(target);
        }

        public virtual void Stack(float newDuration)
        {
            if (durationLeft != -1)
            {
                if (newDuration == -1)
                    durationLeft = -1;
                else
                    durationLeft = Mathf.Max(durationLeft, newDuration);
            }
        }

        public virtual void Remove()
        {
            for (int i = 0; i < appliedAtomicEffects.Length; i++)
                appliedAtomicEffects[i].Remove();
        }
    }

    public float defaultDuration;
    //add stuff like icons, descriptions, important-ness here

    public AtomicEffect[] atomicEffects;

    public virtual AppliedEffect CreateEffect(CharacterSheet target)
    {
        AppliedEffect newEffect = new AppliedEffect(this, defaultDuration, atomicEffects);
        newEffect.Apply(target);
        return newEffect;
    }

    public virtual AppliedEffect CreateEffect(CharacterSheet target, float overrideDuration)
    {
        AppliedEffect newEffect = new AppliedEffect(this, overrideDuration, atomicEffects);
        newEffect.Apply(target);
        return newEffect;
    }

    public T FindAtomic<T>()
    {
        foreach (AtomicEffect atomicEffect in atomicEffects)
            if (atomicEffect is T t)
                return t;
        return default;
    }

    public T FindAtomic<T>(System.Func<T, bool> match)
    {
        foreach (AtomicEffect atomicEffect in atomicEffects)
            if (atomicEffect is T t && match(t))
                return t;
        return default;
    }
}
