using UnityEngine;

public class BaseBat : MonoBehaviour
{
    [Header("Batting Settings")]
    [SerializeField] private float hitPower = 1.5f;
    [SerializeField] private float batPower = 0.5f;

    private Vector3 previousPosition;
    private Vector3 batVelocity;

    private void Start()
    {
        previousPosition = transform.position;
    }

    private void Update()
    {
        // バットの移動速度を取得
        batVelocity = (transform.position - previousPosition) / Time.deltaTime;

        previousPosition = transform.position;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("projectile"))
            return;

        Rigidbody ballRb = collision.gameObject.GetComponent<Rigidbody>();

        if (ballRb == null)
            return;

        // ボールの現在速度
        Vector3 ballVelocity = ballRb.linearVelocity;

        // バットの向いている方向
        Vector3 batDirection = transform.forward;

        // ボールの速度をバット方向へ反射
        Vector3 reflectedDirection =
            Vector3.Reflect(ballVelocity.normalized, batDirection);

        // ボールの速度を基準に打球速度を決定
        float hitSpeed = ballVelocity.magnitude * hitPower;

        // バット自身の速度も加える
        Vector3 finalVelocity =
            reflectedDirection * hitSpeed +
            batVelocity * batPower;

        // ボールを飛ばす
        ballRb.linearVelocity = finalVelocity;
    }
}
