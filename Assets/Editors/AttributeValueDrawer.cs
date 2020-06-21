using UnityEditor;

[CustomPropertyDrawer(typeof(CharacterSheet.AttributeValue))]
public class AttributeValueDrawer : GridPropertyDrawer
{
    protected override DrawInstruction[][] GetDrawings => myDrawings;

    private readonly static DrawInstruction[][] myDrawings = new DrawInstruction[][]
    {
        new DrawInstruction[]
        {
            new DrawInstruction(DrawDisplay.Variable("attribute"), DrawSize.Shared),
            new DrawInstruction(DrawDisplay.Variable("value"), DrawSize.Shared)
        }
    };
}
