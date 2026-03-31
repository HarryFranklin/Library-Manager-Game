using UnityEngine;

public class CheckoutDesk : Placeable
{
    [Header("Staff Interaction")]
    [Tooltip("Where the librarian/staff member stands to work.")]
    public Transform staffNode;

    // We can track if a staff member is currently working here
    public bool HasStaff { get; private set; } = false;

    public void SetStaffPresence(bool isPresent)
    {
        HasStaff = isPresent;
    }

    public void ProcessPayment()
    {
        Debug.Log($"{gameObject.name}: Payment processed! Money increased.");
    }

    protected override void OnDrawGizmos()
    {
        // 1. Draw the base placeable gizmos (yellow pivot, green bounds, and ALL interaction nodes)
        base.OnDrawGizmos();

#if UNITY_EDITOR
        // 2. Draw the specific staff node
        if (IsSelectedRecursive(transform))
        {
            if (staffNode != null)
            {
                Gizmos.color = Color.cyan; // Cyan for Staff
                Gizmos.DrawWireSphere(staffNode.position, 0.2f);
                Gizmos.DrawLine(transform.position, staffNode.position);
            }
        }
#endif
    }
}