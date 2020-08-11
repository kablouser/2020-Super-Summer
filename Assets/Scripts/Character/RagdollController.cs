using UnityEngine;

public class RagdollController : MonoBehaviour
{
    public SkinnedMeshRenderer skin;
    [ContextMenuItem("Set total mass", "SetTotalMass")]
    [ContextMenuItem("Get total mass", "GetTotalMass")]
    public float totalMass;

    public void CopyPose(Animator copyOther, Equipment unpackEquipment)
    {
        //copy the other's material
        var otherSkin = copyOther.GetComponentInChildren<SkinnedMeshRenderer>();
        if (otherSkin != null)
            skin.materials = otherSkin.materials;
        else
            Debug.LogWarning("CopyPose couldn't find other's skin!", this);

        if(unpackEquipment != null)
            unpackEquipment.UnpackEquippedIntoWorld();

        transform.position = copyOther.transform.position;
        transform.rotation = copyOther.transform.rotation;
        transform.localScale = copyOther.transform.lossyScale;

        for (int i = 0; i < transform.childCount; i++)
            RecursiveCopy(transform.GetChild(i), copyOther.transform.GetChild(i));
    }

    private void RecursiveCopy(Transform myBone, Transform copyBone)
    {
        myBone.localPosition = copyBone.localPosition;
        myBone.localRotation = copyBone.localRotation;

        for(int i = 0; i < myBone.childCount; i++)
            RecursiveCopy(myBone.GetChild(i), copyBone.GetChild(i));
    }

    [ContextMenu("Set total mass")]
    private void SetTotalMass()
    {
        var allRigidbodies = GetComponentsInChildren<Rigidbody>();

        float currentTotalMass = 0f;
        for(int i = 0; i < allRigidbodies.Length; i++)
            currentTotalMass += allRigidbodies[i].mass;

        float multiplier = totalMass / currentTotalMass;
        for (int i = 0; i < allRigidbodies.Length; i++)
            allRigidbodies[i].mass *= multiplier;
    }

    [ContextMenu("Get total mass")]
    private void GetTotalMass()
    {
        var allRigidbodies = GetComponentsInChildren<Rigidbody>();

        float currentTotalMass = 0f;
        for (int i = 0; i < allRigidbodies.Length; i++)
            currentTotalMass += allRigidbodies[i].mass;

        Debug.Log("Ragdoll current total mass " + currentTotalMass);
    }
}
