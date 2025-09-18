// /Assets/Scripts/UIManager.cs

using UnityEngine;
using UnityEngine.UI; // Required for UI elements like Text and Image
using TMPro; // Use TextMeshPro for sharper text

/// <summary>
/// Manages the Heads-Up Display (HUD) for the Laser View.
/// Draws the targeting reticle, entity identification, and bounding boxes.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("HUD Elements")]
    [Tooltip("The targeting reticle image at the center of the screen.")]
    public Image reticle;
    
    [Tooltip("The UI panel that acts as the bounding box.")]
    public RectTransform boundingBox;

    [Tooltip("The text element to display entity information.")]
    public TextMeshProUGUI entityInfoText;

    private Camera laserViewCamera;
    private Entity currentlyTrackedTarget; // The target designated by the AI

    void Start()
    {
        // Find the laser view camera in the scene.
        // A more robust solution might use a direct reference set in the inspector.
        laserViewCamera = FindObjectOfType<CameraManager>().laserViewCamera;

        // Ensure HUD elements are initially hidden
        boundingBox.gameObject.SetActive(false);
        entityInfoText.gameObject.SetActive(false);
    }

    void Update()
    {
        // The HUD logic only runs if the Laser View camera is active.
        if (!laserViewCamera.isActiveAndEnabled)
        {
            // If we switch away, ensure the bounding box is hidden.
            if(boundingBox.gameObject.activeSelf)
            {
                boundingBox.gameObject.SetActive(false);
                entityInfoText.gameObject.SetActive(false);
            }
            return;
        }

        HandleEntityRaycast();
    }

    /// <summary>
    /// Casts a ray from the center of the screen to detect entities.
    /// </summary>
    private void HandleEntityRaycast()
    {
        // Create a ray from the center of the camera's viewport
        Ray ray = laserViewCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        // If the ray hits an object with an Entity component
        if (Physics.Raycast(ray, out hit, 500f) && hit.collider.TryGetComponent<Entity>(out Entity hitEntity))
        {
            // An entity is under the reticle, so draw its info.
            DrawBoundingBox(hitEntity);
        }
        else
        {
            // Nothing is under the reticle, hide the info.
            boundingBox.gameObject.SetActive(false);
            entityInfoText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Draws a 2D bounding box and info text around a detected 3D entity.
    /// </summary>
    /// <param name="entity">The entity to draw the box around.</param>
    private void DrawBoundingBox(Entity entity)
    {
        boundingBox.gameObject.SetActive(true);
        entityInfoText.gameObject.SetActive(true);

        // Determine the color based on entity type
        Color boxColor;
        switch (entity.entityType)
        {
            case EntityType.Enemy_Unit:
                boxColor = Color.red;
                break;
            case EntityType.Friendly_Unit:
                boxColor = Color.green;
                break;
            case EntityType.Civilian_Unit:
                boxColor = Color.white;
                break;
            default:
                boxColor = Color.yellow;
                break;
        }
        
        // Flash the box if this is the AI's designated target
        if (currentlyTrackedTarget != null && currentlyTrackedTarget.entityId == entity.entityId)
        {
            // Simple flash effect by alternating color alpha
            boxColor.a = Mathf.PingPong(Time.time * 2.0f, 0.5f) + 0.5f;
        }

        boundingBox.GetComponent<Image>().color = boxColor;
        entityInfoText.text = entity.entityType.ToString().Replace("_", " ");

        // --- Bounding Box Sizing and Positioning ---
        // This is a simplified approach. A true 3D bounding box requires calculating all 8 corners.
        // For this MVP, we will simply center the box on the object's pivot point.
        Vector3 entityScreenPos = laserViewCamera.WorldToScreenPoint(entity.transform.position);

        // Only draw if the entity is in front of the camera
        if (entityScreenPos.z > 0)
        {
            boundingBox.position = entityScreenPos;
        }
        else // Hide if behind the camera
        {
            boundingBox.gameObject.SetActive(false);
            entityInfoText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Public method called by SimulationManager to set the AI's chosen target.
    /// </summary>
    /// <param name="targetEntity">The entity to highlight.</param>
    public void SetDesignatedTarget(Entity targetEntity)
    {
        currentlyTrackedTarget = targetEntity;
    }
}
