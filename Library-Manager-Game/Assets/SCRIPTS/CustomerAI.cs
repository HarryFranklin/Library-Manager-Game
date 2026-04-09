using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(NavMeshAgent))]
public class CustomerAI : MonoBehaviour
{
    public enum CustomerState
    {
        Entering, WalkingToShelf, Browsing, WalkingToDesk, Queuing, Paying, Leaving, WaitingForDesk, WaitingForShelf
    }

    [Header("FX References")]
    public Transform headTransform;

    [Header("State")]
    public CustomerState currentState = CustomerState.Entering;

    [Header("Timers")]
    public float browseTime = 3f;
    public float payTime = 2f;

    [Header("Patience Settings")]
    public float waitBeforeBarSpawns = 5f;
    public float patienceDepletionTime = 10f;
    
    private float currentWaitTimer = 0f;
    private WaitBarUI currentWaitBar;

    private BookContainer targetShelf;
    private CheckoutDesk targetDesk;
    private Transform exitNode; 
    private CustomerSpawner mySpawner;
    private NavMeshAgent agent;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.avoidancePriority = Random.Range(1, 100); 
    }

    private void Start()
    {
        GameObject exitObj = GameObject.FindGameObjectWithTag("Exit");
        if (exitObj != null) exitNode = exitObj.transform;

        FindSmartTargets();
        
        // Only leave if there is literally no checkout desk.
        if (targetDesk == null)
        {
            Debug.LogWarning("Missing checkout desk. Leaving.");
            ChangeState(CustomerState.Leaving);
        }
        else if (targetShelf != null)
        {
            ChangeState(CustomerState.WalkingToShelf);
        }
        else
        {
            // Desk exists, but no shelves are free. Wait at the entrance!
            ChangeState(CustomerState.WaitingForShelf);
        }
    }

    private void Update()
    {
        switch (currentState)
        {
            // Checking for shelves while waiting
            case CustomerState.WaitingForShelf:
                // Only run the search a few times a second to save performance, not every single frame
                if (Time.frameCount % 30 == 0) 
                {
                    FindSmartTargets();
                    if (targetShelf != null)
                    {
                        ChangeState(CustomerState.WalkingToShelf);
                        break;
                    }
                }
                
                // If they don't find a shelf, they get impatient and the bar spawns
                HandlePatienceTimer();
                break;

            case CustomerState.WalkingToShelf:
                if (HasReachedDestination()) ChangeState(CustomerState.Browsing);
                break;

            case CustomerState.WalkingToDesk:
                if (Vector3.Distance(transform.position, agent.destination) < 1.5f || 
                   (agent.remainingDistance < 2.5f && agent.velocity.sqrMagnitude < 0.05f)) 
                {
                    ChangeState(CustomerState.Queuing);
                }
                break;

            case CustomerState.WaitingForDesk:
                if (targetDesk.CanJoinQueue())
                {
                    ChangeState(CustomerState.WalkingToDesk);
                }
                else
                {
                    HandlePatienceTimer(); 
                }
                break;

            case CustomerState.Queuing:
                if (targetDesk.IsAtFront(this) && Vector3.Distance(transform.position, targetDesk.GetPositionInLine(this)) < 1.5f)
                {
                    ChangeState(CustomerState.Paying);
                }
                else
                {
                    HandlePatienceTimer();
                }
                break;
        }
    }

    private void HandlePatienceTimer()
    {
        currentWaitTimer += Time.deltaTime;

        if (currentWaitTimer < waitBeforeBarSpawns) return; 

        if (currentWaitBar == null && PopupManager.Instance != null)
        {
            Transform anchor = headTransform != null ? headTransform : transform;
            currentWaitBar = PopupManager.Instance.CreateWaitBar(anchor);
        }

        if (currentWaitBar != null)
        {
            float timeSinceBarSpawned = currentWaitTimer - waitBeforeBarSpawns;
            float remainingPatience = 1f - (timeSinceBarSpawned / patienceDepletionTime);
            
            currentWaitBar.UpdateProgress(Mathf.Clamp01(remainingPatience));

            if (timeSinceBarSpawned >= patienceDepletionTime)
            {
                Debug.Log("Patience reached zero. Storming out!");
                ChangeState(CustomerState.Leaving);
            }
        }
    }

    private void ChangeState(CustomerState newState)
    {
        currentState = newState;
        
        currentWaitTimer = 0f;
        ClearWaitBar();

        switch (currentState)
        {
            // Step to the side while waiting
            case CustomerState.WaitingForShelf:
                // Pick a random spot within a 2.5m radius to stand in the "lobby"
                Vector3 wanderPos = transform.position + new Vector3(Random.Range(-2.5f, 2.5f), 0, Random.Range(-2.5f, 2.5f));
                
                // Ensure the point is safely on the NavMesh so they don't walk through walls
                if (NavMesh.SamplePosition(wanderPos, out NavMeshHit hit, 3f, NavMesh.AllAreas))
                {
                    agent.SetDestination(hit.position);
                }
                else
                {
                    agent.SetDestination(transform.position); 
                }
                break;

            case CustomerState.WalkingToShelf:
                Transform shelfNode = targetShelf.ReserveSlot();
                if (shelfNode != null)
                {
                    agent.SetDestination(shelfNode.position);
                }
                else
                {
                    // Spot was stolen while calculating! Go back to waiting.
                    ChangeState(CustomerState.WaitingForShelf);
                }
                break;

            case CustomerState.Browsing:
                StartCoroutine(BrowseRoutine());
                break;

            case CustomerState.WalkingToDesk:
                targetShelf.ReleaseSlot();
                agent.SetDestination(targetDesk.JoinQueue(this));
                break;

            case CustomerState.WaitingForDesk:
                targetShelf.ReleaseSlot();
                Vector3 stepAside = transform.position + new Vector3(Random.Range(-1.5f, 1.5f), 0, Random.Range(-1.5f, 1.5f));
                agent.SetDestination(stepAside);
                break;

            case CustomerState.Queuing:
                break;

            case CustomerState.Paying:
                StartCoroutine(PayRoutine());
                break;

            case CustomerState.Leaving:
                if (targetDesk != null) targetDesk.LeaveQueue(this);
                if (exitNode != null) agent.SetDestination(exitNode.position);
                break;
        }
    }

    private IEnumerator BrowseRoutine()
    {
        yield return new WaitForSeconds(browseTime);
        if (targetShelf.TryTakeBook())
        {
            if (targetDesk.CanJoinQueue())
            {
                ChangeState(CustomerState.WalkingToDesk);
            }
            else
            {
                ChangeState(CustomerState.WaitingForDesk);
            }
        }
        else
        {
            targetShelf.ReleaseSlot();
            ChangeState(CustomerState.Leaving);
        }
    }

    private IEnumerator PayRoutine()
    {
        yield return new WaitForSeconds(payTime);
        targetDesk.ProcessPayment();

        if (EconomyManager.Instance != null) EconomyManager.Instance.AddMoney(1);

        if (PopupManager.Instance != null)
        {
            Vector3 spawnPos = headTransform != null ? headTransform.position : transform.position;
            PopupManager.Instance.ShowGain(1, spawnPos);
        }

        ChangeState(CustomerState.Leaving);
    }

    public void UpdateQueueDestination(Vector3 newPoint)
    {
        if (currentState == CustomerState.Queuing || currentState == CustomerState.WalkingToDesk)
        {
            agent.SetDestination(newPoint);
        }
    }

    private bool HasReachedDestination()
    {
        if (agent.pathPending) return false;
        if (!agent.hasPath) return true; 

        if (agent.remainingDistance <= agent.stoppingDistance + 0.5f) return true;

        if (agent.remainingDistance <= 2.5f && agent.velocity.sqrMagnitude < 0.05f) 
        {
            return true;
        }
        
        return false;
    }

    public void SetSpawner(CustomerSpawner spawner)
    {
        mySpawner = spawner;
    }

    private void FindSmartTargets()
    {
        GameObject[] shelfObjects = GameObject.FindGameObjectsWithTag("BookContainer");
        List<BookContainer> availableShelves = new List<BookContainer>();

        foreach (GameObject obj in shelfObjects)
        {
            if (obj.TryGetComponent(out BookContainer shelf))
            {
                if (shelf.HasAvailableSlot()) availableShelves.Add(shelf);
            }
        }

        if (availableShelves.Count > 0)
        {
            int randomIndex = Random.Range(0, availableShelves.Count);
            targetShelf = availableShelves[randomIndex];
        }
        else
        {
            // Explicitly clear it so they know it's full
            targetShelf = null;
        }

        GameObject deskObj = GameObject.FindGameObjectWithTag("CheckoutDesk");
        if (deskObj != null) targetDesk = deskObj.GetComponent<CheckoutDesk>();
    }

    private void ClearWaitBar()
    {
        if (currentWaitBar != null)
        {
            Destroy(currentWaitBar.gameObject);
            currentWaitBar = null;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (currentState == CustomerState.Leaving && other.CompareTag("Exit"))
        {
            ClearWaitBar();
            if (mySpawner != null) mySpawner.OnCustomerLeft();
            Destroy(gameObject);
        }
    }
}