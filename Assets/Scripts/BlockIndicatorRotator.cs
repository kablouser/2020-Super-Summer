using UnityEngine;

public class BlockIndicatorRotator : MonoBehaviour
{
    public Movement movement;

    void Update()
    {
        transform.localEulerAngles = new Vector3(movement.GetSpineAngleX, 0, 0);
    }
}
