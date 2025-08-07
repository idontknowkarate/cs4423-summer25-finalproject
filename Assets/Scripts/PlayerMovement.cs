using UnityEngine;
using DG.Tweening;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float followSmoothTime = 0.05f;
    public float rotationLerpSpeed = 5f;
    public float aimTargetForwardOffset = 5f;
    public Transform aimTarget;

    [Header("Reticle System")]
    public Transform reticle;

    [Header("Model Banking")]
    public float maxRollAngle = 45f; // roll when turning
    public float rollSpeed = 5f;

    [Header("Rolling")]
    private bool isRolling = false;
    public float barrelRollDuration = 0.4f;
    [SerializeField] private ParticleSystem barrelRollFX_Left;
    [SerializeField] private ParticleSystem barrelRollFX_Right;

    [Header("Speed Control")]
    public float normalSpeed = 10f;
    public float boostMultiplier = 2f;
    public float decelMultiplier = 0.5f;

    [Header("Camera")]
    public Camera mainCamera;
    public float normalFOV = 60f;
    public float boostFOV = 70f;
    public float decelFOV = 50f;
    public float fovTransitionSpeed = 5f;

    [Header("Visual FX")]
    public GameObject[] boostTrails;
    public GameObject[] engineGlows;
    public float glowScaleNormal = 1f;
    public float glowScaleBoost = 1.5f;
    public float glowScaleDecel = 0.5f;
    public float glowLerpSpeed = 5f;

    [Header("Boost Settings")]
    public float maxBoost = 3f;
    public float boostRechargeRate = 1f;
    public float boostDepleteRate = 1f;

    private bool isBoosting;
    private float currentBoost;
    private bool canBoost => currentBoost > 0f;
    private bool boostOnCooldown = false;
    public float boostCooldownDuration = 2f;
    private float boostCooldownTimer = 0f;

    [SerializeField] private ParticleSystem boostRing;
    [SerializeField] private ParticleSystem boostParticles;

    private Transform model;
    private Vector3 velocity = Vector3.zero;

    [SerializeField] private AudioSource engineHum;
    [SerializeField] private float idlePitch = 1f;
    [SerializeField] private float boostPitch = 1.4f;
    [SerializeField] private float brakePitch = 0.8f;
    [SerializeField] private float pitchChangeSpeed = 2f;

    private WorldScroller worldScroller;

    // bounds in viewport
    private float minViewportX = 0.1f;
    private float maxViewportX = 0.9f;
    private float minViewportY = 0.1f;
    private float maxViewportY = 0.9f;

    // margin before leveling
    private float levelingMargin = 0.05f;

    void Start()
    {
        model = transform.GetChild(0);
        if (mainCamera == null)
            mainCamera = Camera.main;

        currentBoost = maxBoost;

        worldScroller = FindObjectOfType<WorldScroller>();
    }

    void Update()
    {
        float currentSpeed = normalSpeed;
        float targetFOV = normalFOV;
        float targetGlowScale = glowScaleNormal;
        bool isBoosting = false;

        bool wantsToBoost = Input.GetKey(KeyCode.LeftShift) && !boostOnCooldown && currentBoost > 0f;

        if (wantsToBoost)
        {
            currentSpeed = normalSpeed * boostMultiplier;
            targetFOV = boostFOV;
            targetGlowScale = glowScaleBoost;
            isBoosting = true;

            currentBoost -= boostDepleteRate * Time.deltaTime;
            if (currentBoost <= 0f)
            {
                currentBoost = 0f;
                boostOnCooldown = true;
                boostCooldownTimer = boostCooldownDuration;
            }
        }
        else
        {
            if (boostOnCooldown)
            {
                boostCooldownTimer -= Time.deltaTime;
                if (boostCooldownTimer <= 0f)
                {
                    boostOnCooldown = false;
                }
            }
            else
            {
                currentBoost += boostRechargeRate * Time.deltaTime;
                currentBoost = Mathf.Min(currentBoost, maxBoost);
            }

            if (Input.GetKey(KeyCode.LeftControl))
            {
                currentSpeed = normalSpeed * decelMultiplier;
                targetFOV = decelFOV;
                targetGlowScale = glowScaleDecel;
            }
        }

        // FOV
        mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, targetFOV, Time.deltaTime * fovTransitionSpeed);

        if (worldScroller != null)
        {
            if (Input.GetKey(KeyCode.LeftShift) && canBoost)
                worldScroller.speedMultiplier = boostMultiplier;
            else if (Input.GetKey(KeyCode.LeftControl))
                worldScroller.speedMultiplier = decelMultiplier;
            else
                worldScroller.speedMultiplier = 1f;
        }

        // mouse-follow logic
        FollowMouse();

        // trail toggle
        foreach (var trail in boostTrails)
        {
            if (trail != null)
                trail.SetActive(isBoosting);
        }

        // engine glow scaling (no color change)
        foreach (var glow in engineGlows)
        {
            if (glow != null)
            {
                Vector3 desiredScale = Vector3.one * targetGlowScale;
                glow.transform.localScale = Vector3.Lerp(glow.transform.localScale, desiredScale, Time.deltaTime * glowLerpSpeed);
            }
        }

        if (Input.GetKeyDown(KeyCode.Q)) QuickSpin(-1);
        if (Input.GetKeyDown(KeyCode.E)) QuickSpin(1);

        UpdateEngineSound();
        UpdateBoostFX();
    }

    void UpdateEngineSound()
    {
        float targetPitch = idlePitch;

        if (Input.GetKey(KeyCode.LeftShift) && canBoost)      // boost
            targetPitch = boostPitch;
        else if (Input.GetKey(KeyCode.LeftControl))     // brake
            targetPitch = brakePitch;

        engineHum.pitch = Mathf.Lerp(engineHum.pitch, targetPitch, Time.deltaTime * pitchChangeSpeed);
    }

    void UpdateBoostFX()
    {
        if (boostParticles == null) return;

        var emission = boostParticles.emission;
        float targetRate = (Input.GetKey(KeyCode.LeftShift) && canBoost) ? 50f : 0f;
        emission.rateOverTime = new ParticleSystem.MinMaxCurve(targetRate);

        // Boost ring trigger
        if (Input.GetKeyDown(KeyCode.LeftShift) && canBoost && !boostOnCooldown)
        {
            boostRing.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); // reset
            boostRing.Play();
        }
    }

    void FollowMouse()
    {
        Vector3 mousePos = Input.mousePosition;

        float minX = Screen.width * minViewportX;
        float maxX = Screen.width * maxViewportX;
        float minY = Screen.height * minViewportY;
        float maxY = Screen.height * maxViewportY;

        mousePos.x = Mathf.Clamp(mousePos.x, minX, maxX);
        mousePos.y = Mathf.Clamp(mousePos.y, minY, maxY);

        Vector3 reticleWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(
            mousePos.x, mousePos.y,
            Camera.main.transform.position.z * -1f + aimTargetForwardOffset
        ));
        aimTarget.position = reticleWorldPos;

        Vector3 clampedPlanePos = Camera.main.ScreenToWorldPoint(new Vector3(
            mousePos.x, mousePos.y,
            Mathf.Abs(Camera.main.transform.position.z - transform.position.z)
        ));

        float maxFollowDistance = 5f;
        float followSpeed = 3f;

        Vector3 targetOffset = clampedPlanePos - transform.position;
        if (targetOffset.magnitude > maxFollowDistance)
            targetOffset = targetOffset.normalized * maxFollowDistance;

        transform.position = Vector3.Lerp(transform.position, transform.position + targetOffset, Time.deltaTime * followSpeed);

        Vector3 playerViewport = Camera.main.WorldToViewportPoint(transform.position);
        bool nearBottomEdge = playerViewport.y <= minViewportY + levelingMargin;

        Vector3 dirToTarget = aimTarget.position - model.position;
        if (dirToTarget.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(dirToTarget.normalized, Vector3.up);

            Vector3 aimViewport = Camera.main.WorldToViewportPoint(aimTarget.position);
            float offsetFromCenterX = (aimViewport.x - 0.5f) * 2f;
            float desiredRoll = -offsetFromCenterX * maxRollAngle;
            Quaternion rollRotation = Quaternion.AngleAxis(desiredRoll, Vector3.forward);

            if (nearBottomEdge)
            {
                Vector3 yawOnlyDirection = new Vector3(dirToTarget.x, 0f, dirToTarget.z);
                if (yawOnlyDirection.sqrMagnitude > 0.001f)
                {
                    Quaternion yawOnlyRotation = Quaternion.LookRotation(yawOnlyDirection.normalized, Vector3.up);
                    model.rotation = Quaternion.Slerp(model.rotation, yawOnlyRotation * rollRotation, Time.deltaTime * rotationLerpSpeed * 0.5f);
                }
            }
            else
            {
                model.rotation = Quaternion.Slerp(model.rotation, targetRotation * rollRotation, Time.deltaTime * rollSpeed);
            }
        }

        transform.rotation = Quaternion.identity;
    }

    public void QuickSpin(int dir)
    {
        if (isRolling) return;

        if (dir < 0 && barrelRollFX_Left != null)
            barrelRollFX_Left.Play();
        else if (dir > 0 && barrelRollFX_Right != null)
            barrelRollFX_Right.Play();

        isRolling = true;

        model.DOLocalRotate(
            new Vector3(0, 0, 360f * -dir),
            barrelRollDuration,
            RotateMode.LocalAxisAdd
        )
        .SetEase(Ease.OutSine)
        .OnComplete(() => isRolling = false);
    }

    void OnDrawGizmos()
    {
        if (Camera.main == null) return;

        Gizmos.color = Color.cyan;
        Vector3 bottomLeft = Camera.main.ViewportToWorldPoint(new Vector3(minViewportX, minViewportY, Mathf.Abs(Camera.main.transform.position.z)));
        Vector3 topRight = Camera.main.ViewportToWorldPoint(new Vector3(maxViewportX, maxViewportY, Mathf.Abs(Camera.main.transform.position.z)));
        Vector3 center = (bottomLeft + topRight) / 2f;
        Vector3 size = topRight - bottomLeft;
        Gizmos.DrawWireCube(center, size);

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(20f, 10f, 0.1f));
    }

    public float CurrentBoost => currentBoost;
    public float MaxBoost => maxBoost;

}