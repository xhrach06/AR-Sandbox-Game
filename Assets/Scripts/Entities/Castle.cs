using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class Castle : MonoBehaviour
{
    public GameObject projectilePrefab;
    public float attackRange = 20f;
    public float attackDamage = 30f;
    public float attackCooldown = 3f;
    public Transform firePoint;

    public Health health;
    private float lastAttackTime;
    private readonly List<Transform> enemiesInRange = new List<Transform>();
    public GameObject healthBarPrefab;


    void Awake()
    {
        if (SceneManager.GetActiveScene().name == "CalibrationScene")
        {
            this.enabled = false; // ⛔ disable this script entirely
            return;
        }
    }

    void Start()
    {

        health = GetComponent<Health>();
        if (health == null)
        {
            Debug.LogError("❌ Health component is missing on " + gameObject.name);
            return;
        }

        GameObject canvas = GameObject.Find("HealthBarCanvas");
        if (canvas == null)
        {
            Debug.LogError("❌ HealthBarCanvas not found in the scene!");
            return;
        }

        GameObject bar = Instantiate(healthBarPrefab, canvas.transform);

        FollowWorldTarget follow = bar.GetComponent<FollowWorldTarget>();
        if (follow != null)
        {
            follow.SetTarget(transform);
        }
        else
        {
            Debug.LogWarning("⚠️ No FollowWorldTarget found on HealthBar prefab!");
        }

        HealthBar healthBar = bar.GetComponent<HealthBar>();
        if (healthBar != null)
        {
            healthBar.SetHealth(health);
        }
        else
        {
            Debug.LogWarning("⚠️ No HealthBar script found on HealthBar prefab!");
        }
        health.SetLinkedHealthBar(bar);

        Debug.Log("💡 Spawned health bar for " + gameObject.name);
    }
    void Update()
    {
        // Remove destroyed enemies from the list
        enemiesInRange.RemoveAll(enemy => enemy == null);
        // Attack enemies if cooldown has passed
        if (Time.time - lastAttackTime > attackCooldown && enemiesInRange.Count > 0)
        {
            Transform target = enemiesInRange[0];
            if (target != null) // Check if the target is still valid
            {
                Shoot(target);
                lastAttackTime = Time.time;
            }
        }
    }


    void Shoot(Transform target)
    {
        //Debug.Log("Shooting at: " + target.name);

        if (projectilePrefab == null || firePoint == null) return;

        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        Projectile projScript = projectile.GetComponent<Projectile>();
        if (projScript != null)
        {
            projScript.SetTarget(target, 30f);
        }
    }

    // Method to take damage
    public void TakeDamage(float damage)
    {
        if (health != null)
        {
            health.TakeDamage(damage); // Call the Health component's TakeDamage method
        }
        else
        {
            Debug.LogError("Health component is missing on " + gameObject.name + ", unable to take damage!");
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            enemiesInRange.Add(other.transform);
            //Debug.Log("Enemy entered range: " + other.name);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            enemiesInRange.Remove(other.transform);
            //Debug.Log("Enemy left range: " + other.name);
        }
    }

}
