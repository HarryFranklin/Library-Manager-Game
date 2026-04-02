using UnityEngine;
using UnityEngine.InputSystem;

public class PlacementSystem : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;
    [SerializeField] private GameObject previewObject; // The transparent ghost
    [SerializeField] private GameObject finalObjectPrefab; // The actual bookshelf
    [SerializeField] private LayerMask groundLayer;

    private void Update()
    {
        HandlePreview();
        
        // Check for Left Click using New Input System polling
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            PlaceObject();
        }
    }

    private void HandlePreview()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
        {
            Vector3Int gridPos = gridManager.GetGridPosition(hit.point);
            previewObject.transform.position = gridManager.GetWorldPosition(gridPos);
        }
    }

    private void PlaceObject()
    {
        // Instantiate the real object at the same position as the ghost
        Instantiate(finalObjectPrefab, previewObject.transform.position, Quaternion.identity);
        
        // NOTE: In a full game, we would store this in a Dictionary<Vector3Int, GameObject> to track "occupied" cells
    }
}