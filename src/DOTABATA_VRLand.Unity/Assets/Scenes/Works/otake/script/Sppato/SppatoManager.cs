using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SppatoManager : MonoBehaviour
{

    public static List<GameObject> MineFragments = new List<GameObject>();

    [SerializeField] Transform setpos;
    [SerializeField] GameObject prefab;
    [SerializeField] Cutter Cutter;

    [SerializeField] List<GameObject> FoodPrefabs;

    private void OnEnable()
    {
        if (RoomModel.I == null) return;
        RoomModel.I.OnCutingFood += CutFood;
   
    }

    private void OnDisable()
    {
        if (RoomModel.I == null) return;
        RoomModel.I.OnCutingFood -= CutFood;

    }

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

        int index = UnityEngine.Random.Range(0, FoodPrefabs.Count);

        GameObject food =Å@Instantiate(FoodPrefabs[index],setpos.position,Quaternion.identity);
        
        Cutter.CutOk = false;
        Cutter.cutCount = 0;
        SppatoManager.Register(food);

    }

    public void  SetObject(int index)
    {

    }

    public void CutFood(Guid ID, Vector3 planePoint, Vector3 planeNormal)
    {
        


        //MeshFilter mf = other.GetComponent<MeshFilter>();
        //if (mf == null)
        //    return;

        //Cuttable cut = other.GetComponent<Cuttable>();
        //if (cut == null)
        //    return;

        //var (fragment, original) = MeshCut.CutMesh(
        //    other.gameObject,
        //    planePoint,
        //    planeNormal,
        //    true,
        //    cut.cutMaterial);

        //if (fragment == null || original == null)
        //    return;

        //cut.cutCount++;
        //cutCount++;



        //// éüÇ…êÿífÇ≈Ç´ÇÈéûçèÇê›íË 
        //float nextTime = Time.time + cut.coolTime;

        //Cuttable originalCut = original.GetComponent<Cuttable>();
        //if (originalCut != null)
        //{
        //    originalCut.nextCutTime = nextTime;
        //    originalCut.cutCount = cut.cutCount;
        //    originalCut.cutMaterial = cut.cutMaterial;

        //    if (originalCut.arrow != null)
        //    {
        //        Destroy(originalCut.arrow);
        //    }
        //}

        //Cuttable fragmentCut = fragment.GetComponent<Cuttable>();
        //if (fragmentCut != null)
        //{
        //    fragmentCut.nextCutTime = nextTime;
        //    fragmentCut.cutCount = cut.cutCount;
        //    fragmentCut.cutMaterial = cut.cutMaterial;
        //    if (fragmentCut.arrow != null)
        //    {
        //        Destroy(fragmentCut.arrow);
        //    }
        //}

        //SppatoManager.Register(original);
        //SppatoManager.Register(fragment);

        //// ColliderÇ1ï®óùÉtÉåÅ[ÉÄÇæÇØñ≥å¯âª
        //StartCoroutine(EnableColliderNextFrame(original));
        //StartCoroutine(EnableColliderNextFrame(fragment));
    }

    IEnumerator EnableColliderNextFrame(GameObject obj)
    {
        if (obj == null)
            yield break;

        Collider col = obj.GetComponent<Collider>();

        if (col == null)
            yield break;

        col.enabled = false;

        yield return new WaitForFixedUpdate();

        if (col != null)
            col.enabled = true;
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
