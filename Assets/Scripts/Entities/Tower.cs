using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour
{
    public GameObject projectilePrefab; // Projectile prefab
    public float attackRange = 15f; // Tower's attack range
    public float attackCooldown = 2f; // Cooldown between attacks
    public float attackDamage = 20f; // Damage dealt by the tower
    public Transform firePoint; // Where the projectile is fired from

    private float lastAttackTime;
    private readonly List<Transform> enemiesInRange = new List<Transform>();
    private Health health;
    public GameObject healthBarPrefab;
    void Start()
    {
        health = GetComponent<Health>();
        if (health == null)
        {
            Debug.LogError("Health component is missing on Tower: " + gameObject.name);
            return;
        }

        GameObject bar = Instantiate(healthBarPrefab, GameObject.Find("HealthBarCanvas").transform);

        FollowWorldTarget follow = bar.GetComponent<FollowWorldTarget>();
        if (follow != null)
        {
            follow.SetTarget(transform);
        }

        HealthBar healthBar = bar.GetComponent<HealthBar>();
        if (healthBar != null)
        {
            healthBar.SetHealth(health);
        }
        Debug.Log("💡 Spawned health bar for " + gameObject.name);

    }



    void Update()
    {
        // Remove any destroyed enemies from the list
        enemiesInRange.RemoveAll(enemy => enemy == null);

        // Check cooldown and shoot if there are valid targets
        if (Time.time - lastAttackTime > attackCooldown && enemiesInRange.Count > 0)
        {
            Transform target = SelectTarget();
            Shoot(target);
            lastAttackTime = Time.time;
        }
    }

    void Shoot(Transform target)
    {
        if (projectilePrefab == null || firePoint == null)
        {
            Debug.LogError("Projectile prefab or firePoint is missing!");
            return;
        }

        // Instantiate and fire the projectile
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        Projectile projScript = projectile.GetComponent<Projectile>();
        if (projScript != null)
        {
            projScript.SetTarget(target, attackDamage);
        }
    }

    Transform SelectTarget()
    {
        // Optional: Prioritize the closest enemy
        Transform closestEnemy = null;
        float closestDistance = Mathf.Infinity;

        foreach (Transform enemy in enemiesInRange)
        {
            float distance = Vector3.Distance(transform.position, enemy.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemy;
            }
        }

        return closestEnemy != null ? closestEnemy : enemiesInRange[0];
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            enemiesInRange.Add(other.transform);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            enemiesInRange.Remove(other.transform);
        }
    }

    public void TakeDamage(float damage)
    {
        if (health != null)
        {
            health.TakeDamage(damage);
        }
        else
        {
            Debug.LogError("Health component is missing on Tower: " + gameObject.name);
        }
    }
}
