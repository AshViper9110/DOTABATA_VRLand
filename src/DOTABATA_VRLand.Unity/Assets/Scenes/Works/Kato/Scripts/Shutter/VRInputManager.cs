using UnityEngine;
using Valve.VR;

public class VRInputManager : MonoBehaviour
{
    public SteamVR_Action_Boolean grabAction;
    public SteamVR_Input_Sources handType;

    public Transform controller;

    Vector3 startPos;
    bool isDragging = false;

    public ShutterGameManager shuttergameManager;

    void Update()
    {
        if (!shuttergameManager.canInput)
            return;

        // Aボタン押した瞬間
        if (grabAction.GetStateDown(handType))
        {
            Debug.Log("A押した！");

            startPos = controller.position;
            isDragging = true;
        }

        // Aボタン離した瞬間
        if (grabAction.GetStateUp(handType) && isDragging)
        {
            Vector3 endPos = controller.position;
            Vector3 dir = (endPos - startPos).normalized;

            Shutter.Direction inputDir = GetDirection(dir);

            Shutter current = shuttergameManager.GetCurrentShutter();

            if (current != null)
            {
                current.TryOpen(inputDir);
            }

            isDragging = false;
        }
    }

    Shutter.Direction GetDirection(Vector3 dir)
    {
        if (Vector3.Dot(dir, Vector3.up) > 0.7f)
            return Shutter.Direction.Up;

        if (Vector3.Dot(dir, Vector3.down) > 0.7f)
            return Shutter.Direction.Down;

        if (Vector3.Dot(dir, Vector3.right) > 0.7f)
            return Shutter.Direction.Right;

        return Shutter.Direction.Left;
    }
}