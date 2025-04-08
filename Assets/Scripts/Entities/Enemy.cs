using UnityEngine;
using System.Collections.Generic;

public class Enemy : MonoBehaviour
{
    private LineRenderer pathLine;
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

    [Range(0, 100)]
    public float barrierPassChance = 50f;
    private List<Node> path;  // The A* path
    private int pathIndex = 0; // Tracks position in the path
    private Pathfinding pathfinding; // Reference to pathfinding system
    private GridManager grid; // Reference to the grid
    public System.Action<Enemy> OnEnemyDeath;

    private bool isAttackingCastle = false; // ✅ NEW: flag to stop movement when at the castle
    private bool isInitialized = false;

    // private NavMeshAgent agent; // NAVMESH REMOVED
    public GameObject healthBarPrefab;
    void Start()
    {
        pathLine = GetComponent<LineRenderer>();
        health = GetComponent<Health>();
        currentHealth = baseHealth;
        currentDamage = baseDamage;

        pathfinding = FindObjectOfType<Pathfinding>();
        grid = FindObjectOfType<GridManager>();

        GameObject canvas = GameObject.Find("HealthBarCanvas");
        if (canvas != null && healthBarPrefab != null)
        {
            GameObject bar = Instantiate(healthBarPrefab, canvas.transform);

            FollowWorldTarget follow = bar.GetComponent<FollowWorldTarget>();
            if (follow != null)
            {
                follow.SetTarget(transform);
            }

            HealthBar healthBar = bar.GetComponent<HealthBar>();
            if (healthBar != null)
            {
                healthBar.SetHealth(health);
            }
            health.SetLinkedHealthBar(bar);
            Debug.Log("Enemy health: " + health.GetCurrentHealth());
        }
        else
        {
            Debug.LogError("❌ HealthBarCanvas or HealthBarPrefab missing for " + gameObject.name);
        }

        if (target == null)
            target = GameObject.FindGameObjectWithTag("Castle").transform;

        if (target != null)
            targetHealth = target.GetComponent<Health>();

        Vector3 clampedPosition = new Vector3(
            Mathf.Clamp(transform.position.x, 0, grid.gridSize.x * grid.nodeSize),
            transform.position.y,
            Mathf.Clamp(transform.position.z, 0, grid.gridSize.y * grid.nodeSize)
        );
        transform.position = clampedPosition;
        //InvokeRepeating("FindNewPath", 0f, 1f);
        isInitialized = true;
        FindNewPath();

    }
    void Update()
    {
        // 🔹 Enemy movement toward path nodes (if not attacking the castle)
        if (!isAttackingCastle && path != null && pathIndex < path.Count)
        {
            Vector3 nextPosition = path[pathIndex].worldPosition;

            // 🔹 Avoid nearby towers by adjusting movement
            Collider[] nearbyTowers = Physics.OverlapSphere(transform.position, avoidRadius);
            foreach (var towerCollider in nearbyTowers)
            {
                if (towerCollider.CompareTag("Tower"))
                {
                    Vector3 towerPosition = towerCollider.transform.position;
                    Vector3 directionAway = (transform.position - towerPosition).normalized;
                    nextPosition += directionAway * offsetDistance;
                }
            }

            // 🔹 Move toward next path node
            transform.position = Vector3.MoveTowards(transform.position, nextPosition, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, nextPosition) < 0.5f)
                pathIndex++;

            Debug.Log($"🚶 Enemy moving to next node. Path Index: {pathIndex}/{path.Count}");
        }

        // 🔹 Attack logic:
        if (isAttackingCastle && targetHealth != null)
        {
            Attack(); // Actively attacking while inside the castle
        }
        else if (target != null && Vector3.Distance(transform.position, target.position) < attackRange)
        {
            Attack(); // Attack if close enough, even if not collided yet
        }
    }

    /*
                // 🔹 Check for nearby barriers
                Collider[] nearbyBarriers = Physics.OverlapSphere(transform.position, avoidRadius);
                foreach (var barrierCollider in nearbyBarriers)
                {
                    if (barrierCollider.CompareTag("Barrier"))
                    {
                        float passChance = Random.Range(0f, 100f); // Roll a random chance

                        if (passChance > barrierPassChance)
                        {
                            Debug.Log("❌ Enemy avoids the barrier! Recalculating path...");
                            FindNewPath(); // Force path recalculation
                            return; // Stop further movement processing
                        }
                        else
                        {
                            Debug.Log("✅ Enemy crosses the barrier!");
                        }
                    }
                }
                */
    // 🔹 Find a new path to the castle using A*
    public void FindNewPath()
    {
        if (!isInitialized || pathfinding == null || target == null) return;

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

        DrawPathRuntime();
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
        Debug.Log("took damage: " + damage);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Castle"))
        {
            isAttackingCastle = true;
            Debug.Log("⚔️ Enemy collided with the castle and stopped to attack.");
        }
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
    public void DrawPathRuntime()
    {
        if (path == null || path.Count < 2 || pathLine == null)
        {
            pathLine.positionCount = 0;
            return;
        }

        pathLine.positionCount = path.Count;

        for (int i = 0; i < path.Count; i++)
        {
            pathLine.SetPosition(i, path[i].worldPosition + Vector3.up * 0.5f); // slightly above ground
        }
    }

}
