using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CharacterTag : MonoBehaviour
{
    public CharacterSheet attachedCharacter;

    [ContextMenu("Copy to children colliders")]
    private void CopyToChildren()
    {
        RecursiveCopy(transform);
    }

    private void RecursiveCopy(Transform target)
    {
        Collider col = target.GetComponent<Collider>();
        if (col)
        {
            col.isTrigger = true;

            CharacterTag targetTag = target.GetComponent<CharacterTag>();
            if (targetTag == null)
                targetTag = target.gameObject.AddComponent<CharacterTag>();

            targetTag.attachedCharacter = attachedCharacter;
        }

        for (int i = 0; i < target.childCount; i++)
            RecursiveCopy(target.GetChild(i));
    }
}
