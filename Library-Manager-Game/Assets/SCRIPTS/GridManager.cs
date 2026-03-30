using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Settings")]
    public float cellSize = 1f;
    public LayerMask groundLayer;

    // We store the current "active" floor's bounds
    private Bounds currentFloorBounds;
    private bool hasFloor = false;

    public void UpdateGridBounds(GameObject floorObject)
    {
        if (floorObject.TryGetComponent<Collider>(out Collider col))
        {
            currentFloorBounds = col.bounds;
            hasFloor = true;
        }
    }

    public Vector3Int GetGridPosition(Vector3 worldPosition)
    {
        if (!hasFloor) return Vector3Int.zero;

        // Calculate position relative to the "bottom-left" (min) of the floor's bounds
        Vector3 localPos = worldPosition - currentFloorBounds.min;
        
        int x = Mathf.FloorToInt(localPos.x / cellSize);
        int z = Mathf.FloorToInt(localPos.z / cellSize);

        return new Vector3Int(x, 0, z);
    }

    public Vector3 GetWorldPosition(Vector3Int gridPosition)
    {
        if (!hasFloor) return Vector3.zero;

        float x = gridPosition.x * cellSize;
        float z = gridPosition.z * cellSize;
        
        Vector3 snappedPos = currentFloorBounds.min + new Vector3(x, 0, z);
        snappedPos.y = currentFloorBounds.max.y; 
        
        return snappedPos;
    }

    private void OnDrawGizmos()
    {
        if (!hasFloor) return;

        Gizmos.color = Color.cyan;
        
        int columns = Mathf.FloorToInt(currentFloorBounds.size.x / cellSize);
        int rows = Mathf.FloorToInt(currentFloorBounds.size.z / cellSize);

        for (int x = 0; x < columns; x++)
        {
            for (int z = 0; z < rows; z++)
            {
                // 1. Get the Corner Position (where the object actually snaps)
                Vector3 cornerPos = GetWorldPosition(new Vector3Int(x, 0, z));
                
                // 2. To draw the box correctly, we must offset the GIZMO center, 
                // not the actual logical position.
                Vector3 gizmoCenter = cornerPos + new Vector3(cellSize / 2f, 0, cellSize / 2f);
                
                // 3. Draw the wireframe relative to the offset center
                Gizmos.DrawWireCube(gizmoCenter, new Vector3(cellSize, 0.01f, cellSize));
            }
        }
    }
}