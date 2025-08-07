using UnityEngine;

public class MissionTracker : MonoBehaviour
{
    public WorldScroller worldScroller;      // Assign in Inspector
    public float distanceToComplete = 1000f; // How far the world should scroll
    private Vector3 startPos;
    private bool missionEnded = false;

    void Start()
    {
        if (worldScroller != null)
        {
            startPos = worldScroller.transform.position;
        }
    }

    void Update()
    {
        if (missionEnded || worldScroller == null) return;

        float scrolledDistance = startPos.z - worldScroller.transform.position.z;

        if (scrolledDistance >= distanceToComplete)
        {
            missionEnded = true;

            GameOverManager go = FindObjectOfType<GameOverManager>();
            if (go != null)
            {
                go.TriggerMissionComplete();
                go.ShowMedalForScore();
            }
        }
    }
}