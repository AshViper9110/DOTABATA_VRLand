using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void LateUpdate()
    {
        Vector3 euler = cam.transform.eulerAngles;
        transform.rotation = Quaternion.Euler(0f, euler.y, 0f);
    }
}