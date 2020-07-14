#if UNITY_EDITOR

using UnityEditor;

[CustomPropertyDrawer(typeof(CharacterSheet.AttributeRecord))]
public class AttributeRecordDrawer : GridPropertyDrawer
{
    protected override DrawInstruction[][] GetDrawings => myDrawings;

    private static readonly DrawInstruction[][] myDrawings = new DrawInstruction[][]
    {
        new DrawInstruction[]
        {
            new DrawInstruction(DrawDisplay.Variable("current"), DrawSize.Shared),
            new DrawInstruction(DrawDisplay.Label("+", middleAlign), DrawSize.ConstantWidth(unitWidth)),
            new DrawInstruction(DrawDisplay.Variable("additional"), DrawSize.Shared)
        }
    };
}

#endif