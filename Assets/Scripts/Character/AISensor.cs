using UnityEngine;
using System.Collections.Generic;

public class AISensor : MonoBehaviour
{
    [SerializeField]
    public List<EnemyControl> affectedEnemies;

    private void OnTriggerEnter(Collider other)
    {
        var rigidbody = other.attachedRigidbody;
        if (rigidbody == null) return;

        var character = rigidbody.GetComponent<CharacterComponents>();
        if (character == null) return;

        for (int i = 0; i < affectedEnemies.Count; i++)
            if(affectedEnemies[i] != null)
                affectedEnemies[i].SenseNewThreat(character);
    }
}
