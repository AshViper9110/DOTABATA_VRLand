using UnityEngine;
using UnityEngine.InputSystem;

public class HomeRunRush : MonoBehaviour
{
    [SerializeField] private MinigameFlowController controller;
    [SerializeField] private GameObject ballPrefab;

    [SerializeField] private float shotPower = 10f;
    [SerializeField] private float shotHight = 5f;

    private float shotInterval = 2f;
    private float nextShotTime = 0f;

    private void Update()
    {
        if (Time.time >= nextShotTime)
        {
            ShotBall();

            nextShotTime = Time.time + shotInterval;
        }
    }

    private void ShotBall()
    {
        GameObject ball = Instantiate(
            ballPrefab,
            transform.position,
            transform.rotation
        );

        Rigidbody rb = ball.GetComponent<Rigidbody>();

        rb.linearVelocity =
            transform.forward * shotPower +
            Vector3.up * shotHight;
    }

    private void OnTriggerExit(Collider other)
    {
        // ボール以外は無視
        if (!other.CompareTag("projectile"))
            return;

        Debug.Log("ホームラン！");

        // ホームラン処理
        HomeRun();
    }

    private void HomeRun()
    {
        // ここにホームラン時の処理を書く
        // controller側に処理を作ったらここから呼び出す
    }
}