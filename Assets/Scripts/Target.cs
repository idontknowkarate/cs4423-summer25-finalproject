using UnityEngine;

public class Target : MonoBehaviour, IExplode
{
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private float _size = 10;
    [SerializeField] private float _speed = 10;

    public Rigidbody Rb => _rb;

    public enum EnemyType
    {
        Small = 1,
        Medium = 2,
        Large = 3,
        Boss = 4
    }

    public EnemyType enemyType = EnemyType.Small;

    public float health = 100f;

    [Header("Explosion")]
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private AudioClip explosionSFX;
    [SerializeField][Range(0f, 1f)] private float explosionVolume = 0.1f;

    [Header("Lock-On Reticle")]
    public GameObject lockOnReticlePrefab;
    private GameObject activeReticle;

    private bool hasExploded = false;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void TakeDamage(float amount)
    {
        health -= amount;
        if (health <= 0f)
        {
            Explode();
        }
    }

    void Update()
    {
        var dir = new Vector3(Mathf.Cos(Time.time * _speed) * _size, Mathf.Sin(Time.time * _speed) * _size);
        _rb.linearVelocity = dir;
    }

    // called by PlayerShooting when locking on
    public void ShowLockOnReticle()
    {
        if (lockOnReticlePrefab != null && activeReticle == null)
        {
            activeReticle = Instantiate(lockOnReticlePrefab, transform.position, Quaternion.identity, transform);
        }
    }

    // called by PlayerShooting when lock is lost
    public void HideLockOnReticle()
    {
        if (activeReticle != null)
        {
            Destroy(activeReticle);
            activeReticle = null;
        }
    }

    public void Explode()
    {
        if (hasExploded) return; // prevent multiple explosions
        hasExploded = true;

        ScoreManager.Instance.AddScore((int)enemyType);

        HideLockOnReticle();

        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        if (explosionSFX != null)
        {
            GameObject soundObj = new GameObject("ExplosionSFX");
            soundObj.transform.position = transform.position;

            AudioSource src = soundObj.AddComponent<AudioSource>();
            src.clip = explosionSFX;
            src.volume = explosionVolume;
            src.spatialBlend = 1.0f;
            src.minDistance = 5f;
            src.maxDistance = 50f;
            src.rolloffMode = AudioRolloffMode.Linear;

            src.Play();
            Destroy(soundObj, explosionSFX.length);
        }

        Destroy(gameObject);
    }
}
