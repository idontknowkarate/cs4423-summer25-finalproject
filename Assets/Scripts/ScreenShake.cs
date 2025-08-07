using UnityEngine;

public class ScreenShake : MonoBehaviour
{
    public static ScreenShake Instance;

    public Transform shakeTarget; // set this to the actual camera transform
    public float shakeDuration = 0.3f;
    public float shakeIntensity = 0.5f;

    private Vector3 originalPosition;
    private float shakeTime = 0f;

    void Awake()
    {
        Instance = this;
        if (shakeTarget == null)
            shakeTarget = transform;

        originalPosition = shakeTarget.localPosition;
    }

    void Update()
    {
        if (shakeTime > 0)
        {
            shakeTarget.localPosition = originalPosition + Random.insideUnitSphere * shakeIntensity;
            shakeTime -= Time.deltaTime;
        }
        else
        {
            shakeTarget.localPosition = originalPosition;
        }
    }

    public void Shake(float duration = -1f, float intensity = -1f)
    {
        shakeTime = duration > 0 ? duration : shakeDuration;
        shakeIntensity = intensity > 0 ? intensity : shakeIntensity;
    }
}
