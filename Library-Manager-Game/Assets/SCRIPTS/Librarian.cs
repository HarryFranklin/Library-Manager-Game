using UnityEngine;

public class Librarian : StaffAI
{
    public enum LibrarianState { Idle, WalkingToDesk, Working }
    
    [Header("State")]
    public LibrarianState currentState = LibrarianState.Idle;

    [Header("Librarian Stats")]
    public float baseCheckoutTime = 2.0f; // Default time at Level 1

    private CheckoutDesk assignedDesk;

    private void Start()
    {
        FindWorkstation();
    }

    private void Update()
    {
        switch (currentState)
        {
            case LibrarianState.WalkingToDesk:
                if (HasReachedDestination())
                {
                    ChangeState(LibrarianState.Working);
                }
                break;
                
            case LibrarianState.Working:
                // They will stand here processing customers until told otherwise
                break;
        }
    }

    private void FindWorkstation()
    {
        GameObject[] desks = GameObject.FindGameObjectsWithTag("CheckoutDesk");
        
        foreach (GameObject deskObj in desks)
        {
            if (deskObj.TryGetComponent(out CheckoutDesk desk))
            {
                // Find a desk that doesn't already have staff assigned to it
                if (!desk.hasStaffAssigned)
                {
                    assignedDesk = desk;
                    assignedDesk.AssignStaff(); // Claim it immediately so no one else takes it
                    ChangeState(LibrarianState.WalkingToDesk);
                    return; 
                }
            }
        }
        
        Debug.Log("Librarian: No free desks available. Staying idle.");
    }

    private void ChangeState(LibrarianState newState)
    {
        // 1. Clean up the old state before transitioning
        if (currentState == LibrarianState.Working && assignedDesk != null)
        {
            assignedDesk.RemoveStaff();
        }

        currentState = newState;

        // 2. Set up the new state
        switch (currentState)
        {
            case LibrarianState.WalkingToDesk:
                if (assignedDesk != null && assignedDesk.staffNode != null)
                {
                    MoveTo(assignedDesk.staffNode.position);
                }
                break;

            case LibrarianState.Working:
                if (assignedDesk != null)
                {
                    transform.position = assignedDesk.staffNode.position;
                    transform.rotation = assignedDesk.staffNode.rotation;
                    assignedDesk.isManned = true;
                    
                    // Tell the desk exactly WHO is manning it
                    assignedDesk.activeLibrarian = this; 
                }
                break;
        }
    }

    // Calculates the speed: Reduces time by 10% (0.1f) per level above 1.
    public float GetCheckoutSpeed()
    {
        // Level 1 = 0% reduction, Level 2 = 10% reduction, Level 5 = 40% reduction
        float reductionMultiplier = 1f - ((currentLevel - 1) * 0.10f);
        
        // Clamp it so they never process instantly (minimum 0.5 seconds)
        return Mathf.Max(0.5f, baseCheckoutTime * reductionMultiplier);
    }
}