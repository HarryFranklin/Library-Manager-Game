using UnityEngine;
using UnityEngine.UI;

public class WaitBarUI : MonoBehaviour
{
    [Header("Components")]
    [Tooltip("The UI Image whose Image Type is set to 'Filled'")]
    public Image fillImage;
    
    [Tooltip("Optional: Evaluates from 0 (Left) to 1 (Right). Good for shifting from Green to Red.")]
    public Gradient colourGradient;

    private Transform target;
    private Vector3 offset;

    public void Initialise(Transform targetTransform, Vector3 positionOffset)
    {
        target = targetTransform;
        offset = positionOffset;
        UpdateProgress(0f);
    }

    public void UpdateProgress(float progress)
    {
        if (fillImage != null)
        {
            // Clamps between 0 and 1 automatically
            fillImage.fillAmount = progress;
            
            // Apply gradient colour if one is set in the Inspector
            if (colourGradient != null)
            {
                fillImage.color = colourGradient.Evaluate(progress);
            }
        }
    }

    private void LateUpdate()
    {
        // Fail-safe: If the customer deletes themselves, delete this UI element
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // Track the target smoothly after all movement calculations are done
        transform.position = target.position + offset;

        // Billboard to face the camera perfectly
        if (Camera.main != null)
        {
            transform.rotation = Camera.main.transform.rotation;
        }
    }
}