using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;          // Player
    public Vector3 offset;            // Camera offset
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void LateUpdate()
    {
        if (target == null) return;

        Vector3 position = target.position + offset;
        position.z = -10f;

        transform.position = position;
    }
}
