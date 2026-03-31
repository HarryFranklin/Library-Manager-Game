using UnityEngine;
using UnityEngine.UI;

public class PopupManager : MonoBehaviour
{
    [Header("Popup Settings")]
    public GameObject popupTextPrefab;
    public Canvas canvas; 

    [Header("Canvas Scale Settings")]
    [Tooltip("0.01 is standard for matching UI pixels to World units")]
    public float canvasScale = 0.01f; 

    [Header("Popup Offset")]
    public Vector3 popupOffset = new Vector3(0, 1.5f, 0); 

    [Header("Default Settings")]
    public float defaultDuration = 1.5f;
    public float defaultMoveDistance = 2f; 
    public float defaultFontSize = 36f; 

    [Header("Preset Colours")]
    public Color gainColor = Color.green;
    public Color lossColor = Color.red;

    private static PopupManager instance;
    public static PopupManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindFirstObjectByType<PopupManager>();
            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            SetupCanvas();
        }
        else { Destroy(gameObject); }
    }

    private void SetupCanvas()
    {
        if (canvas == null)
        {
            canvas = GetComponent<Canvas>();
            if (canvas == null) canvas = gameObject.AddComponent<Canvas>();
        }

        canvas.renderMode = RenderMode.WorldSpace;
        canvas.transform.localScale = Vector3.one * canvasScale;
        canvas.transform.position = Vector3.zero;
        
        // Ensure popups render in front of standard objects
        canvas.sortingOrder = 100; 
    }

    public void ShowGain(int amount, Vector3 worldPosition)
    {
        ShowPopup($"+{amount}", worldPosition, gainColor);
    }

    public void ShowPopup(string text, Vector3 worldPosition, Color color)
    {
        if (popupTextPrefab == null || canvas == null) return;

        Vector3 spawnPos = worldPosition + popupOffset;
        GameObject popupObj = Instantiate(popupTextPrefab, canvas.transform);
        popupObj.transform.position = spawnPos;

        PopupText popup = popupObj.GetComponent<PopupText>();
        if (popup != null)
        {
            popup.Initialise(text, color, defaultFontSize, defaultDuration, defaultMoveDistance);
        }
    }
}