using System.Collections;
using UnityEngine;

public class Cutter : MonoBehaviour
{
    private Vector3 previousPosition;

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
        MeshFilter mf = other.GetComponent<MeshFilter>();
        if (mf == null)
            return;

        Cuttable cut = other.GetComponent<Cuttable>();
        if (cut == null)
            return;

        // クールダウン中なら切断しない
        if (Time.time < cut.nextCutTime)
            return;

        Vector3 planePoint =
                 other.ClosestPoint(transform.position);


        Vector3 moveDir = transform.position - previousPosition;

        if (moveDir.sqrMagnitude < 0.0001f)
            return;

        Vector3 bladeDirection = transform.up;

        // 切断面の法線
        Vector3 planeNormal = Vector3.Cross(moveDir.normalized, bladeDirection).normalized;


        var (fragment, original) = MeshCut.CutMesh(
            other.gameObject,
            planePoint,
            planeNormal,
            true,
            null);

        if (fragment == null || original == null)
            return;

        // 次に切断できる時刻を設定
        float nextTime = Time.time + cut.coolTime;

        Cuttable originalCut = original.GetComponent<Cuttable>();
        if (originalCut != null)
        {
            originalCut.nextCutTime = nextTime;
        }

        Cuttable fragmentCut = fragment.GetComponent<Cuttable>();
        if (fragmentCut != null)
        {
            fragmentCut.nextCutTime = nextTime;
        }

        // Colliderを1物理フレームだけ無効化
        StartCoroutine(EnableColliderNextFrame(original));
        StartCoroutine(EnableColliderNextFrame(fragment));
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
}