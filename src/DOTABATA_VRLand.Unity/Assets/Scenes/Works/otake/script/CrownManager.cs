using UnityEngine;

public class CrownManager : MonoBehaviour
{

    public float MaxRotateSpeed;
    public float rotateSpeed;

    public bool isNew = false;

    public float fallSpeed;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rotateSpeed = Random.Range(0.01f,MaxRotateSpeed);
        
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.Rotate(0,rotateSpeed,0);

        if(isNew)
        {
            this.transform.position = new Vector3(transform.position.x,transform.position.y-fallSpeed,transform.position.z);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isNew)
        {
            isNew = false;
            Debug.Log("Ž~‚Ü‚è‚Ü‚·");
        }
    }
}
