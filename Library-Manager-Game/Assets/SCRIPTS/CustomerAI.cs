using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(NavMeshAgent))]
public class CustomerAI : MonoBehaviour
{
    public enum CustomerState
    {
        Entering,
        WalkingToShelf,
        Browsing,
        WalkingToDesk,
        Queuing,
        Paying,
        Leaving
    }

    [Header("FX References")]
    public GameObject floatingTextPrefab;
    public Transform headTransform;

    [Header("State")]
    public CustomerState currentState = CustomerState.Entering;

    [Header("Timers")]
    public float browseTime = 3f;
    public float payTime = 2f;

    [Header("Patience Settings")]
    public float maxWaitTime = 20f;
    private float currentWaitTimer = 0f;

    private BookContainer targetShelf;
    private CheckoutDesk targetDesk;
    private Transform exitNode; 
    private CustomerSpawner mySpawner;
    private NavMeshAgent agent;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        GameObject exitObj = GameObject.FindGameObjectWithTag("Exit");
        if (exitObj != null) exitNode = exitObj.transform;

        FindSmartTargets();
        
        if (targetShelf != null && targetDesk != null)
        {
            ChangeState(CustomerState.WalkingToShelf);
        }
        else
        {
            Debug.LogWarning("Missing shelf or checkout desk. Leaving.");
            ChangeState(CustomerState.Leaving);
        }
    }

    private void Update()
    {
        switch (currentState)
        {
            case CustomerState.WalkingToShelf:
                if (HasReachedDestination()) ChangeState(CustomerState.Browsing);
                break;

            case CustomerState.WalkingToDesk:
            if (HasReachedDestination()) ChangeState(CustomerState.Queuing);
            break;

            case CustomerState.Queuing:
                HandleQueuing();
                break;
                
            case CustomerState.Paying:
                break;
        }
    }

    private void HandleQueuing()
    {
        // 1. Check if we are first in line AND the desk is ready
        // Note: For now, we'll assume it's always ready until we add StaffAI
        if (targetDesk.IsAtFront(this))
        {
            ChangeState(CustomerState.Paying);
            return;
        }

        // 2. Future Logic: Patience/Anger
        currentWaitTimer += Time.deltaTime;
        if (currentWaitTimer >= maxWaitTime)
        {
            Debug.Log("Customer lost patience and left!");
            ChangeState(CustomerState.Leaving);
        }
    }

    private void ChangeState(CustomerState newState)
    {
        currentState = newState;

        switch (currentState)
        {
            case CustomerState.WalkingToShelf:
                Transform shelfNode = targetShelf.ReserveSlot();
                agent.SetDestination(shelfNode != null ? shelfNode.position : targetShelf.transform.position);
                break;

            case CustomerState.Browsing:
                StartCoroutine(BrowseRoutine());
                break;

           case CustomerState.WalkingToDesk:
            targetShelf.ReleaseSlot();
            // Ask the desk where to stand
            agent.SetDestination(targetDesk.JoinQueue(this));
            break;

            case CustomerState.Paying:
                StartCoroutine(PayRoutine());
                break;

            case CustomerState.Leaving:
                if(targetDesk != null) targetDesk.LeaveQueue(this);
                if (exitNode != null) agent.SetDestination(exitNode.position);
                break;
        }
    }

    private IEnumerator BrowseRoutine()
    {
        yield return new WaitForSeconds(browseTime);
        
        if (targetShelf.TryTakeBook())
        {
            ChangeState(CustomerState.WalkingToDesk);
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

        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.AddMoney(1);
        }

        // Trigger the popup via the Manager
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
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f) return true;
        }
        return false;
    }

    public void SetSpawner(CustomerSpawner spawner)
    {
        mySpawner = spawner;
    }

    private void FindSmartTargets()
    {
        // 1. Tag-based lookup for Bookshelves
        GameObject[] shelfObjects = GameObject.FindGameObjectsWithTag("BookContainer");
        List<BookContainer> availableShelves = new List<BookContainer>();

        foreach (GameObject obj in shelfObjects)
        {
            // TryGetComponent is heavily optimised and prevents null reference exceptions
            if (obj.TryGetComponent(out BookContainer shelf))
            {
                if (shelf.HasAvailableSlot())
                {
                    availableShelves.Add(shelf);
                }
            }
        }

        if (availableShelves.Count > 0)
        {
            int randomIndex = Random.Range(0, availableShelves.Count);
            targetShelf = availableShelves[randomIndex];
        }

        // 2. Tag-based lookup for the Checkout Desk
        GameObject deskObj = GameObject.FindGameObjectWithTag("CheckoutDesk");
        if (deskObj != null)
        {
            targetDesk = deskObj.GetComponent<CheckoutDesk>();
        }
    }
}