using UnityEngine;

public class Health : MonoBehaviour
{
    public float maxHealth = 100f;
    public System.Action HandleCastleDestroyed;
    private float currentHealth;
    private GameObject linkedHealthBar;
    void Start()
    {
        currentHealth = maxHealth;
    }
    public void SetLinkedHealthBar(GameObject bar)
    {
        linkedHealthBar = bar;
    }

    // Method to take damage
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        Debug.Log(gameObject.name + " took damage: " + damage + ". Current health: " + currentHealth);
        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    // Method to set health, useful when scaling health for stronger enemies
    public void SetHealth(float health)
    {
        currentHealth = health;
        maxHealth = health;
        //Debug.Log(gameObject.name + " health set to: " + currentHealth);
    }

    // Method to get current health
    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth; // For potential health bar display
    }


    // Method to destroy object when health reaches zero
    void Die()
    {
        // Destroy linked health bar if it exists
        if (linkedHealthBar != null)
        {
            Destroy(linkedHealthBar);
        }

        // If this is an enemy, notify the EnemyManager
        Enemy enemyComponent = GetComponent<Enemy>();
        if (enemyComponent != null)
        {
            EnemyManager enemyManager = FindObjectOfType<EnemyManager>();
            if (enemyManager != null)
            {
                enemyManager.HandleEnemyDeath(enemyComponent);
            }
        }
        Castle castleComponent = GetComponent<Castle>();
        if (castleComponent != null)
        {
            CastleManager castleManager = FindObjectOfType<CastleManager>();
            castleManager.HandleCastleDestroyed();
        }

        Destroy(gameObject);
    }

}
