using UnityEngine;

public class CheckoutDesk : Placeable
{
    [Header("AI Interaction Nodes")]
    [Tooltip("Where the librarian/staff member stands to work.")]
    public Transform staffNode;
    
    [Tooltip("Where the student stands to hand over their books.")]
    public Transform customerNode;

    // We can track if a staff member is currently working here
    public bool HasStaff { get; private set; } = false;

    public void SetStaffPresence(bool isPresent)
    {
        HasStaff = isPresent;
    }

    public void ProcessPayment()
    {
        // This will eventually interface with your EconomyManager
        Debug.Log($"{gameObject.name}: Payment processed! Money increased.");
    }

    // Use 'override' to keep the green physical footprint from Placeable
    protected override void OnDrawGizmos()
    {
        // 1. Draw the base placeable gizmos (yellow pivot, green bounds)
        base.OnDrawGizmos();

#if UNITY_EDITOR
        // 2. Draw the specific interaction nodes for the desk
        if (IsSelectedRecursive(transform))
        {
            if (staffNode != null)
            {
                Gizmos.color = Color.cyan; // Cyan for Staff
                Gizmos.DrawWireSphere(staffNode.position, 0.2f);
                Gizmos.DrawLine(transform.position, staffNode.position);
            }

            if (customerNode != null)
            {
                Gizmos.color = Color.magenta; // Magenta for Customer
                Gizmos.DrawWireSphere(customerNode.position, 0.2f);
                Gizmos.DrawLine(transform.position, customerNode.position);
            }
        }
#endif
    }
}