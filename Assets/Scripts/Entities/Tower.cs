using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Tower : MonoBehaviour
{
    [Header("Attack Settings")]
    public GameObject projectilePrefab;
    public float attackRange = 15f;
    public float attackCooldown = 2f;
    public float attackDamage = 20f;
    public float hitChance = 0.85f;
    public Transform firePoint;

    [Header("Health")]
    public GameObject healthBarPrefab;

    private float lastAttackTime;
    private Health health;
    private readonly List<Transform> enemiesInRange = new();
    // Disables the tower in the CalibrationScene
    private void Awake()
    {
        if (SceneManager.GetActiveScene().name == "CalibrationScene")
        {
            enabled = false;
            return;
        }
    }
    // Initializes health and sets up health bar UI
    private void Start()
    {
        health = GetComponent<Health>();
        if (health == null) return;

        GameObject canvas = GameObject.Find("HealthBarCanvas");
        if (canvas == null) return;

        GameObject bar = Instantiate(healthBarPrefab, canvas.transform);

        FollowWorldTarget follow = bar.GetComponent<FollowWorldTarget>();
        follow?.SetTarget(transform);

        HealthBar healthBar = bar.GetComponent<HealthBar>();
        healthBar?.SetHealth(health);

        health.SetLinkedHealthBar(bar);
    }
    // Handles attack logic and cooldown timing
    private void Update()
    {
        enemiesInRange.RemoveAll(enemy => enemy == null);

        float shootDelay = Random.Range(attackCooldown * 0.5f, attackCooldown * 1.5f);
        if (Time.time - lastAttackTime > shootDelay && enemiesInRange.Count > 0)
        {
            Transform target = SelectTarget();
            Shoot(target);
            lastAttackTime = Time.time;
        }
    }
    // Instantiates and launches a projectile toward the target
    private void Shoot(Transform target)
    {
        if (projectilePrefab == null || firePoint == null) return;

        bool didHit = Random.value <= hitChance;
        Vector3 direction = (target.position - firePoint.position).normalized;

        if (!didHit)
        {
            direction += new Vector3(
                Random.Range(-0.3f, 0.3f),
                Random.Range(-0.3f, 0.3f),
                Random.Range(-0.3f, 0.3f)
            );
            direction.Normalize();
        }

        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.LookRotation(direction));
        Projectile projScript = projectile.GetComponent<Projectile>();

        if (projScript != null)
        {
            if (didHit)
                projScript.SetTarget(target, attackDamage);
            else
                projScript.SetMissDirection(direction);
        }
    }

    // Selects a random valid enemy from the list
    private Transform SelectTarget()
    {
        int index = Random.Range(0, enemiesInRange.Count);
        return enemiesInRange[index] != null ? enemiesInRange[index] : enemiesInRange[0];
    }

    // Adds an enemy to the attack range list
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
            enemiesInRange.Add(other.transform);
    }

    // Removes an enemy from the attack range list
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
            enemiesInRange.Remove(other.transform);
    }

    // Applies damage to the tower
    public void TakeDamage(float damage)
    {
        health?.TakeDamage(damage);
    }
}
