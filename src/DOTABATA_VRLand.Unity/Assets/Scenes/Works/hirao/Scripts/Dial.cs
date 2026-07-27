using UnityEngine;
using UnityEngine.Events;
using Valve.VR.InteractionSystem;

[RequireComponent(typeof(Interactable))]
public class Dial : MonoBehaviour
{
    public enum Axis
    {
        X,
        Y,
        Z
    }

    [Header("Reference")]
    [SerializeField] private Transform pivot;

    [Header("Rotation")]
    [SerializeField] private Axis axis = Axis.Y;
    [SerializeField] private bool rotateObject = true;

    [Header("Smoothing")]
    [SerializeField] private float smoothTime = 0.03f;
    [SerializeField] private float deadZone = 0.25f;

    [Header("Limit")]
    [SerializeField] private bool limited = false;
    [SerializeField] private float minAngle = -90;
    [SerializeField] private float maxAngle = 90;

    [Header("Event")]
    public UnityEvent<float> OnAngleChanged;

    public float Angle => angle;

    private Interactable interactable;

    private Hand currentHand;
    private GrabTypes grabbedType = GrabTypes.None;

    private Quaternion startRotation;

    private float angle;
    private float targetAngle;
    private float velocity;
    private float grabOffset;

    private void Awake()
    {
        interactable = GetComponent<Interactable>();

        if (pivot == null)
            pivot = transform;
    }

    private void Start()
    {
        startRotation = pivot.localRotation;
        angle = 0;
        targetAngle = 0;
    }

    //------------------------------------------------------
    // SteamVR
    //------------------------------------------------------
    private void HandHoverBegin(Hand hand)
    {
        hand.ShowGrabHint();
    }

    private void HandHoverEnd(Hand hand)
    {
        hand.HideGrabHint();
    }

    private void HandHoverUpdate(Hand hand)
    {
        GrabTypes startingGrab = hand.GetGrabStarting();

        bool grabEnded =
            grabbedType != GrabTypes.None &&
            !hand.IsGrabbingWithType(grabbedType);

        // Grab開始
        if (grabbedType == GrabTypes.None &&
            startingGrab != GrabTypes.None)
        {
            grabbedType = startingGrab;
            currentHand = hand;

            float handAngle = GetHandAngle(hand);

            grabOffset = angle - handAngle;
            targetAngle = angle;

            hand.HoverLock(interactable);
            hand.HideGrabHint();
        }
        // Grab終了
        else if (grabEnded)
        {
            hand.HoverUnlock(interactable);

            grabbedType = GrabTypes.None;
            currentHand = null;

            hand.ShowGrabHint();
        }
    }

    //------------------------------------------------------
    // Update
    //------------------------------------------------------
    private void Update()
    {
        if (currentHand != null)
        {
            UpdateTargetAngle();
        }

        angle = Mathf.SmoothDampAngle(
            angle,
            targetAngle,
            ref velocity,
            smoothTime);

        ApplyRotation();
    }

    //------------------------------------------------------
    // 手の位置から目標角度を計算
    //------------------------------------------------------
    private void UpdateTargetAngle()
    {
        float handAngle = GetHandAngle(currentHand);

        float newTarget = handAngle + grabOffset;

        if (limited)
            newTarget = Mathf.Clamp(newTarget, minAngle, maxAngle);

        if (Mathf.Abs(Mathf.DeltaAngle(targetAngle, newTarget)) > deadZone)
        {
            targetAngle = newTarget;
            OnAngleChanged?.Invoke(targetAngle);
        }
    }

    //------------------------------------------------------
    // 回転適用
    //------------------------------------------------------
    private void ApplyRotation()
    {
        if (!rotateObject)
            return;

        Quaternion rot;

        switch (axis)
        {
            case Axis.X:
                rot = Quaternion.AngleAxis(angle, Vector3.right);
                break;

            case Axis.Y:
                rot = Quaternion.AngleAxis(angle, Vector3.up);
                break;

            default:
                rot = Quaternion.AngleAxis(angle, Vector3.forward);
                break;
        }

        pivot.localRotation = startRotation * rot;
    }

    //------------------------------------------------------
    // 手の角度取得
    //------------------------------------------------------
    private float GetHandAngle(Hand hand)
    {
        Vector3 local =
            transform.InverseTransformPoint(hand.hoverSphereTransform.position);

        switch (axis)
        {
            case Axis.X:
                return Mathf.Atan2(local.y, local.z) * Mathf.Rad2Deg;

            case Axis.Y:
                return Mathf.Atan2(local.x, local.z) * Mathf.Rad2Deg;

            case Axis.Z:
                return Mathf.Atan2(local.x, local.y) * Mathf.Rad2Deg;
        }

        return 0f;
    }
}