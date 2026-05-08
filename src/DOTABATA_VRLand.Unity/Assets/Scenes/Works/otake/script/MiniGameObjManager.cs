using UnityEngine;
using UnityEngine.UI;

public class MiniGameObjManager : MonoBehaviour
{
    public int ID;
    public RawImage Image;
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
