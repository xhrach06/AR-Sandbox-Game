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
        isMiss = true;
        Quaternion lookRotation = Quaternion.LookRotation(dir);
        transform.rotation = lookRotation * Quaternion.Euler(90, 0, 0);
        Destroy(gameObject, 3f);
    }

    private void Update()
    {
        if (isMiss)
        {
            transform.position += moveDirection * speed * Time.deltaTime;

            // Rotate to face movement direction
            if (moveDirection != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(moveDirection);

            return;
        }

        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 direction = (target.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;

        // Rotate to face the target direction
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = lookRotation * Quaternion.Euler(90, 0, 0);
        }
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
