using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.Multiplayer.Center.NetcodeForGameObjectsExample.DistributedAuthority;
using UnityEngine;

public class Cutter : MonoBehaviour
{
    private Vector3 previousPosition;
    public bool CutOk = false;

    public int cutCount = 0;
    public int maxCutCount = 10;

    public List<Material> handleMaterials = new List<Material> ();
    [SerializeField] GameObject handle;

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

        SyncObject syncObject = other.GetComponent<SyncObject>();
        if (syncObject == null) return;

        // í«â¡ÅFêÿífâÒêîêßå¿
        if (cut.cutCount >= cut.maxCutCount)
            return;
        if(cutCount >= maxCutCount)
            return ;


        // ÉNÅ[ÉãÉ_ÉEÉìíÜÇ»ÇÁêÿífÇµÇ»Ç¢
        if (Time.time < cut.nextCutTime)
            return;

        //ê≥ÇµÇ¢ï˚å¸Ç©ÇÁêÿÇÍÇƒÇ¢Ç»Ç¢
        if(!CutOk)
            return;

        Vector3 planePoint =
                 other.ClosestPoint(transform.position);


        Vector3 moveDir = transform.position - previousPosition;

        if (moveDir.sqrMagnitude < 0.0001f)
            return;

        Vector3 bladeDirection = transform.up;

        // êÿífñ ÇÃñ@ê¸
        Vector3 planeNormal = Vector3.Cross(moveDir.normalized, bladeDirection).normalized;

        //TODO:Ç±Ç±Ç≈éaÇ¡ÇΩí ímëóÇ¡ÇƒÇ›ÇΩÇ¢
        RoomModel.I.CutFood(syncObject.ObjectId,planePoint,planeNormal);

        cutCount++;

    }

   public void ChengeHandle(int index)
    {
        handle.GetComponent<MeshRenderer>().material = handleMaterials[index];
    }
}