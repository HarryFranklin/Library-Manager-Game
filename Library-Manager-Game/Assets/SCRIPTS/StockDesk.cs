using UnityEngine;

public class StockDesk : Placeable
{
    [Header("Restock Hub")]
    [Tooltip("Where the Restocker stands to gather new books.")]
    public Transform staffNode;

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
#if UNITY_EDITOR
        if (IsSelectedRecursive(transform))
        {
            if (staffNode != null)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(staffNode.position, 0.2f);
                Gizmos.DrawLine(transform.position, staffNode.position);
                
                // Draw a small ray to show rotation/facing direction
                Gizmos.color = Color.red;
                Gizmos.DrawRay(staffNode.position, staffNode.forward * 0.5f);
            }
        }
#endif
    }
}