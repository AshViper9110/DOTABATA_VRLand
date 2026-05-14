using UnityEngine;

public class CrownManager : MonoBehaviour
{

    public float MaxRotateSpeed;
    public float rotateSpeed;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rotateSpeed = Random.Range(0.01f,MaxRotateSpeed);
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.Rotate(0,rotateSpeed,0);
    }
}
