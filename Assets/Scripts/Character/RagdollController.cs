using UnityEngine;

public class RagdollController : MonoBehaviour
{
    public void CopyPose(Animator copyOther)
    {
        transform.position = copyOther.transform.position;
        transform.rotation = copyOther.transform.rotation;

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
}
