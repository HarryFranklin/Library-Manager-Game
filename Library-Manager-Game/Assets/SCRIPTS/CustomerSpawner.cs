using UnityEngine;
using System.Collections;

public class CustomerSpawner : MonoBehaviour
{
    [Header("Spawn Configuration")]
    public GameObject customerPrefab;
    [Tooltip("Where the customers will physically appear.")]
    public Transform spawnPoint;
    
    [Tooltip("Time in seconds between each spawn attempt.")]
    public float spawnInterval = 5f; 
    
    [Tooltip("Maximum number of customers allowed in the library at once.")]
    public int maxCustomers = 10;

    [Header("Current State")]
    public bool isSpawning = true;
    
    // Tracks the current population to prevent overcrowding
    private int currentCustomerCount = 0;

    private void Start()
    {
        // Fallback: If no spawn point is assigned, use the Spawner's own position
        if (spawnPoint == null) 
        {
            spawnPoint = transform;
        }

        // Start the infinite spawning loop
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            if (isSpawning && currentCustomerCount < maxCustomers)
            {
                SpawnCustomer();
            }
            
            // Pause this specific function for 'spawnInterval' seconds before looping again
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnCustomer()
{
    if (customerPrefab == null)
    {
        Debug.LogWarning("Spawner: No Customer Prefab assigned!");
        return;
    }

    // 1. Determine the raw spawn position
    Vector3 rawPos = spawnPoint.position;

    // 2. Adjust for the "Centre Pivot" of primitive shapes
    // A rigged 3D human model usually has its pivot at its feet, but Unity Capsules 
    // have it in the middle. We read the agent's height to push it up perfectly.
    UnityEngine.AI.NavMeshAgent prefabAgent = customerPrefab.GetComponent<UnityEngine.AI.NavMeshAgent>();
    if (prefabAgent != null)
    {
        // Push the centre up by half the height so the "feet" touch the spawn point
        rawPos.y += (prefabAgent.height / 2f);
    }

    // 3. Snap to the NavMesh
    // This prevents the AI from breaking if the spawn point is accidentally 
    // placed slightly off the grid or inside a wall.
    if (UnityEngine.AI.NavMesh.SamplePosition(rawPos, out UnityEngine.AI.NavMeshHit hit, 2.0f, UnityEngine.AI.NavMesh.AllAreas))
    {
        // 4. Instantiate at the mathematically safe NavMesh point
        GameObject newCustomer = Instantiate(customerPrefab, hit.position, spawnPoint.rotation);
        
        CustomerAI ai = newCustomer.GetComponent<CustomerAI>();
        if (ai != null)
        {
            ai.SetSpawner(this);
        }

        currentCustomerCount++;
        Debug.Log($"Customer spawned safely. Current population: {currentCustomerCount}/{maxCustomers}");
    }
    else
    {
        Debug.LogError("Spawner: Could not find a valid NavMesh near the spawn point! Is the Entrance on the blue walkable area?");
    }
}

    public void OnCustomerLeft()
    {
        currentCustomerCount--;
        // Safety clamp to ensure we never drop below 0
        currentCustomerCount = Mathf.Max(0, currentCustomerCount); 
    }
}