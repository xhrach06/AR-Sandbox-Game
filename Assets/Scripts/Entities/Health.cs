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

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void SetLinkedHealthBar(GameObject bar)
    {
        linkedHealthBar = bar;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage. Current health: {currentHealth}");

        if (currentHealth <= 0f)
            Die();
    }

    public void SetHealth(float value)
    {
        currentHealth = value;
        maxHealth = value;
    }

    public float GetCurrentHealth() => currentHealth;

    public float GetHealthPercentage() => currentHealth / maxHealth;

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
