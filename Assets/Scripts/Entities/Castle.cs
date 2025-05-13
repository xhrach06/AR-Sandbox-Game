using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Castle logic for combat and health handling.
/// </summary>
public class Castle : MonoBehaviour
{
    [Header("Attack Settings")]
    public GameObject projectilePrefab;
    public float attackRange = 20f;
    public float attackDamage = 30f;
    public float attackCooldown = 3f;
    public Transform firePoint;

    [Header("Health")]
    public Health health;
    public GameObject healthBarPrefab;

    private float lastAttackTime;
    private readonly List<Transform> enemiesInRange = new();
    // Disables the castle in CalibrationScene
    private void Awake()
    {
        if (SceneManager.GetActiveScene().name == "CalibrationScene")
        {
            enabled = false;
            return;
        }
    }
    // Initializes health and health bar UI
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
    // Handles attack timing and targeting
    private void Update()
    {
        enemiesInRange.RemoveAll(enemy => enemy == null);

        if (Time.time - lastAttackTime > attackCooldown && enemiesInRange.Count > 0)
        {
            Transform target = enemiesInRange[0];
            if (target != null)
            {
                Shoot(target);
                lastAttackTime = Time.time;
            }
        }
    }
    // Fires a projectile at a given target
    private void Shoot(Transform target)
    {
        if (projectilePrefab == null || firePoint == null) return;

        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        Projectile projScript = projectile.GetComponent<Projectile>();
        projScript?.SetTarget(target, attackDamage);
    }
    // Applies damage to the castle
    public void TakeDamage(float damage)
    {
        health?.TakeDamage(damage);
    }

    // Adds enemies to tracking list when they enter range
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
            enemiesInRange.Add(other.transform);
    }

    // Removes enemies from tracking list when they leave range
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
            enemiesInRange.Remove(other.transform);
    }
}
