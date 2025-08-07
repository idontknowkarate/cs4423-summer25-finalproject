using UnityEngine;

public class ReticleBillboard : MonoBehaviour
{
    public float zOffset = -0.5f; // slightly toward camera

    void LateUpdate()
    {
        if (Camera.main == null) return;

        // always face camera
        transform.rotation = Camera.main.transform.rotation;

        // offset toward camera slightly
        Vector3 offset = new Vector3(0f, 0.5f, zOffset); // upward and forward offset
        transform.position = transform.parent.position + Camera.main.transform.TransformDirection(offset);
    }
}