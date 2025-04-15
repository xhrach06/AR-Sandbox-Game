using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Handles enemy pathfinding, movement, attacking, and health.
/// </summary>
public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    public float baseHealth = 100f;
    public float baseDamage = 10f;
    public float attackRange = 10f;
    public float attackCooldown = 1f;
    public float damageToCastle = 10f;
    public float moveSpeed = 3f;
    public float avoidRadius = 5f;
    public float offsetDistance = 2f;
    [Range(0, 100)] public float barrierPassChance = 50f;

    [Header("UI & Components")]
    public GameObject healthBarPrefab;

    private float lastAttackTime;
    private float currentHealth;
    private float currentDamage;
    private bool isInitialized = false;
    private bool isAttackingCastle = false;

    private Transform target;
    private Health targetHealth;
    private Health health;
    private Pathfinding pathfinding;
    private GridManager grid;
    private LineRenderer pathLine;

    private List<Node> path;
    private int pathIndex;

    public System.Action<Enemy> OnEnemyDeath;

    private void Start()
    {
        pathLine = GetComponent<LineRenderer>();
        health = GetComponent<Health>();
        currentHealth = baseHealth;
        currentDamage = baseDamage;

        pathfinding = FindObjectOfType<Pathfinding>();
        grid = FindObjectOfType<GridManager>();

        SetupHealthBar();

        target = GameObject.FindGameObjectWithTag("Castle")?.transform;
        if (target != null)
            targetHealth = target.GetComponent<Health>();

        ClampStartPositionToGrid();

        isInitialized = true;
        FindNewPath();
    }

    private void Update()
    {
        if (!isAttackingCastle && path != null && pathIndex < path.Count)
            FollowPath();

        if (ShouldAttack())
            Attack();
    }

    private void SetupHealthBar()
    {
        GameObject canvas = GameObject.Find("HealthBarCanvas");
        if (canvas == null || healthBarPrefab == null)
        {
            Debug.LogError("❌ HealthBarCanvas or HealthBarPrefab missing for " + gameObject.name);
            return;
        }

        GameObject bar = Instantiate(healthBarPrefab, canvas.transform);

        FollowWorldTarget follow = bar.GetComponent<FollowWorldTarget>();
        if (follow != null) follow.SetTarget(transform);

        HealthBar healthBar = bar.GetComponent<HealthBar>();
        if (healthBar != null) healthBar.SetHealth(health);

        health.SetLinkedHealthBar(bar);
    }

    private void ClampStartPositionToGrid()
    {
        Vector3 clampedPosition = new Vector3(
            Mathf.Clamp(transform.position.x, 0, grid.gridSize.x * grid.nodeSize),
            transform.position.y,
            Mathf.Clamp(transform.position.z, 0, grid.gridSize.y * grid.nodeSize)
        );
        transform.position = clampedPosition;
    }

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

    private void FollowPath()
    {
        Vector3 nextPosition = path[pathIndex].worldPosition;

        // Avoid towers in proximity
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

        transform.position = Vector3.MoveTowards(transform.position, nextPosition, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, nextPosition) < 0.5f)
            pathIndex++;

        Debug.Log($"🚶 Enemy moving to next node. Path Index: {pathIndex}/{path.Count}");
    }

    private bool ShouldAttack()
    {
        return (isAttackingCastle && targetHealth != null) ||
               (target != null && Vector3.Distance(transform.position, target.position) < attackRange);
    }

    private void Attack()
    {
        if (Time.time - lastAttackTime < attackCooldown || targetHealth == null) return;

        targetHealth.TakeDamage(currentDamage);
        Debug.Log("Enemy attacked " + target.name + " for " + currentDamage + " damage.");
        lastAttackTime = Time.time;
    }

    public void TakeDamage(float damage)
    {
        health.TakeDamage(damage);
        Debug.Log("Enemy took damage: " + damage);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        targetHealth = target.GetComponent<Health>();
    }

    public void SetStrengthMultiplier(float multiplier)
    {
        currentHealth = baseHealth * multiplier;
        currentDamage = baseDamage * multiplier;
        health.SetHealth(currentHealth);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Castle"))
        {
            isAttackingCastle = true;
            Debug.Log("⚔️ Enemy collided with the castle and stopped to attack.");
        }
    }

    private void OnDrawGizmos()
    {
        if (path == null || path.Count <= 1) return;

        Gizmos.color = Color.cyan;

        for (int i = 0; i < path.Count - 1; i++)
            Gizmos.DrawLine(path[i].worldPosition, path[i + 1].worldPosition);

        foreach (Node node in path)
            Gizmos.DrawSphere(node.worldPosition, 2f);
    }

    public void DrawPathRuntime()
    {
        if (pathLine == null || path == null || path.Count < 2)
        {
            if (pathLine != null) pathLine.positionCount = 0;
            return;
        }

        pathLine.positionCount = path.Count;
        for (int i = 0; i < path.Count; i++)
        {
            pathLine.SetPosition(i, path[i].worldPosition + Vector3.up * 0.5f);
        }
    }
}
