[System.Serializable]
public class SimpleArray
{
    public bool isVertical;
    public float width;
    public Armament.SlotArray[] array;

    // Creates a vertical looking array
    public SimpleArray()
    {
        isVertical = true;
    }

    // Creates a horizontal looking array
    public SimpleArray(float width)
    {
        isVertical = false;
        this.width = width;
    }
}