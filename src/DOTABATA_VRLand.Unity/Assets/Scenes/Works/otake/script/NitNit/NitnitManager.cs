using UnityEngine;
using Valve.VR.InteractionSystem;

public class NitnitManager : MonoBehaviour
{
    float MaxPoint = 999;
    float point;
    float tempPoint = 0;
    [SerializeField] GameObject RightRod;
    [SerializeField] GameObject LeftRod;
    Interactable RightInteractable;
    Interactable LeftInteractable;
    Vector3 TempRightPos;
    Vector3 TempLeftPos;

    [SerializeField] GameObject nitPrefab;
    [SerializeField] Transform nitsParent;
    public float distans;
    public int nitCount;
     // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        TempRightPos = RightRod.transform.position;
        TempLeftPos = LeftRod.transform.position;
        RightInteractable = RightRod.GetComponent<Interactable>();
        LeftInteractable = LeftRod.GetComponent<Interactable>();
        point = 0;
        nitCount = 0;
        tempPoint = 0;


    }

    // Update is called once per frame
    void Update()
    {
        if(!RightInteractable.attachedToHand || !LeftInteractable.attachedToHand) { return; }
        Vector3 RightVector = (RightRod.transform.position - TempRightPos);
        Vector3 LeftVector = (LeftRod.transform.position - TempLeftPos);



        float sqrtRight = Mathf.Sqrt(Mathf.Abs(RightVector.y));
        sqrtRight = Mathf.Floor(sqrtRight * 1000) / 1000;
        float sqrtLeft = Mathf.Sqrt(Mathf.Abs(LeftVector.y));
        sqrtLeft = Mathf.Floor(sqrtLeft * 1000) / 1000;

        float temp = Mathf.Abs(sqrtRight + sqrtLeft);
        temp = Mathf.Floor(temp*100)/100;
       
        //Debug.Log(temp);
         
        point += temp;
        if( point > MaxPoint )
        {
            point = MaxPoint;
        }
       
        Debug.Log(point);

        if(point-tempPoint >= 1)
        {
            
            addNit();
            tempPoint = point;
        }
        else
        {
           
        }
        


        TempRightPos = RightRod.transform.position;
        TempLeftPos = LeftRod.transform.position;
    }

    public void addNit()
    {
        GameObject nit = Instantiate(nitPrefab,nitsParent);
        nit.transform.position = new Vector3(nit.transform.position.x + (distans * nitCount), nit.transform.position.y, nit.transform.position.z);
        nitsParent.position = new Vector3(nitsParent.transform.position.x - (distans), nitsParent.transform.position.y, nitsParent.gameObject.transform.position.z);
        nitCount++;
    }
}
