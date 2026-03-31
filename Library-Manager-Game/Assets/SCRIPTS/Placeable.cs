using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Placeable : MonoBehaviour
{
    public string itemName;
    
    [Header("Physical Dimensions (Floats)")]
    public float width = 1f;  
    public float depth = 1f;  
    public float height = 1f; 

    public int GridWidth => Mathf.CeilToInt(width);
    public int GridDepth => Mathf.CeilToInt(depth);
    public Vector2Int GetFootprint() => new Vector2Int(GridWidth, GridDepth);

    [Header("AI Interaction Slots")]
    [Tooltip("Points where AI can stand to interact with this object.")]
    public List<Transform> interactionNodes = new List<Transform>();

    // Tracks how many AI are currently walking to or using this object
    protected int currentUsers = 0; 

    /// <summary>
    /// Base logic: Returns true if there are more slots than current users.
    /// 'virtual' allows child classes to add extra conditions.
    /// </summary>
    public virtual bool HasAvailableSlot()
    {
        return currentUsers < interactionNodes.Count;
    }

    /// <summary>
    /// AI calls this to claim a spot before they start walking.
    /// </summary>
    public virtual Transform ReserveSlot()
    {
        if (HasAvailableSlot())
        {
            Transform assignedNode = interactionNodes[currentUsers];
            currentUsers++;
            return assignedNode;
        }
        return null; 
    }

    /// <summary>
    /// AI calls this when they leave the object.
    /// </summary>
    public virtual void ReleaseSlot()
    {
        currentUsers--;
        currentUsers = Mathf.Max(0, currentUsers); 
    }

    protected virtual void OnDrawGizmos()
    {
#if UNITY_EDITOR
        if (IsSelectedRecursive(transform))
        {
            DrawPlacementGizmos();
        }
#endif
    }

    private void DrawPlacementGizmos()
    {
        // 1. Draw the Pivot
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 0.1f);
        
        // 2. Draw the Footprint Box
        Vector3 center = new Vector3(width / 2f, height / 2f, depth / 2f);
        Vector3 boxSize = new Vector3(width, height, depth);
        
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
        Gizmos.DrawCube(center, boxSize);
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(center, boxSize);

        // Reset matrix so world-space nodes draw correctly
        Gizmos.matrix = Matrix4x4.identity;

        // 3. Draw Interaction Nodes for ALL placeables automatically
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

#if UNITY_EDITOR
    private void OnValidate()
    {
        Vector3 newSize = new Vector3(width, height, depth);
        Vector3 newCenter = new Vector3(width / 2f, height / 2f, depth / 2f);

        BoxCollider box = GetComponent<BoxCollider>();
        if (box != null) { box.size = newSize; box.center = newCenter; }

        UnityEngine.AI.NavMeshObstacle obstacle = GetComponent<UnityEngine.AI.NavMeshObstacle>();
        if (obstacle != null) { obstacle.size = newSize; obstacle.center = newCenter; obstacle.carving = true; }
    }

    protected bool IsSelectedRecursive(Transform target)
    {
        if (Selection.activeGameObject == target.gameObject) return true;
        foreach (Transform child in target) { if (IsSelectedRecursive(child)) return true; }
        return false;
    }
#endif
}