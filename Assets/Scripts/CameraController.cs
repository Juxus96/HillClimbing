using UnityEngine;

/// <summary>
/// Handles Game Cameras
/// </summary>
public class CameraController : MonoBehaviour
{
    public Camera[] cameras;
    private int currentCam = 0;

    private void Start()
    {
        // Only enables the first camera
        cameras[0].enabled = true;
        for (int i = 1; i < cameras.Length; i++)
            cameras[i].enabled = false;
    }

    public void SwitchCamera()
    {
        // Goes to the next camera after disabling the previous one
        cameras[currentCam].enabled = false;
        if (++currentCam >= cameras.Length)
            currentCam = 0;
        cameras[currentCam].enabled = true;
    }
}
