using UnityEngine;

public class SelPointManager : MonoBehaviour
{
    public int SelectId;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        SelectId = other.GetComponent<MiniGameObjManager>().ID;
     
    }
}
