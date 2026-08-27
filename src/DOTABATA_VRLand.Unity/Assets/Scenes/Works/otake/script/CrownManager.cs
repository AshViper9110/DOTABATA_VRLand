using UnityEngine;

public class CrownManager : MonoBehaviour
{

    public float MaxRotateSpeed;
    public float rotateSpeed;

    public bool isNew = false;

    public float fallSpeed;

    public Transform ParentTrans;
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
            this.transform.position = Vector3.MoveTowards(transform.position,ParentTrans.position,fallSpeed);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isNew)
        {
            if (collision.gameObject.layer == 8)
            {
                isNew = false;
          
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (isNew)
        {
            if (other.gameObject.layer == 8)
            {
                isNew = false;
               
            }
        }
    }
}
