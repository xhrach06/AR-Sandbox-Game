using UnityEngine;

public class Meteor : MonoBehaviour
{
    public float damageRadius = 5f; // Area of effect
    public float damage = 50f; // Damage dealt to enemies
    public ParticleSystem explosionEffect; // Explosion VFX

    void Start()
    {
        // Trigger explosion after falling
        Invoke(nameof(Explode), 1f); // Delays the explosion for visual effect
    }

    void Explode()
    {
        // Play explosion effect
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        // Damage all enemies in the radius
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, damageRadius);
        foreach (Collider hit in hitColliders)
        {
            if (hit.CompareTag("Enemy"))
            {
                Enemy enemy = hit.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                }
            }
        }

        Destroy(gameObject); // Destroy the meteor after the explosion
    }
}
