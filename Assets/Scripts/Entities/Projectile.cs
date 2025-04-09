using UnityEngine;
using UnityEngine.AI;

public class Projectile : MonoBehaviour
{
    public float speed = 10f;  // Speed of the projectile
    public float damage = 10f; // Damage dealt by the projectile
    private Transform target;
    public float lifetime = 5f; // Time until the projectile is destroyed if it doesn't hit anything
    private float lifetimeTimer; // Timer to track projectile's lifetime
    private Vector3 moveDirection;
    private bool isMiss = false;

    // Set the target of the projectile
    public void SetTarget(Transform newTarget, float attackDamage)
    {
        target = newTarget;
        damage = attackDamage;
        lifetimeTimer = 0f; // Reset timer when a target is set
    }

    public void SetMissDirection(Vector3 dir)
    {
        moveDirection = dir;
        isMiss = true;
        Destroy(gameObject, 3f); // auto-destroy after flying away
    }

    void Update()
    {
        if (isMiss)
        {
            transform.position += moveDirection * speed * Time.deltaTime;
            return;
        }

        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 direction = (target.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;

        /*
        if (Vector3.Distance(transform.position, target.position) < 0.5f)
        {
            HitTarget();
        }

        lifetimeTimer += Time.deltaTime;
        if (lifetimeTimer >= lifetime)
        {
            Destroy(gameObject); // Destroy if lifetime exceeded
        }
        */
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if we hit the target
        if (other.transform == target)
        {
            Health health = other.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(damage); // Apply damage to the target
            }

            Destroy(gameObject); // Destroy the projectile on impact
        }
    }


    void HitTarget()
    {
        Health targetHealth = target.GetComponent<Health>();
        if (targetHealth != null)
        {
            targetHealth.TakeDamage(damage); // Call TakeDamage on the Health component
            //Debug.Log("Hit target: " + target.name + " for " + damage + " damage.");
        }

        // Destroy the projectile after hitting the target
        Destroy(gameObject);
    }


}
