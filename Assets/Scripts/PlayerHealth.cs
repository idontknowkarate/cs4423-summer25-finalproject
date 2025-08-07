using UnityEngine;

public class PlayerHealth : MonoBehaviour, IExplode
{
    public float maxHealth = 100f;
    private float currentHealth;

    public GameObject explosionPrefab;
    public AudioSource audioSource;
    public AudioClip explosionSFX;


    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        Debug.Log("Player took damage! Current HP: " + currentHealth);

        if (currentHealth <= 0f)
        {
            Explode();
        }
    }

    public void Explode()
    {
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        if (audioSource && explosionSFX)
            audioSource.PlayOneShot(explosionSFX, 0.1f);

        // update HUD before destroying the player
        PlayerUI ui = FindObjectOfType<PlayerUI>();
        if (ui) ui.ForceHealthBarEmpty();

        Object.FindFirstObjectByType<GameOverManager>().TriggerGameOver();

        Destroy(gameObject);
    }
    
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

}
