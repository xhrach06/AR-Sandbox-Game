using UnityEngine;

/// <summary>
/// Logic for projectile movement, hit/miss behavior, and damage application.
/// </summary>
public class Projectile : MonoBehaviour
{
    public float speed = 10f;
    public float damage = 10f;
    public float lifetime = 5f;

    private Transform target;
    private Vector3 moveDirection;
    private bool isMiss = false;
    private float lifetimeTimer;

    public void SetTarget(Transform newTarget, float attackDamage)
    {
        target = newTarget;
        damage = attackDamage;
        lifetimeTimer = 0f;
    }

    public void SetMissDirection(Vector3 dir)
    {
        moveDirection = dir;
        isMiss = true;
        Destroy(gameObject, 3f); // Auto-destroy missed projectile
    }

    private void Update()
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
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform != target) return;

        Health health = other.GetComponent<Health>();
        if (health != null)
            health.TakeDamage(damage);

        Destroy(gameObject);
    }
}
