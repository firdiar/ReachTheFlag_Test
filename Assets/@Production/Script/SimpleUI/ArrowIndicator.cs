using UnityEngine;
using UnityEngine.UI;

public class ArrowIndicator : MonoBehaviour
{
    public float offset;
    public Transform target;  // Assign your target object in the inspector
    private Camera mainCamera;
    private RectTransform arrowRectTransform;
    private RectTransform parentRect;
    private Image arrowImage;

    private void Start()
    {
        mainCamera = Camera.main;
        arrowRectTransform = GetComponent<RectTransform>();
        parentRect = transform.parent.GetComponent<RectTransform>();
        arrowImage = GetComponent<Image>();
    }

    void Update()
    {
        PointToTarget();
    }

    void PointToTarget()
    {
        var sizeArea = parentRect.rect.size;
        
        Vector2 toPosition = mainCamera.WorldToScreenPoint(target.position);
        Vector2 fromPosition = mainCamera.WorldToScreenPoint(mainCamera.transform.position);
        
        //normalizer for different resolution
        toPosition = new Vector2(toPosition.x * (sizeArea.x / Screen.width), toPosition.y * (sizeArea.y / Screen.height));
        fromPosition = new Vector2(fromPosition.x * (sizeArea.x / Screen.width), fromPosition.y * (sizeArea.y / Screen.height));
        Vector2 direction = (toPosition - fromPosition).normalized;

        // If target is inside the screen
        if (toPosition.x >= 0 && toPosition.x <= sizeArea.x && toPosition.y >= 0 && toPosition.y <= sizeArea.y)
        {
            arrowImage.enabled = false;  // Hide the arrow
            return; // Exit the function
        }

        arrowImage.enabled = true; // Show the arrow
        
        // Calculate angle to rotate
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        arrowRectTransform.localEulerAngles = new Vector3(0, 0, angle - 90);  // Subtracting 90 degrees to account for default UI orientation

        // Clamp the position of the arrow to screen boundaries
        Vector3 clampedPosition = toPosition;        
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, offset, sizeArea.x - offset);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, offset, sizeArea.y - offset);
        
        arrowRectTransform.anchoredPosition = clampedPosition;
    }
}
