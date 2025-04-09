using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour
{
    public GameObject projectilePrefab;
    public float attackRange = 15f;
    public float attackCooldown = 2f;
    public float attackDamage = 20f;
    public Transform firePoint;

    [Range(0f, 1f)]
    public float hitChance = 0.85f; // 💥 Chance to actually hit the target

    private float lastAttackTime;
    private readonly List<Transform> enemiesInRange = new List<Transform>();
    private Health health;
    public GameObject healthBarPrefab;

    void Start()
    {
        health = GetComponent<Health>();
        if (health == null)
        {
            //Debug.LogError("❌ Health component is missing on " + gameObject.name);
            return;
        }

        GameObject canvas = GameObject.Find("HealthBarCanvas");
        if (canvas == null)
        {
            //Debug.LogError("❌ HealthBarCanvas not found in the scene!");
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
            //Debug.LogWarning("⚠️ No FollowWorldTarget found on HealthBar prefab!");
        }

        HealthBar healthBar = bar.GetComponent<HealthBar>();
        if (healthBar != null)
        {
            healthBar.SetHealth(health);
        }
        else
        {
            //Debug.LogWarning("⚠️ No HealthBar script found on HealthBar prefab!");
        }
        health.SetLinkedHealthBar(bar);

        //Debug.Log("💡 Spawned health bar for " + gameObject.name);
    }

    void Update()
    {
        enemiesInRange.RemoveAll(enemy => enemy == null);

        float shootTime = Random.Range(attackCooldown - attackCooldown / 2, attackCooldown + attackCooldown / 2);
        if (Time.time - lastAttackTime > shootTime && enemiesInRange.Count > 0)
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
            //Debug.LogError("Projectile prefab or firePoint is missing!");
            return;
        }

        bool didHit = Random.value <= hitChance;

        Vector3 direction = (target.position - firePoint.position).normalized;

        // If it missed, apply a small offset
        if (!didHit)
        {
            direction += new Vector3(
                Random.Range(-0.3f, 0.3f),
                Random.Range(-0.3f, 0.3f),
                Random.Range(-0.3f, 0.3f)
            );

            direction.Normalize(); // Make sure the direction stays valid
            //Debug.Log("🎯 MISS! Tower fired but missed the enemy.");
        }

        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.LookRotation(direction));
        Projectile projScript = projectile.GetComponent<Projectile>();
        if (projScript != null)
        {
            if (didHit)
            {
                projScript.SetTarget(target, attackDamage);
            }
            else
            {
                projScript.SetMissDirection(direction); // You’ll need this method in your Projectile script
            }
        }
    }

    Transform SelectTarget()
    {
        int index = Random.Range(0, enemiesInRange.Count);
        return enemiesInRange[index] != null ? enemiesInRange[index] : enemiesInRange[0];
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
            //Debug.LogError("Health component is missing on Tower: " + gameObject.name);
        }
    }
}
