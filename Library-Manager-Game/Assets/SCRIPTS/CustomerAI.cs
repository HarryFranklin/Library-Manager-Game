using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class CustomerAI : MonoBehaviour
{
    public enum CustomerState
    {
        Entering,
        WalkingToShelf,
        Browsing,
        WalkingToDesk,
        Paying,
        Leaving
    }

    [Header("State")]
    public CustomerState currentState = CustomerState.Entering;

    [Header("Timers")]
    public float browseTime = 3f;
    public float payTime = 2f;

    // References
    private NavMeshAgent agent;
    private BookContainer targetShelf;
    private CheckoutDesk targetDesk;
    private CustomerSpawner mySpawner;
    private Transform exitNode; // Where they go to leave

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        // For this vertical slice, the customer automatically finds targets on spawn.
        // Later, a 'CustomerManager' should assign these to avoid expensive Find() calls.
        FindTargets();
        
        if (targetShelf != null)
        {
            ChangeState(CustomerState.WalkingToShelf);
        }
        else
        {
            Debug.LogWarning("Customer spawned but no shelves found! Leaving.");
            ChangeState(CustomerState.Leaving);
        }
    }

    private void Update()
    {
        // The State Machine Logic
        switch (currentState)
        {
            case CustomerState.WalkingToShelf:
                if (HasReachedDestination())
                {
                    ChangeState(CustomerState.Browsing);
                }
                break;

            case CustomerState.WalkingToDesk:
                if (HasReachedDestination())
                {
                    ChangeState(CustomerState.Paying);
                }
                break;

            case CustomerState.Leaving:
                if (HasReachedDestination())
                {
                    Destroy(gameObject); // Customer leaves the game
                }
                break;
        }
    }

    private void ChangeState(CustomerState newState)
    {
        currentState = newState;

        // Trigger one-time actions when entering a new state
        switch (currentState)
        {
            case CustomerState.WalkingToShelf:
                Transform node = targetShelf.GetAvailableNode(transform.position);
                agent.SetDestination(node != null ? node.position : targetShelf.transform.position);
                break;

            case CustomerState.Browsing:
                StartCoroutine(BrowseRoutine());
                break;

            case CustomerState.WalkingToDesk:
                agent.SetDestination(targetDesk.customerNode.position);
                break;

            case CustomerState.Paying:
                StartCoroutine(PayRoutine());
                break;

            case CustomerState.Leaving:
                if (HasReachedDestination())
                {
                    // Tell the spawner we are leaving to free up a slot in the population cap
                    if (mySpawner != null)
                    {
                        mySpawner.OnCustomerLeft();
                    }
                    
                    Destroy(gameObject); 
                }
                break;
                    }
                }

    // --- Coroutines for timed actions ---

    private IEnumerator BrowseRoutine()
    {
        yield return new WaitForSeconds(browseTime);
        
        if (targetShelf.TryTakeBook())
        {
            // Book acquired, head to the till
            ChangeState(CustomerState.WalkingToDesk);
        }
        else
        {
            // Shelf was empty, leave disappointed
            ChangeState(CustomerState.Leaving);
        }
    }

    private IEnumerator PayRoutine()
    {
        yield return new WaitForSeconds(payTime);
        targetDesk.ProcessPayment();
        ChangeState(CustomerState.Leaving);
    }

    // --- Helper Methods ---

    private bool HasReachedDestination()
    {
        // Check if the agent is actively calculating a path
        if (agent.pathPending) return false;
        
        // Check if they are within the stopping distance
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            // Ensure they aren't just paused/slowed down
            if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
            {
                return true;
            }
        }
        return false;
    }

    private void FindTargets()
    {
        // Prototype logic: Just grab the first ones it finds in the scene
        targetShelf = FindFirstObjectByType<BookContainer>();
        targetDesk = FindFirstObjectByType<CheckoutDesk>();
        
        // Look for an object named "Entrance" to use as the exit node
        GameObject entrance = GameObject.Find("Entrance");
        if (entrance != null) exitNode = entrance.transform;
    }

    public void SetSpawner(CustomerSpawner spawner)
    {
        mySpawner = spawner;
    }
}