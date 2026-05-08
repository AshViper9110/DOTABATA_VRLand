using UnityEngine;
using UnityEngine.XR.Management;

public class PlayerCon : MonoBehaviour
{
    public float sensitivity = 2f;

    float pitch;

    void Update()
    {
        // VR’†‚Í–³Œø
        bool isVR =
            XRGeneralSettings.Instance.Manager.activeLoader != null;

        if (isVR) return;

        float mx = Input.GetAxis("Mouse X");
        float my = Input.GetAxis("Mouse Y");

        // ¶‰E‰ñ“]
        transform.parent.Rotate(Vector3.up * mx * sensitivity);

        // ã‰º‰ñ“]
        pitch -= my * sensitivity;
        pitch = Mathf.Clamp(pitch, -80f, 80f);

        transform.localEulerAngles = Vector3.right * pitch;
    }
}