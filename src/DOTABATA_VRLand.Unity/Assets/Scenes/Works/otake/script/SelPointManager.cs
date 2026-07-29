using UnityEngine;

public class SelPointManager : MonoBehaviour
{
    public string sceneName;
    public string titleName;
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
        sceneName = other.GetComponent<MiniGameObjManager>().sceneName;
        titleName = other.GetComponent<MiniGameObjManager>().titleName;
        Debug.Log("‚ ‚Á‚½ƒ^ƒˆ");
    }
}
