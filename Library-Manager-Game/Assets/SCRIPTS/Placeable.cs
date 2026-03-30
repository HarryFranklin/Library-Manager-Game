using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Placeable : MonoBehaviour
{
    public string itemName;
    
    [Header("Physical Dimensions (Floats)")]
    public float width = 1f;  // X
    public float depth = 1f;  // Z
    public float height = 1f; // Y

    // Separate physical size from grid occupancy.
    // A 1.8f wide object still locks down 2 whole grid cells.
    public int GridWidth => Mathf.CeilToInt(width);
    public int GridDepth => Mathf.CeilToInt(depth);

    public Vector2Int GetFootprint() => new Vector2Int(GridWidth, GridDepth);

    // 'protected virtual' allows child scripts (like BookContainer) to add to this, 
    // rather than destroying it.
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
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 0.1f);
        
        Vector3 center = new Vector3(width / 2f, height / 2f, depth / 2f);
        Vector3 boxSize = new Vector3(width, height, depth);
        
        Gizmos.matrix = transform.localToWorldMatrix;
        
        Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
        Gizmos.DrawCube(center, boxSize);
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(center, boxSize);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        Vector3 newSize = new Vector3(width, height, depth);
        Vector3 newCenter = new Vector3(width / 2f, height / 2f, depth / 2f);

        // Auto-size BoxCollider
        BoxCollider box = GetComponent<BoxCollider>();
        if (box != null)
        {
            box.size = newSize;
            box.center = newCenter;
        }

        // Auto-size NavMeshObstacle
        UnityEngine.AI.NavMeshObstacle obstacle = GetComponent<UnityEngine.AI.NavMeshObstacle>();
        if (obstacle != null)
        {
            obstacle.size = newSize;
            obstacle.center = newCenter;
            
            // Best practice: Ensure it actually carves the mesh dynamically
            obstacle.carving = true; 
        }
    }

    protected bool IsSelectedRecursive(Transform target)
    {
        if (Selection.activeGameObject == target.gameObject) return true;
        foreach (Transform child in target)
        {
            if (IsSelectedRecursive(child)) return true;
        }
        return false;
    }
#endif
}