using UnityEngine;

public class PositionChecker : MonoBehaviour {
    public enum HandType {
        Right,
        Left
    };

    public HandType CheckHandType = HandType.Right;

    private Transform rightHandTransform;
    private Transform leftHandTransform;

    public float Up;
    public float Down;
    public float Right;
    public float Left;
    public float Forward;
    public float Behind;

    private void Start() {
        rightHandTransform = GameObject.Find("RightHand").transform;
        leftHandTransform = GameObject.Find("LeftHand").transform;
    }

    private void Update() {
        switch (CheckHandType) {
            case HandType.Right:
                this.transform.position =
            rightHandTransform.position +
            rightHandTransform.up.normalized * Up + -rightHandTransform.up.normalized * Down +
            rightHandTransform.right.normalized * Right + -rightHandTransform.right.normalized * Left +
            rightHandTransform.forward.normalized * Forward + -rightHandTransform.forward.normalized * Behind;
                break;
            case HandType.Left:
                this.transform.position =
            leftHandTransform.position +
            leftHandTransform.up.normalized * Up + -leftHandTransform.up.normalized * Down +
            leftHandTransform.right.normalized * Right + -leftHandTransform.right.normalized * Left +
            leftHandTransform.forward.normalized * Forward + -leftHandTransform.forward.normalized * Behind;
                break;
        }
    }
}
