using UnityEngine;

public class MiniGameObjManager : MonoBehaviour
{
    public int ID;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       this.gameObject.transform.rotation = Camera.main.transform.rotation;
    }
}
