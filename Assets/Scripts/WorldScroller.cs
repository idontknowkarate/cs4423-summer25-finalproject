using UnityEngine;

public class WorldScroller : MonoBehaviour
{
    public float scrollSpeed = 5f;
    [Range(0.1f, 3f)] public float speedMultiplier = 1f; // Default multiplier

    void Update()
    {
        transform.position += Vector3.back * scrollSpeed * speedMultiplier * Time.deltaTime;
    }
}
