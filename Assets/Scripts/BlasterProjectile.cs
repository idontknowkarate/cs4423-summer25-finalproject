using UnityEngine;

public class BlasterProjectile : MonoBehaviour
{
    [SerializeField] private float damage = 10f;
    public float speed = 50f;
    public float maxLifetime = 3f;
    public GameObject impactEffectPrefab;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("No Rigidbody found on projectile!");
            return;
        }

        rb.useGravity = false;
        rb.linearVelocity = transform.forward * speed;

        Invoke(nameof(Explode), maxLifetime);
    }

    void OnTriggerEnter(Collider other)
    {
        // don't collide with other projectiles
        if (other.gameObject.layer == LayerMask.NameToLayer("Projectile"))
            return;

        IExplode damageable = other.GetComponentInParent<IExplode>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage);
        }

        TriggerImpactFX();
        Destroy(gameObject);
    }

    void Explode()
    {
        TriggerImpactFX();
        Destroy(gameObject);
    }

    void TriggerImpactFX()
    {
        if (impactEffectPrefab != null)
        {
            GameObject impact = Instantiate(impactEffectPrefab, transform.position, Quaternion.identity);
            var ps = impact.GetComponent<ParticleSystem>();
            if (ps != null)
                ps.Play();
            Destroy(impact, 2f);
        }
    }
}
