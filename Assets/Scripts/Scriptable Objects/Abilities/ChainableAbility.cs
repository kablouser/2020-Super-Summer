using UnityEngine;

public abstract class ChainableAbility : Ability
{
    private const float maxHoldDifference = 0.1f;

    protected bool ChainIsUsing => holdToChain != null && holdToChain.IsUsing;

    [Header("Hold input will chain into this ability")]
    [SerializeField] private Ability holdToChain = null;
    private float startUseTime;

    public override void Setup(CharacterComponents characterComponents, bool mirror)
    {
        base.Setup(characterComponents, mirror);
        holdToChain?.Setup(characterComponents, mirror);
    }

    public override bool CanUse(Fighter.InputPhase phase)
    {
        if (phase == Fighter.InputPhase.hold &&
            IsUsing &&
            holdToChain != null &&
            (Time.time - startUseTime - Fighter.holdDuration) < maxHoldDifference &&
            holdToChain.CanUse(Fighter.InputPhase.down))
            return true;
        else return false;
    }

    public override void ForceEndUse() =>
        holdToChain?.ForceEndUse();

    public override bool HasEnded() =>
        holdToChain == null || holdToChain.HasEnded();

    public override void TryEndUse(out bool isProblem)
    {
        if (holdToChain == null)
            isProblem = false;
        else
            holdToChain.TryEndUse(out isProblem);
    }

    public override void Use(Fighter.InputPhase phase)
    {
        if (phase == Fighter.InputPhase.down)
            startUseTime = Time.time;
        else if (phase == Fighter.InputPhase.hold)
            holdToChain?.Use(Fighter.InputPhase.down);
    }
}
