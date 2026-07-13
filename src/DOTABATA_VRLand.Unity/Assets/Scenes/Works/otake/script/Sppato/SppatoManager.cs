using System.Collections.Generic;
using UnityEngine;

public class SppatoManager : MonoBehaviour
{

    public static List<GameObject> MineFragments = new List<GameObject>();

    [SerializeField] Transform setpos;
    [SerializeField] GameObject prefab;
    [SerializeField] Cutter Cutter;

    [SerializeField] List<GameObject> FoodPrefabs;

    public static void Register(GameObject obj)
    {
        MineFragments.Add(obj);
    }

    public static void DestroyAll()
    {
        foreach (GameObject obj in MineFragments)
        {
            if (obj != null)
                Destroy(obj);
        }

        MineFragments.Clear();
    }

    public void ResteObject()
    {
        SppatoManager.DestroyAll();

        int index = Random.Range(0, FoodPrefabs.Count);

        GameObject food =Å@Instantiate(FoodPrefabs[index],setpos.position,Quaternion.identity);
        
        Cutter.CutOk = false;
        Cutter.cutCount = 0;
        SppatoManager.Register(food);

    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
