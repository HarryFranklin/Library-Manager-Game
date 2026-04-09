using UnityEngine;

public class Librarian : StaffAI
{
    public enum LibrarianState { Idle, WalkingToDesk, Working }
    
    [Header("State")]
    public LibrarianState currentState = LibrarianState.Idle;

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
                    // Snap them perfectly into position and rotation behind the desk
                    transform.position = assignedDesk.staffNode.position;
                    transform.rotation = assignedDesk.staffNode.rotation;
                    
                    // Tell the desk to start processing the customer queue
                    assignedDesk.isManned = true;
                }
                break;
        }
    }
}