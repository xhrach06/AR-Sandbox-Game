using UnityEngine;
using System.Collections.Generic;

public class Enemy : MonoBehaviour
{
    public float baseHealth = 100f;
    public float baseDamage = 10f;
    public float attackRange = 10f;
    public float attackCooldown = 1f;
    public float damageToCastle = 10f;
    public float moveSpeed = 3f;
    public float avoidRadius = 5f; // Radius to avoid towers

    public Health health;
    public float offsetDistance = 2f; // Distance to move around towers
    private Transform target;
    private Health targetHealth;
    private float lastAttackTime;
    private float currentHealth;
    private float currentDamage;

    private List<Node> path;  // The A* path
    private int pathIndex = 0; // Tracks position in the path
    private Pathfinding pathfinding; // Reference to pathfinding system
    private GridManager grid; // Reference to the grid

    // private NavMeshAgent agent; // NAVMESH REMOVED

    void Start()
    {
        // agent = GetComponent<NavMeshAgent>(); // NAVMESH REMOVED
        health = GetComponent<Health>();
        currentHealth = baseHealth;
        currentDamage = baseDamage;

        pathfinding = FindObjectOfType<Pathfinding>(); // A* Pathfinding System
        grid = FindObjectOfType<GridManager>(); // Grid for pathfinding

        if (target == null)
            target = GameObject.FindGameObjectWithTag("Castle").transform;

        if (target != null)
            targetHealth = target.GetComponent<Health>();

        // Ensure enemy starts inside the grid
        Vector3 clampedPosition = new Vector3(
            Mathf.Clamp(transform.position.x, 0, grid.gridSize.x * grid.nodeSize),
            transform.position.y,
            Mathf.Clamp(transform.position.z, 0, grid.gridSize.y * grid.nodeSize)
        );
        transform.position = clampedPosition;

        // Call FindNewPath every 1 second to update movement
        InvokeRepeating("FindNewPath", 0f, 1f);
    }

    void Update()
    {
        if (path != null && pathIndex < path.Count)
        {
            Vector3 nextPosition = path[pathIndex].worldPosition;

            // 🔹 Check for nearby towers and adjust path to avoid them
            Collider[] nearbyTowers = Physics.OverlapSphere(transform.position, avoidRadius);
            foreach (var towerCollider in nearbyTowers)
            {
                if (towerCollider.CompareTag("Tower"))
                {
                    Vector3 towerPosition = towerCollider.transform.position;
                    Vector3 directionAway = (transform.position - towerPosition).normalized;
                    nextPosition += directionAway * offsetDistance; // Adjust position to move around the tower
                }
            }

            // 🔹 Move towards the next node in the path
            transform.position = Vector3.MoveTowards(transform.position, nextPosition, moveSpeed * Time.deltaTime);

            // If the enemy reaches the node, move to the next one
            if (Vector3.Distance(transform.position, nextPosition) < 0.5f)
                pathIndex++;

            Debug.Log($"🚶 Enemy moving to next node. Path Index: {pathIndex}/{path.Count}");
        }

        // 🔹 Attack if in range of the castle
        if (target != null && Vector3.Distance(transform.position, target.position) < attackRange)
        {
            Attack();
        }
    }

    // 🔹 Find a new path to the castle using A*
    void FindNewPath()
    {
        if (target != null)
        {
            path = pathfinding.FindPath(transform.position, target.position);
            pathIndex = 0;

            if (path == null || path.Count == 0)
            {
                Debug.LogError("❌ Enemy did not receive a valid path!");
            }
            else
            {
                Debug.Log($"✅ Enemy path received! Moving towards {path.Count} nodes.");
            }
        }
    }

    // 🔹 Set the target for the enemy
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        targetHealth = target.GetComponent<Health>();
    }

    // 🔹 Set the strength of the enemy based on a multiplier (for different levels)
    public void SetStrengthMultiplier(float multiplier)
    {
        currentHealth = baseHealth * multiplier;
        currentDamage = baseDamage * multiplier;
        health.SetHealth(currentHealth);
    }

    // 🔹 Attack logic
    void Attack()
    {
        if (targetHealth != null && Time.time - lastAttackTime > attackCooldown)
        {
            targetHealth.TakeDamage(currentDamage);
            Debug.Log("Enemy attacked " + target.name + " for " + currentDamage + " damage.");
            lastAttackTime = Time.time;
        }
    }

    public void TakeDamage(float damage)
    {
        health.TakeDamage(damage);
    }

    /*
    // NAVMESH REMOVED
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
    */

    // 🔹 Visualize enemy path
    void OnDrawGizmos()
    {
        if (path != null && path.Count > 0)
        {
            Gizmos.color = Color.cyan; // Set the color of the path

            for (int i = 0; i < path.Count - 1; i++)
            {
                Gizmos.DrawLine(path[i].worldPosition, path[i + 1].worldPosition); // Draw lines between nodes
            }

            // Draw a sphere at each node for better visualization
            foreach (Node node in path)
            {
                Gizmos.DrawSphere(node.worldPosition, 2f); // Adjust size if needed
            }
        }
    }
}
