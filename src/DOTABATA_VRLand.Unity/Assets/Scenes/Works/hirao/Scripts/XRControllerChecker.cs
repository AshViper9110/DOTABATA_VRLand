using UnityEngine;

public class XRControllerChecker : MonoBehaviour
{
    [SerializeField]
    private MonoBehaviour trackedPoseDriver;

    private void OnApplicationFocus(bool hasFocus)
    {
        Debug.Log($"Focus : {hasFocus}");

        if (trackedPoseDriver != null)
        {
            trackedPoseDriver.enabled = hasFocus;
        }
    }
}