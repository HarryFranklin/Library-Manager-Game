using UnityEngine;
using System.Collections.Generic;

public class CheckoutDesk : Placeable
{
    [Header("Staffing")]
    public Transform staffNode;
    
    // hasStaffAssigned means a librarian is walking towards it.
    // isManned means they have physically arrived and are ready to work.
    public bool hasStaffAssigned = false; 
    public bool isManned = false; 

    public Librarian activeLibrarian;

    [Header("Queue Settings")]
    public List<Transform> queueNodes = new List<Transform>();
    private List<CustomerAI> customersInQueue = new List<CustomerAI>();

    public void ProcessPayment()
    {
        Debug.Log($"{gameObject.name}: Transaction Complete.");
    }

    public void AssignStaff()
    {
        hasStaffAssigned = true;
    }

    public void RemoveStaff()
    {
        hasStaffAssigned = false;
        isManned = false;
        activeLibrarian = null;
    }

    public bool CanJoinQueue()
    {
        return customersInQueue.Count < queueNodes.Count;
    }

    public Vector3 JoinQueue(CustomerAI customer)
    {
        if (!customersInQueue.Contains(customer))
            customersInQueue.Add(customer);
            
        return GetPositionInLine(customer);
    }

    public void LeaveQueue(CustomerAI customer)
    {
        if (customersInQueue.Contains(customer))
        {
            customersInQueue.Remove(customer);
            UpdateQueuePositions();
        }
    }

    private void UpdateQueuePositions()
    {
        for (int i = 0; i < customersInQueue.Count; i++)
        {
            customersInQueue[i].UpdateQueueDestination(GetPositionInLine(customersInQueue[i]));
        }
    }

    public Vector3 GetPositionInLine(CustomerAI customer)
    {
        int index = customersInQueue.IndexOf(customer);
        
        if (index >= 0 && index < queueNodes.Count && queueNodes[index] != null)
        {
            return queueNodes[index].position;
        }

        return transform.position; 
    }

    public bool IsAtFront(CustomerAI customer)
    {
        return isManned && customersInQueue.Count > 0 && customersInQueue[0] == customer;
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
#if UNITY_EDITOR
        if (IsSelectedRecursive(transform))
        {
            if (staffNode != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(staffNode.position, 0.2f);
                Gizmos.DrawLine(transform.position, staffNode.position);
                
                // Draw a small line to indicate which way the staff member will face
                Gizmos.color = Color.red;
                Gizmos.DrawRay(staffNode.position, staffNode.forward * 0.5f);
            }

            Gizmos.color = Color.blue;
            for (int i = 0; i < queueNodes.Count; i++)
            {
                if (queueNodes[i] != null)
                {
                    Gizmos.DrawWireSphere(queueNodes[i].position, 0.3f);
                    if (i > 0 && queueNodes[i - 1] != null)
                    {
                        Gizmos.DrawLine(queueNodes[i].position, queueNodes[i - 1].position);
                    }
                }
            }
        }
#endif
    }
}