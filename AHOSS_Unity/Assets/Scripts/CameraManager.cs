// /Assets/Scripts/CameraManager.cs

using UnityEngine;

/// <summary>
/// Manages the different camera views in the simulation.
/// Allows switching between a top-down strategic view and a first-person tactical view.
/// </summary>
public class CameraManager : MonoBehaviour
{
    [Header("Camera References")]
    [Tooltip("The top-down orthographic camera for the Metaview.")]
    public Camera metaviewCamera;

    [Tooltip("The first-person camera attached to the AHOSS craft.")]
    public Camera laserViewCamera;

    [Header("UI References")]
    [Tooltip("The main UI canvas for the Heads-Up Display.")]
    public Canvas hudCanvas;

    private bool isMetaviewActive = true;

    void Start()
    {
        // Start in Metaview by default
        EnableMetaview();
    }

    void Update()
    {
        // Listen for the 'V' key to switch views
        if (Input.GetKeyDown(KeyCode.V))
        {
            isMetaviewActive = !isMetaviewActive;
            if (isMetaviewActive)
            {
                EnableMetaview();
            }
            else
            {
                EnableLaserView();
            }
        }
    }

    /// <summary>
    /// Activates the top-down strategic view.
    /// </summary>
    private void EnableMetaview()
    {
        metaviewCamera.enabled = true;
        laserViewCamera.enabled = false;
        // The HUD is only for Laser View
        hudCanvas.gameObject.SetActive(false);
    }

    /// <summary>
    /// Activates the first-person view from the AHOSS craft.
    /// </summary>
    private void EnableLaserView()
    {
        metaviewCamera.enabled = false;
        laserViewCamera.enabled = true;
        // Show the HUD in Laser View
        hudCanvas.gameObject.SetActive(true);
    }
}
