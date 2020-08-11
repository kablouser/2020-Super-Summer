using UnityEngine;

public abstract class Effect : ScriptableObject
{
    [System.Serializable]
    public struct AppliedEffect
    {
        public Effect effect;
        public float durationLeft;

        public AppliedEffect(Effect effect, float durationLeft)
        {
            this.effect = effect;
            this.durationLeft = durationLeft;
        }
    }

    [Tooltip("Set to -1 for infinite duration")]
    public float duration;

    //add stuff like icons, descriptions, important-ness here

    public virtual AppliedEffect CreateAppliedEffect() => new AppliedEffect(this, duration);

    public virtual void StackAppliedEffect(ref AppliedEffect applied)
    {
        if (applied.durationLeft != -1)
        {
            if (duration == -1)
                applied.durationLeft = -1;
            else if(applied.durationLeft < duration)
                applied.durationLeft = duration;
        }
    }

    public abstract void Apply(CharacterSheet target);
    public abstract void Remove(CharacterSheet target);

    public virtual T IsA<T>() where T : Effect => this as T;
}
