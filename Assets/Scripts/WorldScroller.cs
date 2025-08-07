using UnityEngine;

public class WorldScroller : MonoBehaviour
{
    public float scrollSpeed = 10f;

    void Update()
    {
        transform.position += Vector3.back * scrollSpeed * Time.deltaTime;
    }
}
