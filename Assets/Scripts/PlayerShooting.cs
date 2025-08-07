using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerShooting : MonoBehaviour
{
    [Header("Primary Fire")]
    public GameObject projectilePrefab;
    public Transform singleGun;
    public Transform aimTarget;
    public float fireRate = 0.25f;
    public ParticleSystem muzzleFlash;
    public AudioSource audioSource;
    public AudioClip blasterSFX;

    [Header("Missile Settings")]
    public GameObject missilePrefab;
    public Transform missileLauncher;
    public AudioClip missileFireSFX;
    public AudioClip lockOnGrowl;
    public AudioClip lockConfirmedSFX;
    public float lockOnRange = 50f;
    public float lockOnAngle = 30f;
    public float missileCooldown = 5f;
    public float missileHoldTime = 2f;

    [Header("Missile Ammo")]
    private float[] pylonCooldownTimers;
    private int pylonCount = 4;
    public int maxMissileReserve = 12;
    public int currentMissileReserve;
    public int maxLocksPerBurst = 4;

    [Header("Reticle")]
    [SerializeField] private MeshRenderer reticle;
    [SerializeField] private MeshRenderer outerReticle;
    [SerializeField, ColorUsage(false, true)] private Color missileLockColor = Color.red;
    private Color reticleDefaultColor;
    private Color outerReticleDefaultColor;

    [Header("Lock UI")]
    [SerializeField] private List<MeshRenderer> lockDots;
    [SerializeField, ColorUsage(false, true)] private Color lockDotActiveColor;

    private AudioSource growlSource;
    private AudioSource beepSource;
    private AudioSource missileSource;

    private float nextFireTime = 0f;
    private float nextMissileTime = 0f;
    private float fireButtonHeldTime = 0f;
    private bool holdingPrimary = false;
    private bool inMissileMode = false;

    private List<Target> lockedTargets = new List<Target>();

    void Start()
    {
        pylonCooldownTimers = new float[pylonCount];
        currentMissileReserve = maxMissileReserve;

        growlSource = gameObject.AddComponent<AudioSource>();
        growlSource.clip = lockOnGrowl;
        growlSource.loop = true;
        growlSource.playOnAwake = false;
        growlSource.volume = 0.8f;

        beepSource = gameObject.AddComponent<AudioSource>();
        beepSource.clip = lockConfirmedSFX;
        beepSource.loop = true;
        beepSource.playOnAwake = false;
        beepSource.volume = 1.0f;

        missileSource = gameObject.AddComponent<AudioSource>();
        missileSource.clip = missileFireSFX;
        missileSource.playOnAwake = false;
        missileSource.volume = 0.4f;

        if (reticle != null)
            reticleDefaultColor = reticle.material.GetColor("_EmissionColor");
        if (outerReticle != null)
            outerReticleDefaultColor = outerReticle.material.GetColor("_EmissionColor");
    }

    void Update()
    {
        HandleFireInput();
        bool isLocked = inMissileMode && lockedTargets.Count > 0;
        UpdateReticleColor(isLocked);
        UpdateLockDotVisibility(inMissileMode);
    }

    void HandleFireInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            holdingPrimary = true;
            fireButtonHeldTime = 0f;
            nextFireTime = 0f;
        }

        if (holdingPrimary)
        {
            fireButtonHeldTime += Time.deltaTime;

            if (!inMissileMode)
            {
                if (Time.time >= nextFireTime)
                {
                    Shoot();
                    nextFireTime = Time.time + fireRate;
                }

                if (fireButtonHeldTime >= missileHoldTime && ReadyMissiles > 0 && currentMissileReserve > 0)
                    EnterMissileMode();
            }
            else
            {
                LockOnMissiles();
            }
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            if (inMissileMode)
            {
                FireMissiles();
                ExitMissileMode();
            }

            holdingPrimary = false;
            fireButtonHeldTime = 0f;
        }
    }

    void EnterMissileMode()
    {
        inMissileMode = true;
    }

    void ExitMissileMode()
    {
        inMissileMode = false;
        if (growlSource.isPlaying) growlSource.Stop();
        if (beepSource.isPlaying) beepSource.Stop();

        foreach (Target t in lockedTargets)
        {
            if (t != null) t.HideLockOnReticle();
        }
        lockedTargets.Clear();
    }

    void LockOnMissiles()
    {
        if (currentMissileReserve <= 0)
        {
            foreach (Target t in lockedTargets)
            {
                t.HideLockOnReticle();
            }
            lockedTargets.Clear();
        }

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Vector3 aimDir = (aimTarget.position - transform.position).normalized;

        Target bestCandidate = null;
        float bestAngle = float.MaxValue;

        foreach (GameObject enemy in enemies)
        {
            Target t = enemy.GetComponent<Target>();
            if (lockedTargets.Contains(t)) continue;

            Vector3 dir = (enemy.transform.position - transform.position).normalized;
            float angle = Vector3.Angle(aimDir, dir);
            float dist = Vector3.Distance(transform.position, enemy.transform.position);

            if (angle < lockOnAngle && dist < lockOnRange && angle < bestAngle)
            {
                bestAngle = angle;
                bestCandidate = t;
            }
        }

        if (bestCandidate != null && lockedTargets.Count < Mathf.Min(maxLocksPerBurst, currentMissileReserve))
        {
            bestCandidate.HideLockOnReticle();
            bestCandidate.ShowLockOnReticle();
            lockedTargets.Add(bestCandidate);
        }

        for (int i = 0; i < lockDots.Count; i++)
        {
            if (lockDots[i] == null) continue;

            Color c = i < lockedTargets.Count ? lockDotActiveColor : Color.black;
            lockDots[i].material.SetColor("_EmissionColor", c * 2f); // HDR-compatible
        }

        if (lockedTargets.Count > 0)
        {
            if (growlSource.isPlaying) growlSource.Stop();
            if (!beepSource.isPlaying) beepSource.Play();
        }
        else
        {
            if (!growlSource.isPlaying) growlSource.Play();
            if (beepSource.isPlaying) beepSource.Stop();
        }
    }

    public int ReadyMissiles
    {
        get
        {
            int ready = 0;
            for (int i = 0; i < pylonCount; i++)
            {
                if (Time.time >= pylonCooldownTimers[i])
                    ready++;
            }
            return ready;
        }
    }

    public int ReserveMissiles => currentMissileReserve;

    void FireMissiles()
    {
        if (Time.time < nextMissileTime || lockedTargets.Count == 0 || currentMissileReserve <= 0)
            return;

        int missilesFired = 0;

        foreach (Target t in lockedTargets)
        {
            if (missilesFired >= currentMissileReserve) break;

            // Find next available pylon
            int pylonIndex = -1;
            for (int i = 0; i < pylonCount; i++)
            {
                if (Time.time >= pylonCooldownTimers[i])
                {
                    pylonIndex = i;
                    break;
                }
            }

            if (pylonIndex == -1)
                break; // no pylons ready

            GameObject missile = Instantiate(missilePrefab, missileLauncher.position, missileLauncher.rotation);
            HomingMissile homing = missile.GetComponent<HomingMissile>();
            if (homing != null)
            {
                homing.SetTarget(t);
            }

            pylonCooldownTimers[pylonIndex] = Time.time + missileCooldown;
            missilesFired++;
        }

        currentMissileReserve -= missilesFired;
        nextMissileTime = Time.time + 0.25f; // brief delay between salvos

        if (missileFireSFX != null)
            missileSource.PlayOneShot(missileFireSFX);
    }

    void Shoot()
    {
        int layerMask = LayerMask.GetMask("Default", "Enemy");
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        Vector3 direction;
        if (Physics.Raycast(ray, out hit, 1000f, layerMask))
        {
            direction = (hit.point - singleGun.position).normalized;
        }
        else
        {
            direction = ray.direction;
        }

        Instantiate(projectilePrefab, singleGun.position, Quaternion.LookRotation(direction));

        if (muzzleFlash) muzzleFlash.Play();
        if (audioSource && blasterSFX) audioSource.PlayOneShot(blasterSFX, 1f);

        audioSource.volume = Random.Range(0.3f, 0.5f);
        audioSource.pitch = Random.Range(0.95f, 1.05f);
    }

    void OnDisable()
    {
        if (growlSource && growlSource.isPlaying) growlSource.Stop();
        if (beepSource && beepSource.isPlaying) beepSource.Stop();

        foreach (Target t in lockedTargets)
        {
            if (t != null) t.HideLockOnReticle();
        }
        lockedTargets.Clear();
    }

    void UpdateReticleColor(bool locked)
    {
        Color targetColor = locked ? missileLockColor : reticleDefaultColor;
        Color targetOuter = locked ? missileLockColor : outerReticleDefaultColor;

        if (reticle != null)
            reticle.material.SetColor("_EmissionColor", targetColor * 2f);

        if (outerReticle != null)
            outerReticle.material.SetColor("_EmissionColor", targetOuter * 2f);
    }

    void UpdateLockDotVisibility(bool visible)
    {
        foreach (var dot in lockDots)
        {
            if (dot != null)
                dot.enabled = visible;
        }
    }

    void UpdateLockDots(int lockCount)
    {
        for (int i = 0; i < lockDots.Count; i++)
        {
            Color colorToSet = (i < lockCount) ? lockDotActiveColor * 2f : Color.black;
            lockDots[i].material.SetColor("_EmissionColor", colorToSet);
        }
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, transform.forward * lockOnRange);

        Vector3 aimDir = (aimTarget.position - transform.position).normalized;
        Quaternion leftRot = Quaternion.AngleAxis(-lockOnAngle * 0.5f, transform.up);
        Quaternion rightRot = Quaternion.AngleAxis(lockOnAngle * 0.5f, transform.up);

        Vector3 leftDirection = leftRot * aimDir;
        Vector3 rightDirection = rightRot * aimDir;

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, leftDirection * lockOnRange);
        Gizmos.DrawRay(transform.position, rightDirection * lockOnRange);
    }

    public float GetMissileCooldown(int index)
    {
        if (index < 0 || index >= pylonCooldownTimers.Length)
            return 0f;

        return Mathf.Max(0f, pylonCooldownTimers[index] - Time.time);
    }
    public int PylonCount => pylonCooldownTimers.Length;
    
}
