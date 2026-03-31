using UnityEngine;
using System.Collections.Generic;

public class CheckoutDesk : Placeable
{
    [Header("Staffing")]
    public Transform staffNode;
    public bool isManned = false;

    [Header("Queue Settings")]
    [Tooltip("The points where customers stand, from front (index 0) to back.")]
    public List<Transform> queueNodes = new List<Transform>();
    
    // Logic: Track who is in line to manage their positions
    private List<CustomerAI> customersInQueue = new List<CustomerAI>();

    public Vector3 JoinQueue(CustomerAI customer)
    {
        if (!customersInQueue.Contains(customer))
        {
            customersInQueue.Add(customer);
        }
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
            // Tell each customer to move to their new updated spot
            customersInQueue[i].UpdateQueueDestination(GetPositionInLine(customersInQueue[i]));
        }
    }

    public Vector3 GetPositionInLine(CustomerAI customer)
    {
        int index = customersInQueue.IndexOf(customer);
        
        // If they are within the number of nodes we placed, give them that spot
        if (index >= 0 && index < queueNodes.Count)
        {
            return queueNodes[index].position;
        }
        
        // If the line is longer than nodes, they stand at the very back
        return queueNodes[queueNodes.Count - 1].position;
    }

    public bool IsAtFront(CustomerAI customer)
    {
        return customersInQueue.Count > 0 && customersInQueue[0] == customer;
    }

    public void ProcessPayment()
    {
        Debug.Log($"{gameObject.name}: Transaction Complete.");
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
#if UNITY_EDITOR
        if (IsSelectedRecursive(transform))
        {
            Gizmos.color = Color.blue;
            foreach (var node in queueNodes)
            {
                if (node != null) Gizmos.DrawWireSphere(node.position, 0.3f);
            }
        }
#endif
    }
}