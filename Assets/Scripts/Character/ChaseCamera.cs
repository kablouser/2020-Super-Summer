using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseCamera : MonoBehaviour
{
    public float CurrentDistance { get; private set; }

    [ContextMenuItem("Set to current distance", "SetMaxDistance")]
    public float maxDistance;
    public LayerMask layerMask;
    public float lerpSpeed = 1f;
    public float wallOffset = 0.01f;

    private Transform parent;    
    private Vector3 direction;

    private void Awake()
    {
        parent = transform.parent;
        CurrentDistance = maxDistance;
        direction = transform.localPosition.normalized;
    }

    private void LateUpdate()
    {
        Vector3 parentPosition = parent.position;
        bool offsetFromWall = false;
        if (Physics.Raycast(parentPosition, parent.rotation * direction, out RaycastHit hitInfo, maxDistance, layerMask))
        {
            offsetFromWall = true;
            if (hitInfo.distance < CurrentDistance)
                CurrentDistance = hitInfo.distance;
            else
                CurrentDistance = Mathf.Lerp(CurrentDistance, hitInfo.distance, lerpSpeed * Time.deltaTime);
        }   
        else
        {
            CurrentDistance = Mathf.Lerp(CurrentDistance, maxDistance, lerpSpeed * Time.deltaTime);
        }

        transform.localPosition = direction * CurrentDistance;
        if(offsetFromWall)
            transform.Translate(hitInfo.normal * wallOffset, Space.World);
    }

    private void SetMaxDistance()
    {
        maxDistance = transform.localPosition.magnitude;
    }
}
