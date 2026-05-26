using System.Collections.Generic;
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

    [SerializeField] List<GameObject> nitPrefabs = new List<GameObject>();
    [SerializeField] List<Material> materials = new List<Material>();
    int nitIndex;
    int indexVector;
    [SerializeField] Transform nitsParent;
    public float distans;
    public int nitCount;
    public int nitLate;//êLÇ—ó¶
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
        nitIndex = 0;
        indexVector = 1;

    }

    // Update is called once per frame
    void Update()
    {
        if(!RightInteractable.attachedToHand || !LeftInteractable.attachedToHand) { return; }
        Vector3 RightVector = (RightRod.transform.position - TempRightPos);
        Vector3 LeftVector = (LeftRod.transform.position - TempLeftPos);



        float sqrtRight = Mathf.Sqrt(Mathf.Abs(RightVector.y));
        sqrtRight = Mathf.Floor(sqrtRight * 10) / 10;
        float sqrtLeft = Mathf.Sqrt(Mathf.Abs(LeftVector.y));
        sqrtLeft = Mathf.Floor(sqrtLeft * 10) / 10;

        float temp = Mathf.Abs(sqrtRight + sqrtLeft);
        temp = Mathf.Floor(temp*10)/10;
       
        temp = temp * nitLate;        //Debug.Log(temp);
         
        point += temp;
       
        if ( point > MaxPoint )
        {
            point = MaxPoint;
        }
       
       

        if (Mathf.Floor(point -tempPoint) >= 1)
        {
           // Debug.Log(Mathf.Floor(point - tempPoint));
            for (int i = 0; i < (int)(point - tempPoint); i++)
            {
                addNit();
            }
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
        GameObject nit = Instantiate(nitPrefabs[nitIndex],nitsParent);
        nit.transform.position = new Vector3(nit.transform.position.x + (distans * nitCount), nit.transform.position.y, nit.transform.position.z);
        if(indexVector == -1)
        {
            nit.transform.Rotate(0, 180, 0);
            nit.GetComponent<MeshRenderer>().material = materials[1];
        }
        nitsParent.position = new Vector3(nitsParent.transform.position.x - (distans), nitsParent.transform.position.y, nitsParent.gameObject.transform.position.z);
        nitCount++;
        nitIndex += indexVector;

        Debug.Log(nitIndex);
        if (nitIndex >= nitPrefabs.Count)
        {
           indexVector = -indexVector;
            nitIndex = nitPrefabs.Count - 1;
        }
        else if(nitIndex < 0)
        {
            indexVector = -indexVector;
            nitIndex = 0;
        }
    }
}
