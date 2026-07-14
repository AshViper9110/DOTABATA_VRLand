using UnityEngine;
using Valve.VR.InteractionSystem;


[RequireComponent(typeof(Interactable))]
[RequireComponent(typeof(Rigidbody))]
public class GarageManager : MonoBehaviour
{
    public enum SlideAxis
    {
        X,
        Y,
        Z
    }

    [Header("Slide")]
    public SlideAxis slideAxis = SlideAxis.X;

    public float minPosition = 0f;
    public float maxPosition = 1f;

    Rigidbody rb;

    private Hand currentHand;
    private bool grabbed;

    private Vector3 initialLocalPosition;
    private float grabOffset;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.isKinematic = true;
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        initialLocalPosition = transform.localPosition;
    }

    public void BeginGrab(Hand hand)
    {
        currentHand = hand;
        grabbed = true;

        Vector3 handLocal =
            transform.parent.InverseTransformPoint(hand.transform.position);

        float handValue = GetAxisValue(handLocal);

        float doorValue = GetAxisValue(transform.localPosition);

        grabOffset = handValue - doorValue;
    }

    public void EndGrab()
    {
        grabbed = false;
        currentHand = null;
    }

    void FixedUpdate()
    {
        if (!grabbed || currentHand == null)
            return;

        Vector3 handLocal =
            transform.parent.InverseTransformPoint(currentHand.transform.position);

        float value = GetAxisValue(handLocal);

        value -= grabOffset;

        value = Mathf.Clamp(value, minPosition, maxPosition);

        Vector3 target = transform.localPosition;

        switch (slideAxis)
        {
            case SlideAxis.X:
                target.x = value;
                break;

            case SlideAxis.Y:
                target.y = value;
                break;

            case SlideAxis.Z:
                target.z = value;
                break;
        }

        rb.MovePosition(transform.parent.TransformPoint(target));
    }

    float GetAxisValue(Vector3 v)
    {
        switch (slideAxis)
        {
            case SlideAxis.X:
                return v.x;

            case SlideAxis.Y:
                return v.y;

            default:
                return v.z;
        }
    }


}
