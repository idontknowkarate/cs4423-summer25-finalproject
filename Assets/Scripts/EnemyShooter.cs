using UnityEngine;
using System.Collections;

public class EnemyShooter : MonoBehaviour
{
    public GameObject projectilePrefab;
    public Transform[] firePoints;
    public float projectileSpeed = 25f;

    [Header("Burst Settings")]
    public float burstInterval = 2f;
    public int shotsPerBurst = 3;
    public float timeBetweenShots = 0.2f;
    public float fireDelay = 0f;

    [Header("Muzzle Flash")]
    public ParticleSystem[] muzzleFlashes;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip shootingSFX;

    [Header("Range Check")]
    public float firingRange = 50f;

    private float burstTimer = 0f;
    private bool isBursting = false;
    private Transform player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        burstTimer = Random.Range(0f, burstInterval); // add randomness to start time
    }

    void Update()
    {
        if (!player) return;

        burstTimer += Time.deltaTime;
        if (burstTimer >= burstInterval && !isBursting)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (distanceToPlayer <= firingRange)
            {
                StartCoroutine(FireBurst());
            }
        }
    }

    IEnumerator FireBurst()
    {
        isBursting = true;

        for (int i = 0; i < shotsPerBurst; i++)
        {
            // re-check player every shot
            if (player == null)
            {
                isBursting = false;
                yield break;
            }

            FireAtPlayer();
            yield return new WaitForSeconds(timeBetweenShots);
        }

        burstTimer = 0f;
        isBursting = false;
    }

    void FireAtPlayer()
    {
        foreach (Transform firePoint in firePoints)
        {
            Vector3 dir = (player.position - firePoint.position).normalized;
            GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.LookRotation(dir));
            Rigidbody rb = proj.GetComponent<Rigidbody>();
            if (rb != null)
                rb.linearVelocity = dir * projectileSpeed;

            int i = System.Array.IndexOf(firePoints, firePoint);
            if (muzzleFlashes != null && i >= 0 && i < muzzleFlashes.Length && muzzleFlashes[i] != null)
            {
                muzzleFlashes[i].Play();
            }

            if (audioSource && shootingSFX)
                audioSource.PlayOneShot(shootingSFX, Random.Range(0.3f, 0.5f));

            audioSource.volume = Random.Range(0.3f, 0.5f);
            audioSource.pitch = Random.Range(0.95f, 1.05f);
        }        
    }
}
