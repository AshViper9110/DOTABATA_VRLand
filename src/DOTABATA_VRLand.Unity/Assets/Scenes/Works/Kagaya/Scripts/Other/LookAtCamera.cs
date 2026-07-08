using UnityEngine;

public class LookAtCamera : MonoBehaviour {
    [SerializeField] private bool isReverse;

    private void Update() {
        if (Camera.main == null) return;

        if (isReverse) {
            Vector3 dir = transform.position - Camera.main.transform.position;
            this.transform.LookAt(transform.position + dir);
        }
        else {
            this.transform.LookAt(Camera.main.transform);
        }
    }
}
