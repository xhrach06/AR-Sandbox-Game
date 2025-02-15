using UnityEngine;

public class Health : MonoBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    // Method to take damage
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        //Debug.Log(gameObject.name + " took damage: " + damage + ". Current health: " + currentHealth);
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
        // Optionally add particle effect or sound before destruction
        Destroy(gameObject);
        //Debug.Log(gameObject.name + " has been destroyed!");
    }
}
