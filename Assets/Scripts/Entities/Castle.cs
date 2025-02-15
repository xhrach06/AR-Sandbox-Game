using System.Collections.Generic;
using UnityEngine;

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


    void Start()
    {
        health = GetComponent<Health>(); // Get the Health component
        if (health == null)
        {
            Debug.LogError("Health component is missing on " + gameObject.name);
        }
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
