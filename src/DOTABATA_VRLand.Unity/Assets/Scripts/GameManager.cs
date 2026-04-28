using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEditor.ShaderData;


public class GameManager : MonoBehaviour
{
   public List<SceneAsset> miniGames = new List<SceneAsset>();

    //É~ÉjÉQÅ[ÉÄÇÃUIîzíuä÷åW
    public float radius;
    [SerializeField] GameObject MinigamePrefab;

    [SerializeField] GameObject CenterObj;
    Rigidbody CenterObjRb;

    [SerializeField] GameObject selectPoint;
    SelPointManager selPointManager;

    bool isSpin;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetMiniGame();
        CenterObjRb = CenterObj.GetComponent<Rigidbody>();
        selPointManager = selectPoint.GetComponent<SelPointManager>();
        isSpin = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isSpin)
        {
            CenterObjRb.angularVelocity = new Vector3(0, 0.3f, 0);
        }
        else if (isSpin)
        {
            if (CenterObjRb.angularVelocity.y < 0.01f)
            {
                Debug.Log(selPointManager.SelectId+"Ç…ÉQÅ[ÉÄÇ™åàÇ‹ÇËÇ‹ÇµÇΩ");
                MoveScene(miniGames[selPointManager.SelectId]);
                enabled = false;
            }
        }
    }

    public void SelectMiniGame()
    {
        isSpin = true;
       float spinPower = Random.Range(3, 6);

       CenterObjRb.angularVelocity = new Vector3(0, spinPower, 0);
    }

    public void MoveScene(SceneAsset scene)
    {
        Initiate.Fade(scene.name, Color.black, 1.0f);
    }

    public void SetMiniGame()
    {
        // äpìxÇåvéZ
        float angle = 3 * Mathf.PI * 2 / 4;

        // à íuÇåvéZ (X, ZïΩñ )
        Vector3 pos = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;
        selectPoint.transform.position = CenterObj.transform.position + pos;

        int count = miniGames.Count;
        for (int i = 0; i < count; i++)
        {
            // äpìxÇåvéZ
             angle = i * Mathf.PI * 2 / count;

            // à íuÇåvéZ (X, ZïΩñ )
             pos = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;

            // ê∂ê¨ÇµÇƒâÒì]ÇìKóp
            GameObject obj = Instantiate(MinigamePrefab, 
                CenterObj.transform.position + pos,
                Quaternion.identity,
                CenterObj.transform);

            MiniGameObjManager manager = obj.GetComponent<MiniGameObjManager>();
            manager.ID = i;

        }
    }
}
