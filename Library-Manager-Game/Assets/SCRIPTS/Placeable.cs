using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Placeable : MonoBehaviour
{
    public string itemName;
    
    [Header("3D Dimensions")]
    public int width = 1;  // X
    public int depth = 1;  // Z
    public int height = 1; // Y

    public Vector2Int GetFootprint() => new Vector2Int(width, depth);

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        // Check if this object OR any child of this object is currently selected
        if (IsSelectedRecursive(transform))
        {
            DrawPlacementGizmos();
        }
#endif
    }

    private void DrawPlacementGizmos()
    {
        // 1. Draw the Pivot Point (The "Anchor")
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 0.1f);
        
        // 2. Draw the 3D Occupancy Volume
        // Pivot is bottom-left-front, so center is half of each dimension
        Vector3 center = new Vector3(width / 2f, height / 2f, depth / 2f);
        Vector3 boxSize = new Vector3(width, height, depth);
        
        Gizmos.matrix = transform.localToWorldMatrix;
        
        Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
        Gizmos.DrawCube(center, boxSize);
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(center, boxSize);
    }

#if UNITY_EDITOR
    private bool IsSelectedRecursive(Transform target)
    {
        // Check if the current object is selected
        if (Selection.activeGameObject == target.gameObject) return true;

        // Check if any child of this object is the active selection
        foreach (Transform child in target)
        {
            if (IsSelectedRecursive(child)) return true;
        }

        return false;
    }
#endif

    // OnValidate runs whenever you change a value in the Inspector
    private void OnValidate()
    {
        BoxCollider box = GetComponent<BoxCollider>();
        if (box != null)
        {
            // Automatically align the collider to our grid dimensions
            box.size = new Vector3(width, height, depth);
            box.center = new Vector3(width / 2f, height / 2f, depth / 2f);
        }
    }
}