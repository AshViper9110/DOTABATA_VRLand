using UnityEngine;

public class Ball : MonoBehaviour
{
    [SerializeField] private float hitPower = 1.5f;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        Destroy(gameObject, 10f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Bat"))
            return;

        // バットに当たる直前のボールの速度
        Vector3 velocity = rb.linearVelocity;

        // 飛んできた方向を完全に反転
        Vector3 reverseDirection = -velocity.normalized;

        // 元の速度 × 倍率
        float speed = velocity.magnitude * hitPower;

        rb.linearVelocity = reverseDirection * speed;
    }
}