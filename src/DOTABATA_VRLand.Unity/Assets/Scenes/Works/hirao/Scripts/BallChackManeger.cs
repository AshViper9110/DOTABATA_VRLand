using UnityEngine;

public class BallChackManeger : MonoBehaviour
{
    [SerializeField] private HomeRunRush homeRunRush;
    [SerializeField] private Transform homeBaseTransform;

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("projectile"))
            return;

        Vector3 ballPosition = collision.gameObject.transform.position;

        // ホームベースからボールへの方向
        Vector3 direction = ballPosition - homeBaseTransform.position;

        // ホームベースの前方向との内積
        float forwardDistance = Vector3.Dot(
            direction,
            homeBaseTransform.forward
        );

        // ホームベースより後ろなら0、それ以外なら距離を使用
        float distance = forwardDistance < 0f
            ? 0f
            : direction.magnitude;

        homeRunRush.AddScore(distance);
    }
}