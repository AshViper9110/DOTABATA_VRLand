using UnityEngine;

public class ChefHatManager : MonoBehaviour
{
    [SerializeField] GameObject MidPrefab;
    [SerializeField] GameObject parent;
    int index;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        index = 1;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddHatMid(int count)
    {
        for (int i = 0; i < count; i++)
        {
           GameObject mid = Instantiate(MidPrefab,parent.transform);

            mid.transform.position = new Vector3(mid.transform.position.x,
                mid.transform.position.y - ( 0.1f * index),
                mid.transform.position.z);

            parent.transform.position = new Vector3(parent.transform.position.x, parent.transform.position.y +  0.1f, parent.transform.position.z);

            index++;

        }
    }
}
