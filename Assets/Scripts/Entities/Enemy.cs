using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public float baseHealth = 100f;
    public float baseDamage = 10f;
    public float attackRange = 10f;
    public float attackCooldown = 1f;
    public float damageToCastle = 10f;

    public Health health;
    public float offsetDistance = 2f; // Distance to move around towers
    private Transform target;
    private Health targetHealth;
    private NavMeshAgent agent;
    private float lastAttackTime;
    private float currentHealth;
    private float currentDamage;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<Health>();
        currentHealth = baseHealth;
        currentDamage = baseDamage;
    }

    void Update()
    {
        if (target != null)
        {
            Vector3 targetPosition = target.position;
            Vector3 adjustedPosition = targetPosition;

            // Check for nearby towers within a certain radius
            Collider[] nearbyTowers = Physics.OverlapSphere(transform.position, 5f);

            foreach (var towerCollider in nearbyTowers)
            {
                if (towerCollider.CompareTag("Tower"))
                {
                    // Calculate a direction to move around the tower
                    Vector3 towerPosition = towerCollider.transform.position;
                    Vector3 directionFromTower = (transform.position - towerPosition).normalized;

                    // Offset the enemy's position slightly to avoid the tower
                    adjustedPosition = transform.position + directionFromTower * offsetDistance;

                    // Ensure that the enemy still moves towards the castle (main target) but avoids the tower
                    adjustedPosition = Vector3.Lerp(adjustedPosition, targetPosition, 0.9f); // 90% towards the target, 10% to avoid tower
                }
            }

            float distance = Vector3.Distance(transform.position, adjustedPosition);

            if (distance > attackRange)
            {
                if (agent.isOnNavMesh) // Ensure the agent is on the NavMesh
                {
                    agent.SetDestination(adjustedPosition);
                    agent.isStopped = false;
                }
                else
                {
                    Debug.LogWarning(gameObject.name + " is not on the NavMesh!");
                    Destroy(gameObject); // Destroy agents not on the NavMesh
                }
            }
            else
            {
                // Stop the enemy and start attacking
                agent.isStopped = true;

                if (Time.time - lastAttackTime > attackCooldown)
                {
                    Attack();
                    lastAttackTime = Time.time;
                }
            }
        }
    }


    // Set the target for the enemy
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        targetHealth = target.GetComponent<Health>();
    }

    // Set the strength of the enemy based on a multiplier (for different levels)
    public void SetStrengthMultiplier(float multiplier)
    {
        currentHealth = baseHealth * multiplier;
        currentDamage = baseDamage * multiplier;
        health.SetHealth(currentHealth); 
    }

    // Attack logic
    void Attack()
    {
        if (targetHealth != null)
        {
            targetHealth.TakeDamage(currentDamage);
            Debug.Log("Enemy attacked " + target.name + " for " + currentDamage + " damage.");
        }
    }

    public void TakeDamage(float damage)
    {
        health.TakeDamage(damage);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Castle"))
        {
            // Stop the NavMeshAgent
            if (agent != null && agent.isOnNavMesh)
            {
                agent.isStopped = true;
            }

            // Deal damage to the Castle
            Castle castle = collision.gameObject.GetComponent<Castle>();
            if (castle != null)
            {
                castle.TakeDamage(damageToCastle);
                Debug.Log(gameObject.name + " collided with " + collision.gameObject.name + " and dealt " + damageToCastle + " damage.");
            }
        }
    }

}
