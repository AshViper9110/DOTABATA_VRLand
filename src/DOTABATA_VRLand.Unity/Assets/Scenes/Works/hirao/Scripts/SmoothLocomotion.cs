using Unity.VisualScripting;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class SmoothLocomotion : MonoBehaviour
{
    public Transform bodyCollider;

    // 左スティック移動
    public SteamVR_Action_Vector2 walkAction;

    public float walkSpeed = 2.0f;


    [SerializeField]
    private LayerMask obstacleMask;

    [SerializeField] private Rigidbody rb;

    private void Start() {
        // HMDの初期位置を原点に合わせる
        Vector3 pos = transform.position;

        pos.x -= Player.instance.hmdTransform.localPosition.x;
        pos.z -= Player.instance.hmdTransform.localPosition.z;

        transform.position = pos;
    }

    //private void FixedUpdate()
    //{
    //    Vector2 input = walkAction.axis;

    //    if (input.sqrMagnitude < 0.01f)
    //        return;

    //    // HMDの向きだけ取得（上下は無視）
    //    Vector3 forward = Player.instance.hmdTransform.forward;
    //    forward.y = 0;
    //    forward.Normalize();

    //    Vector3 right = Player.instance.hmdTransform.right;
    //    right.y = 0;
    //    right.Normalize();

    //    Vector3 move = (right * input.x + forward * input.y) *
    //                   walkSpeed * Time.fixedDeltaTime;


    //    //transform.position += move;
    // 

    //    // rb.MovePosition(rb.position + move);
    //    rb.linearVelocity = Vector3.zero;
    //    rb.angularVelocity = Vector3.zero;

    //}


    private void FixedUpdate()
    {
        Vector2 input =
    walkAction.GetAxis(SteamVR_Input_Sources.LeftHand);

        if (input.sqrMagnitude < 0.01f)
            return;

        // HMDの水平向き
        Vector3 forward = Player.instance.hmdTransform.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude < 0.0001f)
            return;

        forward.Normalize();

        Vector3 right = Player.instance.hmdTransform.right;
        right.y = 0f;
        right.Normalize();

        Vector3 move =
            (right * input.x + forward * input.y) *
            walkSpeed *
            Time.fixedDeltaTime;

        move.y = 0f;

        if (move.sqrMagnitude < 0.000001f)
            return;


        CapsuleCollider col =
            bodyCollider.GetComponent<CapsuleCollider>();

        Vector3 scale = col.transform.lossyScale;

        float radius =
            col.radius * Mathf.Max(scale.x, scale.z);

        float height =
            Mathf.Max(col.height * scale.y, radius * 2f);

        float halfHeight =
            height * 0.5f - radius;

        float skinWidth = 0.005f;


        // =========================================
        // Colliderの現在の中心
        // =========================================

        Vector3 startCenter =
            col.transform.TransformPoint(col.center);

        Vector3 center = startCenter;


        // =========================================
        // 壁との衝突処理
        // =========================================

        for (int i = 0; i < 3; i++)
        {
            if (move.sqrMagnitude < 0.000001f)
                break;

            Vector3 point1 =
                center + Vector3.up * halfHeight;

            Vector3 point2 =
                center - Vector3.up * halfHeight;

            Vector3 direction =
                move.normalized;

            float distance =
                move.magnitude;


            RaycastHit[] hits = Physics.CapsuleCastAll(
     point1,
     point2,
     radius,
     direction,
     distance + skinWidth,
     Physics.DefaultRaycastLayers,
     QueryTriggerInteraction.Ignore
 );

            RaycastHit? nearestHit = null;
            float nearestDistance = float.MaxValue;

            foreach (RaycastHit h in hits)
            {
                // 自分自身は無視
                if (IsOwnCollider(h.collider))
                    continue;

                if (h.distance < nearestDistance)
                {
                    nearestDistance = h.distance;
                    nearestHit = h;
                }
            }

            if (nearestHit.HasValue)
            {
                RaycastHit hit = nearestHit.Value;

                float safeDistance =
                    Mathf.Max(0f, hit.distance - skinWidth);

                Vector3 moveToWall =
                    direction * safeDistance;

                center += moveToWall;

                Vector3 remainingMove =
                    move - moveToWall;

                move = Vector3.ProjectOnPlane(
                    remainingMove,
                    hit.normal
                );

                move.y = 0f;
            }
            else
            {
                center += move;
                break;
            }
        }


        // =========================================
        // Collider中心の移動量
        // =========================================

        Vector3 delta =
            center - startCenter;


        // =========================================
        // Rigidbodyを移動
        // =========================================

        rb.MovePosition(
            rb.position + delta
        );

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    private bool IsOwnCollider(Collider col)
    {
        return col.transform == transform ||
               col.transform.IsChildOf(transform);
    }

    //void Start()
    //{
    //    Vector3 pos = transform.position;

    //    pos.x -= Player.instance.hmdTransform.localPosition.x;
    //    pos.z -= Player.instance.hmdTransform.localPosition.z;
    //    pos.y = transform.position.y;

    //    transform.position = pos;

    //    Vector3 colpos = bodyCollider.transform.position;

    //    colpos.x = Player.instance.hmdTransform.position.x;
    //    colpos.z = Player.instance.hmdTransform.position.z;

    //    bodyCollider.transform.position = colpos;
    //}

    //void LateUpdate()
    //{

    //    return;
    //    Vector3 player_pos = transform.position;

    //    player_pos.x -=
    //        Player.instance.hmdTransform.position.x
    //        - bodyCollider.transform.position.x;

    //    player_pos.z -=
    //        Player.instance.hmdTransform.position.z
    //        - bodyCollider.transform.position.z;

    //    player_pos.y = bodyCollider.transform.position.y;

    //    transform.position = player_pos;
    //}

    //void FixedUpdate()
    //{
    //    Vector3 player_pos = transform.position;
    //    Vector3 body_pos = bodyCollider.transform.position;

    //    // body位置同期
    //    body_pos.x = Player.instance.hmdTransform.position.x;
    //    body_pos.z = Player.instance.hmdTransform.position.z;

    //    bodyCollider.transform.position = body_pos;

    //    // 左スティック移動
    //    Vector2 moveInput = walkAction.axis;

    //    Vector3 direction =
    //        Player.instance.hmdTransform.TransformDirection(
    //            new Vector3(moveInput.x, 0, moveInput.y)
    //        );

    //    player_pos.x +=
    //        walkSpeed * Time.deltaTime * direction.x;

    //    player_pos.z +=
    //        walkSpeed * Time.deltaTime * direction.z;

    //    // 高さ固定
    //    player_pos.y = bodyCollider.transform.position.y;

    //    transform.position = player_pos;
    //}
}