using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ScaleSpriteToCamera : MonoBehaviour
{
    [SerializeField]
    SpriteRenderer spriteRenderer;

    [SerializeField]
    bool triggerScaleUpdate;

    private void OnValidate()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (triggerScaleUpdate)
        {
            triggerScaleUpdate = false;
            ScaleToCamera();
        }
    }

    private void Start()
    {
        ScaleToCamera();
        Destroy(this);
    }

    private void ScaleToCamera()
    {
        float cameraHeight = Camera.main.orthographicSize * 2;
        float cameraWidth = cameraHeight * Camera.main.aspect;

        Vector2 spriteSize = spriteRenderer.sprite.bounds.size;

        Vector2 newScale = transform.localScale;
        newScale.y = cameraHeight / spriteSize.y;
        newScale.x = cameraWidth / spriteSize.x; // Scale to fit full width

        transform.localScale = newScale;
    }
}
