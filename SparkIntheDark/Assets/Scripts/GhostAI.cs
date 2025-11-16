using UnityEngine;
using UnityEngine.AI;

public class GhostAI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The player transform to chase/flee from")]
    public Transform player;

    private NavMeshAgent agent;

    [Header("AI States")]
    public GhostState currentState = GhostState.Chase;

    [Header("Chase Settings")]
    [Tooltip("Speed when chasing player")]
    [Range(1f, 10f)]
    public float chaseSpeed = 3.5f;

    [Tooltip("How often to update path (seconds)")]
    [Range(0.1f, 1f)]
    public float pathUpdateInterval = 0.2f;

    [Header("Flee Settings")]
    [Tooltip("Speed when fleeing from player")]
    [Range(1f, 10f)]
    public float fleeSpeed = 2.5f;

    [Tooltip("Distance to maintain from player when fleeing")]
    [Range(5f, 20f)]
    public float fleeDistance = 10f;

    [Header("Rotation Settings")]
    [Tooltip("Snap rotation to 90 degree increments (4 directions)")]
    public bool snapRotation = true;

    [Tooltip("Speed of rotation snapping")]
    [Range(1f, 20f)]
    public float rotationSpeed = 10f;

    [Header("Visual Feedback")]
    [Tooltip("Material when in chase mode")]
    public Material chaseMaterial;

    [Tooltip("Material when in flee mode (vulnerable)")]
    public Material fleeMaterial;

    [Tooltip("Material when being eaten (returning to spawn)")]
    public Material eatenMaterial;

    private Renderer enemyRenderer;
    private float pathUpdateTimer;
    private Vector3 spawnPoint;
    private bool isEaten = false;
    private Vector3 lastPosition;

    public enum GhostState
    {
        Chase,
        Flee,
        Eaten,
        Respawning
    }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        enemyRenderer = GetComponent<Renderer>();
        spawnPoint = transform.position;
        lastPosition = transform.position;

        // Disable NavMeshAgent's built-in rotation
        if (snapRotation)
        {
            agent.updateRotation = false;
        }

        // Auto-find player if not assigned
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }

        SetState(GhostState.Chase);
    }

    void Update()
    {
        if (player == null || isEaten) return;

        pathUpdateTimer += Time.deltaTime;

        if (pathUpdateTimer >= pathUpdateInterval)
        {
            pathUpdateTimer = 0f;
            UpdateBehavior();
        }

        // Handle rotation
        if (snapRotation)
        {
            HandleSnapRotation();
        }
    }

    void HandleSnapRotation()
    {
        // Calculate movement direction
        Vector3 moveDirection = (transform.position - lastPosition).normalized;

        // Only rotate if moving
        if (moveDirection.magnitude > 0.01f)
        {
            // Get the angle based on direction, offset by 90 degrees
            float targetAngle = GetSnapAngleWithOffset(moveDirection);

            // Smoothly rotate to target angle
            Quaternion targetRotation = Quaternion.Euler(0f, targetAngle, 0f);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        lastPosition = transform.position;
    }

    float GetSnapAngleWithOffset(Vector3 direction)
    {
        // Calculate angle from direction
        float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

        // Add 90 degrees to rotate perpendicular to movement direction
        angle += 90f;

        // Snap to nearest 90 degrees (0, 90, 180, 270)
        angle = Mathf.Round(angle / 90f) * 90f;

        return angle;
    }

    void UpdateBehavior()
    {
        switch (currentState)
        {
            case GhostState.Chase:
                ChasePlayer();
                break;

            case GhostState.Flee:
                FleeFromPlayer();
                break;

            case GhostState.Eaten:
                ReturnToSpawn();
                break;
        }
    }

    void ChasePlayer()
    {
        // Move directly towards player
        agent.SetDestination(player.position);
    }

    void FleeFromPlayer()
    {
        if (player == null) return;

        // Calculate direction away from player
        Vector3 directionAwayFromPlayer = (transform.position - player.position).normalized;

        // Calculate multiple potential flee positions at different distances
        Vector3 fleePosition = transform.position + directionAwayFromPlayer * fleeDistance;

        // Try to find a valid position on the NavMesh
        NavMeshHit hit;
        bool foundPosition = false;

        // Try at full flee distance first
        if (NavMesh.SamplePosition(fleePosition, out hit, fleeDistance * 0.5f, NavMesh.AllAreas))
        {
            foundPosition = true;
        }
        // If that fails, try shorter distances
        else
        {
            for (float distance = fleeDistance * 0.75f; distance >= fleeDistance * 0.25f; distance -= fleeDistance * 0.25f)
            {
                fleePosition = transform.position + directionAwayFromPlayer * distance;
                if (NavMesh.SamplePosition(fleePosition, out hit, distance * 0.5f, NavMesh.AllAreas))
                {
                    foundPosition = true;
                    break;
                }
            }
        }

        // Set destination if we found a valid flee position
        if (foundPosition)
        {
            agent.SetDestination(hit.position);
        }
        else
        {
            // Fallback: try random nearby positions away from player
            for (int i = 0; i < 8; i++)
            {
                float angle = i * 45f; // Try 8 directions
                Vector3 direction = Quaternion.Euler(0, angle, 0) * directionAwayFromPlayer;
                Vector3 testPosition = transform.position + direction * fleeDistance * 0.5f;

                if (NavMesh.SamplePosition(testPosition, out hit, fleeDistance * 0.3f, NavMesh.AllAreas))
                {
                    agent.SetDestination(hit.position);
                    break;
                }
            }
        }
    }


    void ReturnToSpawn()
    {
        agent.SetDestination(spawnPoint);

        // Check if reached spawn point
        if (Vector3.Distance(transform.position, spawnPoint) < 1f)
        {
            Respawn();
        }
    }

    public void SetState(GhostState newState)
    {
        currentState = newState;

        switch (newState)
        {
            case GhostState.Chase:
                agent.speed = chaseSpeed;
                if (chaseMaterial != null && enemyRenderer != null)
                    enemyRenderer.material = chaseMaterial;
                isEaten = false;
                break;

            case GhostState.Flee:
                agent.speed = fleeSpeed;
                if (fleeMaterial != null && enemyRenderer != null)
                    enemyRenderer.material = fleeMaterial;
                isEaten = false;
                break;

            case GhostState.Eaten:
                agent.speed = chaseSpeed * 1.5f; // Return faster
                if (eatenMaterial != null && enemyRenderer != null)
                    enemyRenderer.material = eatenMaterial;
                isEaten = true;
                break;
        }
    }

    public void GetEaten()
    {
        if (currentState == GhostState.Flee)
        {
            SetState(GhostState.Eaten);

            // Notify game manager for score
            GameManager gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                gameManager.OnGhostEaten();
            }
        }
    }

    void Respawn()
    {
        SetState(GhostState.Chase);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController playerController = other.GetComponent<PlayerController>();

            if (currentState == GhostState.Flee)
            {
                // Player eats ghost
                GetEaten();
            }
            else if (currentState == GhostState.Chase)
            {
                // Ghost damages/kills player
                if (playerController != null)
                {
                    playerController.TakeDamage();
                }
            }
        }
    }

    // Debug visualization
    void OnDrawGizmos()
    {
        if (currentState == GhostState.Flee)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, fleeDistance);
        }
    }
}
