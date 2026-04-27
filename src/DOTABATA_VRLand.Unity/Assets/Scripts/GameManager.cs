using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
   public List<SceneAsset> miniGames = new List<SceneAsset>();

    //ミニゲームのUI配置関係
    public float radius;
    [SerializeField] GameObject MinigamePrefab;
    [SerializeField] GameObject CenterObj;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SelectMiniGame()
    {
        int Max = miniGames.Count;
        int selectNum = Random.Range(0, Max);

        //ここで演出した後シーン遷移

        MoveScene(miniGames[selectNum]);
    }

    public void MoveScene(SceneAsset scene)
    {
        Initiate.Fade(scene.name, Color.black, 1.0f);
    }

    public void SetMiniGame()
    {
        int count = miniGames.Count;
        for (int i = 0; i < count; i++)
        {
            // 角度を計算
            float angle = i * Mathf.PI * 2 / count;

            // 位置を計算 (X, Z平面)
            Vector3 pos = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;

            // 生成して回転を適用
            GameObject obj = Instantiate(MinigamePrefab, CenterObj.transform.position + pos, Quaternion.identity);
            obj.transform.parent = this.transform; // 管理しやすいように親に設定

            // 中心を向かせる場合
            obj.transform.LookAt(transform.position);
        }
    }
}
