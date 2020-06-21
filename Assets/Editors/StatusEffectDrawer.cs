using UnityEditor;

[CustomPropertyDrawer(typeof(CharacterSheet.StatusEffect))]
public class StatusEffectDrawer : GridPropertyDrawer
{
    protected override DrawInstruction[][] GetDrawings => myDrawings;

    private readonly static DrawInstruction[][] myDrawings = new DrawInstruction[][]
    {
        new DrawInstruction[]
        {
            new DrawInstruction(DrawDisplay.Variable("name"), DrawSize.Shared),
            new DrawInstruction(DrawDisplay.Label("duration", rightAlign), DrawSize.ConstantWidth(60)),
            new DrawInstruction(DrawDisplay.Variable("duration"), DrawSize.Shared)
        },
        new DrawInstruction[]
        {
            new DrawInstruction(DrawDisplay.Variable("attributes"), DrawSize.Shared),
            new DrawInstruction(DrawDisplay.Variable("maxResources"), DrawSize.Shared),
            new DrawInstruction(DrawDisplay.Variable("regenResources"), DrawSize.Shared)
        }
    };
}
