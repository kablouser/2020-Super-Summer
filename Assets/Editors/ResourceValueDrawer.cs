using UnityEditor;

[CustomPropertyDrawer(typeof(CharacterSheet.ResourceValue))]
public class ResourceValueDrawer : GridPropertyDrawer
{
    protected override DrawInstruction[][] GetDrawings => myDrawings;

    private static readonly DrawInstruction[][] myDrawings = new DrawInstruction[][]
    {
        new DrawInstruction[]
        {
            new DrawInstruction(DrawDisplay.Variable("resource"),DrawSize.Shared),
            new DrawInstruction(DrawDisplay.Variable("value"),DrawSize.Shared)
        }
    };
}
