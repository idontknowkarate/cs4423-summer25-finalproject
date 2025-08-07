using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MissionTracker : MonoBehaviour
{
    public float missionDuration = 20f;
    private float elapsedTime = 0f;
    private bool missionEnded = false;

    private void Update()
    {
        if (missionEnded) return;

        elapsedTime += Time.deltaTime;

        if (elapsedTime >= missionDuration)
        {
            missionEnded = true;

            GameOverManager go = FindObjectOfType<GameOverManager>();
            go.TriggerMissionComplete();
            go.ShowMedalForScore();
        }
    }
}