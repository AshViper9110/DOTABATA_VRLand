using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class SmoothLocomotion : MonoBehaviour
{
    public Transform bodyCollider;

    // 左スティック移動
    public SteamVR_Action_Vector2 walkAction;

    public float walkSpeed = 2.0f;

    void Start()
    {
        Vector3 pos = transform.position;

        pos.x -= Player.instance.hmdTransform.localPosition.x;
        pos.z -= Player.instance.hmdTransform.localPosition.z;
        pos.y = transform.position.y;

        transform.position = pos;

        Vector3 colpos = bodyCollider.transform.position;

        colpos.x = Player.instance.hmdTransform.position.x;
        colpos.z = Player.instance.hmdTransform.position.z;

        bodyCollider.transform.position = colpos;
    }

    void LateUpdate()
    {
        Vector3 player_pos = transform.position;

        player_pos.x -=
            Player.instance.hmdTransform.position.x
            - bodyCollider.transform.position.x;

        player_pos.z -=
            Player.instance.hmdTransform.position.z
            - bodyCollider.transform.position.z;

        player_pos.y = bodyCollider.transform.position.y;

        transform.position = player_pos;
    }

    void FixedUpdate()
    {
        Vector3 player_pos = transform.position;
        Vector3 body_pos = bodyCollider.transform.position;

        // body位置同期
        body_pos.x = Player.instance.hmdTransform.position.x;
        body_pos.z = Player.instance.hmdTransform.position.z;

        bodyCollider.transform.position = body_pos;

        // 左スティック移動
        Vector2 moveInput = walkAction.axis;

        Vector3 direction =
            Player.instance.hmdTransform.TransformDirection(
                new Vector3(moveInput.x, 0, moveInput.y)
            );

        player_pos.x +=
            walkSpeed * Time.deltaTime * direction.x;

        player_pos.z +=
            walkSpeed * Time.deltaTime * direction.z;

        // 高さ固定
        player_pos.y = bodyCollider.transform.position.y;

        transform.position = player_pos;
    }
}