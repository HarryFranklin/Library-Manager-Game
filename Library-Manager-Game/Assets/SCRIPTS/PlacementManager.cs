using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class PlacementManager : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;
    [SerializeField] private LayerMask groundLayer;

    private bool isBuildModeActive = false;
    private Placeable currentPendingPrefab;
    private GameObject visualPreview;
    
    // The source of truth for every occupied square
    private Dictionary<Vector3Int, GameObject> occupiedCells = new Dictionary<Vector3Int, GameObject>();

    private void Update()
    {
        // Exit if not in build mode or no object selected
        if (!isBuildModeActive || currentPendingPrefab == null) 
        {
            if (visualPreview != null) visualPreview.SetActive(false);
            return;
        }

        // Block placement logic if the mouse is over UI buttons
        if (EventSystem.current.IsPointerOverGameObject()) 
        {
            if (visualPreview != null) visualPreview.SetActive(false);
            return;
        }

        HandlePlacementLogic();
    }

    private void HandlePlacementLogic()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
        {
            gridManager.UpdateGridBounds(hit.collider.gameObject);
            
            // 1. Get the discrete coordinate
            Vector3Int gridPos = gridManager.GetGridPosition(hit.point);
            
            // 2. Get the physical world position
            Vector3 snappedPos = gridManager.GetWorldPosition(gridPos);

            if (visualPreview != null)
            {
                visualPreview.SetActive(true);
                visualPreview.transform.position = snappedPos;
            }

            // 3. Ensure we use the exact footprint from the Placeable data
            Vector2Int footprint = new Vector2Int(currentPendingPrefab.width, currentPendingPrefab.depth);

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (IsAreaClear(gridPos, footprint))
                {
                    PlaceObject(gridPos, snappedPos);
                }
                else
                {
                    // Provide feedback so you know the code is actually blocking it
                    Debug.LogWarning($"Area {gridPos} is blocked!");
                }
            }
        }
    }

    private bool IsAreaClear(Vector3Int startGridPos, Vector2Int size)
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int z = 0; z < size.y; z++)
            {
                Vector3Int cellToCheck = startGridPos + new Vector3Int(x, 0, z);
                if (occupiedCells.ContainsKey(cellToCheck))
                {
                    return false; 
                }
            }
        }
        return true;
    }

    private void PlaceObject(Vector3Int startGridPos, Vector3 worldPos)
{
    GameObject newObj = Instantiate(currentPendingPrefab.gameObject, worldPos, Quaternion.identity);

    // Register every coordinate in the footprint
    for (int x = 0; x < currentPendingPrefab.width; x++)
    {
        for (int z = 0; z < currentPendingPrefab.depth; z++)
        {
            // We use Y=0 for the dictionary key to keep it 2D-spatial, 
            // unless you plan on stacking objects vertically later.
            Vector3Int cellToAdd = startGridPos + new Vector3Int(x, 0, z);
            
            if (!occupiedCells.ContainsKey(cellToAdd))
            {
                occupiedCells.Add(cellToAdd, newObj);
            }
        }
    }
}

    public void SelectObjectToBuild(Placeable prefab)
    {
        if (visualPreview != null) Destroy(visualPreview);
        
        currentPendingPrefab = prefab;
        visualPreview = Instantiate(prefab.gameObject);
        
        // Disable preview colliders so they don't block the Raycast
        foreach (var col in visualPreview.GetComponentsInChildren<Collider>()) 
        {
            col.enabled = false;
        }
    }

    public void ToggleBuildMode()
    {
        isBuildModeActive = !isBuildModeActive;
        if (!isBuildModeActive && visualPreview != null) 
        {
            visualPreview.SetActive(false);
        }
        Debug.Log("Build Mode Active: " + isBuildModeActive);
    }
}