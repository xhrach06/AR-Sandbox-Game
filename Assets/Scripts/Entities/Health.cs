using UnityEngine;
/// <summary>
/// Handles health logic and notifies appropriate managers upon death.
/// </summary>
public class Health : MonoBehaviour
{
    public float maxHealth = 100f;
    public System.Action HandleCastleDestroyed;

    private float currentHealth;
    private GameObject linkedHealthBar;

    // Initializes health on start
    private void Start()
    {
        currentHealth = maxHealth;
    }

    // Links the health bar GameObject to this entity
    public void SetLinkedHealthBar(GameObject bar)
    {
        linkedHealthBar = bar;
    }

    // Reduces health by damage and triggers death if necessary
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0f)
            Die();
    }

    // Sets health and updates maxHealth
    public void SetHealth(float value)
    {
        currentHealth = value;
        maxHealth = value;
    }

    // Returns current health value
    public float GetCurrentHealth() => currentHealth;

    // Returns current health as a percentage
    public float GetHealthPercentage() => currentHealth / maxHealth;

    // Handles destruction logic when health reaches zero
    private void Die()
    {
        if (linkedHealthBar != null)
            Destroy(linkedHealthBar);

        Enemy enemyComponent = GetComponent<Enemy>();
        if (enemyComponent != null)
        {
            EnemyManager enemyManager = FindObjectOfType<EnemyManager>();
            enemyManager?.HandleEnemyDeath(enemyComponent);
        }

        Castle castleComponent = GetComponent<Castle>();
        if (castleComponent != null)
        {
            CastleManager castleManager = FindObjectOfType<CastleManager>();
            castleManager?.HandleCastleDestroyed();
        }

        Destroy(gameObject);
    }
}