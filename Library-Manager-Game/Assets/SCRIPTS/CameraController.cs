using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float panSpeed = 20f;
    public float panBorderThickness = 10f; 
    
    [Header("Zoom Settings")]
    public float scrollSpeed = 2f; 
    public float minY = 5f;  
    public float maxY = 30f; 

    [Header("Bounds Settings")]
    // Manually set these to match your floor size
    public Vector2 minBounds = new Vector2(-10, -10);
    public Vector2 maxBounds = new Vector2(10, 10);

    private void Update()
    {
        HandlePan();
        HandleZoom();
    }

    private void HandlePan()
    {
        Vector3 pos = transform.position;
        if (Keyboard.current == null || Mouse.current == null) return;

        Vector2 mousePosition = Mouse.current.position.ReadValue();

        // Standard movement logic
        if (Keyboard.current.wKey.isPressed || mousePosition.y >= Screen.height - panBorderThickness)
            pos.z += panSpeed * Time.deltaTime;
        if (Keyboard.current.sKey.isPressed || mousePosition.y <= panBorderThickness)
            pos.z -= panSpeed * Time.deltaTime;
        if (Keyboard.current.dKey.isPressed || mousePosition.x >= Screen.width - panBorderThickness)
            pos.x += panSpeed * Time.deltaTime;
        if (Keyboard.current.aKey.isPressed || mousePosition.x <= panBorderThickness)
            pos.x -= panSpeed * Time.deltaTime;

        // INDUSTRY BEST PRACTICE: Clamp the final position before applying it
        // This ensures the camera never leaves the designated rectangle.
        pos.x = Mathf.Clamp(pos.x, minBounds.x, maxBounds.x);
        pos.z = Mathf.Clamp(pos.z, minBounds.y, maxBounds.y);

        transform.position = pos;
    }

    private void HandleZoom()
    {
        if (Mouse.current == null) return;

        float scroll = Mouse.current.scroll.ReadValue().y;
        Vector3 pos = transform.position;

        pos.y -= scroll * scrollSpeed * Time.deltaTime;
        pos.y = Mathf.Clamp(pos.y, minY, maxY);

        transform.position = pos;
    }
}