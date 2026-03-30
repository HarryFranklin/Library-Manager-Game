using UnityEngine;
using System.Collections.Generic;

public class BookContainer : Placeable
{
    [Header("Inventory Settings")]
    public int maxBooks = 10;
    public int currentBooks = 10; 

    [Header("AI Interaction")]
    [Tooltip("Add multiple empty GameObjects here for a larger shelf.")]
    public List<Transform> interactionNodes = new List<Transform>();

    public bool TryTakeBook()
    {
        if (currentBooks > 0)
        {
            currentBooks--;
            Debug.Log($"{gameObject.name}: Book taken. {currentBooks} remaining.");
            return true;
        }
        return false;
    }

    public Transform GetAvailableNode(Vector3 searcherPosition)
    {
        if (interactionNodes.Count == 0) return null;

        // For now, we just return the first one. 
        // Later, we can add logic to check if another AI is already standing there.
        return interactionNodes[0]; 
    }

    // Use 'override' to tap into the parent's Gizmo logic
    protected override void OnDrawGizmos()
    {
        // 1. Draw the yellow zeroing point and green box from Placeable
        base.OnDrawGizmos();

        // 2. Draw our specific blue interaction nodes
#if UNITY_EDITOR
        if (IsSelectedRecursive(transform))
        {
            Gizmos.color = Color.blue;
            foreach (Transform node in interactionNodes)
            {
                if (node != null)
                {
                    Gizmos.DrawWireSphere(node.position, 0.2f);
                    Gizmos.DrawLine(transform.position, node.position);
                }
            }
        }
#endif
    }
}