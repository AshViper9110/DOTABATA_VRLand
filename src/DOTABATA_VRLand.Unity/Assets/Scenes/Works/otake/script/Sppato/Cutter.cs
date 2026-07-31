using System.Collections;
using Unity.Multiplayer.Center.NetcodeForGameObjectsExample.DistributedAuthority;
using UnityEngine;

public class Cutter : MonoBehaviour
{
    private Vector3 previousPosition;
    public bool CutOk = false;

    public int cutCount = 0;
    public int maxCutCount = 100;

    void Start()
    {
        previousPosition = transform.position;
    }

    void LateUpdate()
    {
        previousPosition = transform.position;
    }



    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "CutOk")
        { CutOk = true; }

        MeshFilter mf = other.GetComponent<MeshFilter>();
        if (mf == null)
            return;

        Cuttable cut = other.GetComponent<Cuttable>();
        if (cut == null)
            return;

        // 追加：切断回数制限
        if (cut.cutCount >= cut.maxCutCount)
            return;
        if(cutCount >= maxCutCount)
            return ;


        // クールダウン中なら切断しない
        if (Time.time < cut.nextCutTime)
            return;

        //正しい方向から切れていない
        if(!CutOk)
            return;

        Vector3 planePoint =
                 other.ClosestPoint(transform.position);


        Vector3 moveDir = transform.position - previousPosition;

        if (moveDir.sqrMagnitude < 0.0001f)
            return;

        Vector3 bladeDirection = transform.up;

        // 切断面の法線
        Vector3 planeNormal = Vector3.Cross(moveDir.normalized, bladeDirection).normalized;

        //TODO:ここで斬った通知送ってみたい

    }

   
}